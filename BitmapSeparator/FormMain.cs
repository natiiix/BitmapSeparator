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
        private const int NEIGHBOR_PIXEL_DETECTION_RANGE = 8;
        // The lowest possible area considered to be a valid sub-bitmap
        // Used to prevent undesired small sub-bitmap from appearing
        private const int DIGIT_BOUNDARIES_THRESHOLD_AREA = 32;
        // How much is the sub-bitmap area supposed to be inflated once confirmed to be valid
        private const int DIGIT_AREA_INFLATION_X = 8;
        private const int DIGIT_AREA_INFLATION_Y = 8;
        // Determines the resolution of the output sub-bitmaps
        private const int DIGIT_BITMAP_NORMALIZED_WIDTH = 32;
        private const int DIGIT_BITMAP_NORMALIZED_HEIGHT = 32;
        // If pixel sum value is below this value the pixel is considered to be active
        private const int ACTIVE_PIXEL_THRESHOLD = 256;
        // Used for clamping pixel values during convertion to grayscale
        private const float PIXEL_SUM_CLAMP_LOW = 240.0f;
        private const float PIXEL_SUM_CLAMP_HIGH = 540.0f;

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

        // User wants to select a bitmap via openFileDialog
        private void buttonLoadBitmap_Click(object sender, EventArgs e)
        {
            openFileDialogBitmap.ShowDialog();
        }

        // Source bitmap file has been selected
        private void openFileDialogBitmap_FileOk(object sender, CancelEventArgs e)
        {
            buttonProcess.Enabled = true;
        }

        // User requeted processing of the selected bitmap
        private void buttonProcess_Click(object sender, EventArgs e)
        {
            processBitmap(openFileDialogBitmap.FileName);
        }

        // Loads the file selected using openFileDialong
        private static void processBitmap(string strBitmapPath)
        {
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

                    int digitCount = 0;

                    // Go through all the pixels row by row and attempt to find all sub-bitmaps
                    for (int y = 0; y < bmpSourceNormalized.Height; y++)
                        for (int x = 0; x < bmpSourceNormalized.Width; x++)
                            // If there's an active pixel - dark one simply put
                            if (isActive(bmpSourceNormalized.GetPixel(x, y)))
                            {
                                // Get the boundaries that encapsulate the sub-bitmap the program has "bumped" into
                                Rectangle rectBounds = getBounds(bmpSourceNormalized, x, y);

                                // Found boundaries must fulfill the requested area threshold
                                // Smaller area would be unlikely to contain any comprehensive digit
                                if (rectBounds.Width * rectBounds.Height > DIGIT_BOUNDARIES_THRESHOLD_AREA)
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

                    // Save the final version of the input bitmap
                    // This exists mostly for the purposes of debugging as there
                    // is no real use to this bitmap after the process is done
                    // It should be ideally completely empty anyways
                    bmpSourceNormalized.Save("final.bmp", ImageFormat.Bmp);
                }
            }
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

            // Figure out the aspect ratio of input bitmap
            float aspectRatio = (float)bmpInput.Width / (float)bmpInput.Height;

            // Scale width and height, but make sure neither one of them exceeds
            // its normalized size while at least one of them meets its normalized size
            int scaledWidth = (int)((float)DIGIT_BITMAP_NORMALIZED_WIDTH * (bmpInput.Width >= bmpInput.Height ? 1.0f : aspectRatio));
            int scaledHeight = (int)((float)DIGIT_BITMAP_NORMALIZED_HEIGHT * (bmpInput.Height >= bmpInput.Width ? 1.0f : aspectRatio));

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
    }
}
