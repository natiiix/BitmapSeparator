using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace BitmapSeparator
{
    public partial class FormMain : Form
    {
        // Used to fill empty spaces in bitmaps
        private static Color COLOR_INACTIVE_PIXEL = Color.FromArgb(255, 255, 255);
        private static Brush BRUSH_INACTIVE_AREA = new SolidBrush(COLOR_INACTIVE_PIXEL);
        // The range at which active neighbor pixels are detected
        private const int NEIGHBOR_PIXEL_DETECTION_RANGE = 2;
        // The lowest possible area considered to be a valid sub-bitmap
        // Used to prevent undesired small sub-bitmap from appearing
        private static int DIGIT_BOUNDARIES_AREA_THRESHOLD;
        // How much is the sub-bitmap area supposed to be inflated once confirmed to be valid
        private const int DIGIT_AREA_INFLATION_X = 2;
        private const int DIGIT_AREA_INFLATION_Y = 2;
        // Determines the resolution of the output sub-bitmaps
        private static int DIGIT_BITMAP_NORMALIZED_WIDTH;
        private static int DIGIT_BITMAP_NORMALIZED_HEIGHT;
        // If pixel sum value is below this value the pixel is considered to be active
        private static int ACTIVE_PIXEL_THRESHOLD;
        // Used for clamping pixel values during convertion to grayscale
        private static int PIXEL_SUM_CLAMP_LOW;
        private static int PIXEL_SUM_CLAMP_HIGH;

        // Can't use standard Rectangle, because we don't know width and height until all neighbor pixels are scanned
        // It would be inefficient to re-compute the Rectangle with each neighbor over and over
        public struct BoundsHolder
        {
            public int Left;
            public int Right;
            public int Top;
            public int Bottom;

            // Convert the BoundsHolder into a rectangle describing the same area
            public Rectangle ToRectangle()
            {
                return new Rectangle(Left, Top, Right - Left + 1, Bottom - Top + 1);
            }
        }

        // Form initialization
        public FormMain()
        {
            InitializeComponent();
        }

        // Form has loaded
        private void FormMain_Load(object sender, EventArgs e)
        {
            // Load default element toggle settings
            formElementsToggle(true, false, true);

            // Update processing settings to their initial values
            trackBarDigitBoundariesAreaThreshold_ValueChanged(this, new EventArgs());
            textBoxDigitBitmapNormalizedWidth_TextChanged(this, new EventArgs());
            textBoxDigitBitmapNormalizedHeight_TextChanged(this, new EventArgs());

            trackBarActivePixelThreshold_ValueChanged(this, new EventArgs());
            textBoxPixelSumClampLow_TextChanged(this, new EventArgs());
            textBoxPixelSumClampHigh_TextChanged(this, new EventArgs());
        }

        // Update bitmap processing settings and their labels
        private void trackBarDigitBoundariesAreaThreshold_ValueChanged(object sender, EventArgs e)
        {
            DIGIT_BOUNDARIES_AREA_THRESHOLD = trackBarDigitBoundariesAreaThreshold.Value;
            labelDigitBoundariesAreaThreshold.Text = "Digit Boundaries Area Threshold: " + DIGIT_BOUNDARIES_AREA_THRESHOLD.ToString();
        }

        private void textBoxDigitBitmapNormalizedWidth_TextChanged(object sender, EventArgs e)
        {
            int.TryParse(textBoxDigitBitmapNormalizedWidth.Text, out DIGIT_BITMAP_NORMALIZED_WIDTH);
        }

        private void textBoxDigitBitmapNormalizedHeight_TextChanged(object sender, EventArgs e)
        {
            int.TryParse(textBoxDigitBitmapNormalizedHeight.Text, out DIGIT_BITMAP_NORMALIZED_HEIGHT);
        }

        private void trackBarActivePixelThreshold_ValueChanged(object sender, EventArgs e)
        {
            ACTIVE_PIXEL_THRESHOLD = trackBarActivePixelThreshold.Value;
            labelActivePixelThreshold.Text = "Active Pixel Threshold: " + ACTIVE_PIXEL_THRESHOLD.ToString();
        }

        private void textBoxPixelSumClampLow_TextChanged(object sender, EventArgs e)
        {
            int.TryParse(textBoxPixelSumClampLow.Text, out PIXEL_SUM_CLAMP_LOW);
        }

        private void textBoxPixelSumClampHigh_TextChanged(object sender, EventArgs e)
        {
            int.TryParse(textBoxPixelSumClampHigh.Text, out PIXEL_SUM_CLAMP_HIGH);
        }

        // User wants to select a bitmap via openFileDialog
        private void buttonLoadBitmap_Click(object sender, EventArgs e)
        {
            openFileDialogBitmap.ShowDialog();
        }

        // Source bitmap file has been selected
        private void openFileDialogBitmap_FileOk(object sender, CancelEventArgs e)
        {
            // Enable the process button
            formElementsToggle(true, true, true);
        }

        // User requeted processing of the selected bitmap
        private void buttonProcess_Click(object sender, EventArgs e)
        {
            processBitmap(openFileDialogBitmap.FileName);
        }

        // Loads the file selected using openFileDialong
        private void processBitmap(string strBitmapPath)
        {
            // Disable form elements
            formElementsToggle(false, false, false);
            // Re-draw the form to make sure the user sees
            // all the setting elements as disabled
            Application.DoEvents();

            // Load raw bitmap from file
            using (Bitmap bmpSouceRaw = new Bitmap(strBitmapPath))
            {
                // Normalize input bitmap into grayscale format with highest possible contrast
                using (Bitmap bmpSourceNormalized = bmpToGrayscale(bmpSouceRaw))
                {
                    // Save the original bitmap in BMP format
                    // Solely for debugging purposes, to make sure all the bitmap files are
                    // in the executable directory in case the original bitmap is lost
                    bmpSouceRaw.Save("source.bmp", ImageFormat.Bmp);

                    // Save the normalized version of the input bitmap
                    // For debugging purposes
                    bmpSourceNormalized.Save("normalized.bmp", ImageFormat.Bmp);

                    // Create the discovered digit counter
                    // Used to generate unique file name for each sub-bitmap
                    int digitCount = 0;

                    // Since the progress bar updates with every row
                    // it's maximum has to be set to the highest value
                    // it can each, which is the height of the bitmap - 1
                    progressBarBitmapProcessing.Maximum = bmpSourceNormalized.Height - 1;

                    // Go through all the pixels row by row and attempt to find all sub-bitmaps
                    for (int y = 0; y < bmpSourceNormalized.Height; y++)
                    {
                        for (int x = 0; x < bmpSourceNormalized.Width; x++)
                            // If there's an active pixel - dark one simply put
                            if (isActive(bmpSourceNormalized.GetPixel(x, y)))
                            {
                                // Get the boundaries that encapsulate the sub-bitmap the program has "bumped" into
                                Rectangle rectBounds = getBounds(bmpSourceNormalized, x, y);

                                // Found boundaries must fulfill the requested area threshold
                                // Smaller area would be unlikely to contain any comprehensive digit
                                if (rectBounds.Width * rectBounds.Height > DIGIT_BOUNDARIES_AREA_THRESHOLD)
                                {
                                    // Inflate the boundaries slightly to make sure we've got the whole digit
                                    rectBounds.Inflate(DIGIT_AREA_INFLATION_X, DIGIT_AREA_INFLATION_Y);

                                    // Each sub-bitmap is re-scaled and converted to grayscale to fit the algorithm as well as possible
                                    // That means all the output sub-bitmaps have the same resolution and highest possible contrast
                                    using (Bitmap bmpDigit = bmpSourceNormalized.Clone(rectBounds, bmpSourceNormalized.PixelFormat))
                                        normalizeDigitBitmap(bmpDigit).Save("digit" + digitCount++.ToString("0000") + ".bmp", ImageFormat.Bmp);

                                    // Remove the recently discovered sub-bitmap from the source bitmap
                                    // to make sure it doesn't get re-discovered accidentally
                                    using (Graphics g = Graphics.FromImage(bmpSourceNormalized))
                                        g.FillRectangle(BRUSH_INACTIVE_AREA, rectBounds);
                                }
                            }

                        // Update the progress bar
                        progressBarBitmapProcessing.Value = y;
                        // Make sure the form keeps re-drawing so that the user
                        // can observe the progress bar as it updates
                        // It seems that it doesn't even have to be here,
                        // but I kind of want to make sure it works properly
                        Application.DoEvents();
                    }

                    // Save the final version of the input bitmap
                    // This exists mostly for the purposes of debugging as there
                    // is no real use to this bitmap after the process is done
                    // It should be ideally completely empty anyways
                    bmpSourceNormalized.Save("final.bmp", ImageFormat.Bmp);
                }
            }

            // Reset the progress bar
            progressBarBitmapProcessing.Value = 0;

            // Re-enable form elements
            formElementsToggle(true, true, true);
        }

        // Decides whether a pixel is active or not
        // Dark pixels are considered to be active while bright pixels are supposedly inactive
        private static bool isActive(Color c)
        {
            return (c.R + c.G + c.B <= ACTIVE_PIXEL_THRESHOLD);
        }

        // Get boundaries for a sub-bitmap that neighbors with x, y pixel
        private static Rectangle getBounds(Bitmap bmp, int x, int y)
        {
            BoundsHolder bounds = new BoundsHolder();
            // This matrix stores information about whether a pixel has been already visited by the followNeighbors function
            // This is used to make sure the program doesn't start running in an infinite loop
            bool[,] visited = new bool[bmp.Width, bmp.Height];

            // We know for sure that x, y are in fact inside the boudaries by definition
            bounds.Left = bounds.Right = x;
            bounds.Top = bounds.Bottom = y;

            // Start following neighbor pixels to determine the boundaries of this sub-bitmap
            followNeighbors(bmp, ref bounds, ref visited, x, y);

            // Return the boudaries as a rectangle
            return bounds.ToRectangle();
        }

        // Sets valSet to valCompare if valCompare is higher than valSet
        private static void setIfHigher(ref int valSet, int valCompare)
        {
            if (valCompare > valSet)
                valSet = valCompare;
        }

        // Sets valSet to valCompare if valCompare is lower than valSet
        private static void setIfLower(ref int valSet, int valCompare)
        {
            if (valCompare < valSet)
                valSet = valCompare;
        }

        // Follows neighbor pixels in order to find the whole boundaries for the currently processed sub-bitmap
        private static void followNeighbors(Bitmap bmp, ref BoundsHolder bounds, ref bool[,] visited, int x, int y)
        {
            // Pixel x, y has been visited and shall not be visited again
            visited[x, y] = true;
            // Get all the active neighbor pixels within specified range
            List<Point> activeNeighbors = getActiveNeighbors(bmp, x, y);

            // Update boundaries
            setIfLower(ref bounds.Left, x);
            setIfLower(ref bounds.Top, y);
            setIfHigher(ref bounds.Right, x);
            setIfHigher(ref bounds.Bottom, y);

            // Process each discovered active neighbor
            while (activeNeighbors.Count > 0)
            {
                Point pNeighbor = activeNeighbors.Last();

                if (!visited[pNeighbor.X, pNeighbor.Y])
                    followNeighbors(bmp, ref bounds, ref visited, pNeighbor.X, pNeighbor.Y);

                activeNeighbors.RemoveAt(activeNeighbors.Count - 1);
            }
        }

        // Find all the active neighbor pixels within a specified range of pixel x, y
        private static List<Point> getActiveNeighbors(Bitmap bmp, int x, int y)
        {
            List<Point> points = new List<Point>();

            for (int iy = y - NEIGHBOR_PIXEL_DETECTION_RANGE; iy <= y + NEIGHBOR_PIXEL_DETECTION_RANGE; iy++)
                for (int ix = x - NEIGHBOR_PIXEL_DETECTION_RANGE; ix <= x + NEIGHBOR_PIXEL_DETECTION_RANGE; ix++)
                    // Only test pixels within the boudaries of the input bitmap
                    // Pixel x, y itself isn't supposed to be tested either
                    if (ix >= 0 && ix < bmp.Width && iy >= 0 && iy < bmp.Height &&
                        (ix != x || iy != y) && isActive(bmp.GetPixel(ix, iy)))
                        points.Add(new Point(ix, iy));

            return points;
        }

        // To make the work for the AI algorithm easier all sub-bitmaps are scaled
        // to the same resolution and converted to grayscale using this function
        private static Bitmap normalizeDigitBitmap(Bitmap bmpInput)
        {
            // Create a bitmap to store the final sub-bitmap with normalized resolution
            Bitmap bmpNormalized = new Bitmap(DIGIT_BITMAP_NORMALIZED_WIDTH, DIGIT_BITMAP_NORMALIZED_HEIGHT);
            // We need to make sure there are no black bars on sides (if the input bitmap has different aspect ration than the output)
            using (Graphics g = Graphics.FromImage(bmpNormalized))
                g.FillRectangle(new SolidBrush(COLOR_INACTIVE_PIXEL), new Rectangle(0, 0, bmpNormalized.Width, bmpNormalized.Height));

            // Caculate the aspect ratio of the normalized output bitmap
            float normalizedAspectRatio = (float)DIGIT_BITMAP_NORMALIZED_WIDTH / (float)DIGIT_BITMAP_NORMALIZED_HEIGHT;
            // Figure out the aspect ratio of input bitmap
            float inputAspectRatio = (float)bmpInput.Width / (float)bmpInput.Height;

            // Scale width and height, but make sure neither one of them exceeds
            // its normalized size while at least one of them meets its normalized size
            int scaledWidth = (int)(inputAspectRatio >= normalizedAspectRatio ? DIGIT_BITMAP_NORMALIZED_WIDTH : DIGIT_BITMAP_NORMALIZED_HEIGHT * inputAspectRatio);
            int scaledHeight = (int)(inputAspectRatio <= normalizedAspectRatio ? DIGIT_BITMAP_NORMALIZED_HEIGHT : DIGIT_BITMAP_NORMALIZED_WIDTH / inputAspectRatio);

            // Used to store the scaled digit bitmap before it's copied into the normalized bitmap
            // It's necessary because the normalized bitmap has white background which would ruin
            // the process of increasing contrast using the normalizeBitmapToGrayscale function
            using (Bitmap bmpScaled = new Bitmap(scaledWidth, scaledHeight))
            {
                // Draw the input bitmap onto the scaled bitmap while maintaining the aspect ration and changing its resolution
                // This is not enough, because it's necessary to keep the aspect ratio, yet achieve the desired normalized resolution
                // which would not be possible if the input sub-bitmap had a different aspect ratio than the ouput one
                using (Graphics g = Graphics.FromImage(bmpScaled))
                {
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.DrawImage(bmpInput, new Rectangle(0, 0, scaledWidth, scaledHeight));
                }

                // Calculate the padding to make sure the scaled bitmap
                // is properly centered in the final normalized bitmap
                int paddingLeft = (DIGIT_BITMAP_NORMALIZED_WIDTH - scaledWidth) / 2;
                int paddingTop = (DIGIT_BITMAP_NORMALIZED_HEIGHT - scaledHeight) / 2;

                // Draw the scaled bitmap onto the final normalized bitmap which has
                // the proper output resolution required by the processing algorithm
                using (Graphics g = Graphics.FromImage(bmpNormalized))
                {
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.DrawImage(bmpToGrayscale(bmpScaled, true), new Rectangle(paddingLeft, paddingTop, bmpScaled.Width, bmpScaled.Height));
                }
            }

            // Return the normalized bitmap with proper resolution converted to grayscale
            return bmpNormalized;
        }

        // Makes sure the bitmap is gray-only and has the higher possible contrast
        // overContrast = pixel values are clamped by specified constants
        private static Bitmap bmpToGrayscale(Bitmap bmpInput, bool overContrast = false)
        {
            // Create bitmap used to store the fully normalized bitmap
            Bitmap bmpNormalized = new Bitmap(bmpInput.Width, bmpInput.Height);

            int maxPixelSum = int.MinValue;
            int minPixelSum = int.MaxValue;

            // Find the highest and lowest pixel value sum
            for (int y = 0; y < bmpInput.Height; y++)
                for (int x = 0; x < bmpInput.Width; x++)
                {
                    int pixelSum = getPixelSum(bmpInput.GetPixel(x, y));

                    setIfHigher(ref maxPixelSum, pixelSum);
                    setIfLower(ref minPixelSum, pixelSum);
                }

            // Figure out the scale required to achieve the highest possible contrast
            float pixelScale = 256.0f / ((maxPixelSum - minPixelSum) / 3.0f);

            // Scale each pixel to make sure the highest possible contrast is obtained
            for (int y = 0; y < bmpInput.Height; y++)
                for (int x = 0; x < bmpInput.Width; x++)
                    // While scaling the pixel values, all the pixels are also converted to grayscale in case they weren't before
                    bmpNormalized.SetPixel(x, y, scaleColorToGrayscale(bmpInput.GetPixel(x, y), pixelScale, minPixelSum, overContrast));

            // Return the grayscale bitmap with maxed-out contrast
            return bmpNormalized;
        }

        // Convert a pixel to grayscale and scale it to maximize the contrast
        private static Color scaleColorToGrayscale(Color colInput, float scale, int minSum, bool overContrast = false)
        {
            // Get the pixel sum for this pixel
            int pixelSum = getPixelSum(colInput);
            // Scale the pixel sum
            float pixelSumScaled = (pixelSum - minSum) * scale;

            // Clamp the pixel sum if activated
            if (overContrast)
            {
                pixelSumScaled -= PIXEL_SUM_CLAMP_LOW;
                pixelSumScaled *= ((256.0f * 3.0f) / (PIXEL_SUM_CLAMP_HIGH - PIXEL_SUM_CLAMP_LOW));
            }

            // Calculate the amount of gray for each channel
            // Since it's grayscale, all channel share the same intensity
            int pixelChannelScaled = (int)(pixelSumScaled / 3.0f);

            // Clamp the channel value
            if (pixelChannelScaled < 0)
                pixelChannelScaled = 0;
            else if (pixelChannelScaled > 255)
                pixelChannelScaled = 255;

            // Return color scaled grayscale pixel
            return Color.FromArgb(255, pixelChannelScaled, pixelChannelScaled, pixelChannelScaled);
        }

        // Return the pixel sum value for a specified pixel color
        private static int getPixelSum(Color col)
        {
            return col.R + col.G + col.B;
        }

        // Enables / disables all modifiable form elements
        // Used to make sure settings don't change during the process
        private void formElementsToggle(bool enableLoad, bool enableProcess, bool enableSettings)
        {
            trackBarDigitBoundariesAreaThreshold.Enabled = enableSettings;
            textBoxDigitBitmapNormalizedWidth.Enabled = enableSettings;
            textBoxDigitBitmapNormalizedHeight.Enabled = enableSettings;

            trackBarActivePixelThreshold.Enabled = enableSettings;
            textBoxPixelSumClampLow.Enabled = enableSettings;
            textBoxPixelSumClampHigh.Enabled = enableSettings;

            // Buttons need to be set lastly to ensure one of them keeps the focus
            buttonLoadBitmap.Enabled = enableLoad;
            buttonProcess.Enabled = enableProcess;
        }
    }
}
