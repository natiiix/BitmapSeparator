using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Diagnostics;

namespace BitmapSeparator
{
    public partial class FormMain : Form
    {
        // The lowest possible area considered to be a valid sub-bitmap
        // Used to prevent undesired small sub-bitmap from appearing
        private static int DIGIT_BOUNDARIES_AREA_THRESHOLD;
        // How much is the sub-bitmap area supposed to be inflated once confirmed to be valid
        private const int DIGIT_AREA_INFLATION_X = 2;
        private const int DIGIT_AREA_INFLATION_Y = 2;

        // Form initialization
        public FormMain()
        {
            InitializeComponent();
        }

        #region events
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
            int.TryParse(textBoxDigitBitmapNormalizedWidth.Text, out BitmapTools.DIGIT_BITMAP_NORMALIZED_WIDTH);
        }

        private void textBoxDigitBitmapNormalizedHeight_TextChanged(object sender, EventArgs e)
        {
            int.TryParse(textBoxDigitBitmapNormalizedHeight.Text, out BitmapTools.DIGIT_BITMAP_NORMALIZED_HEIGHT);
        }

        private void trackBarActivePixelThreshold_ValueChanged(object sender, EventArgs e)
        {
            BitmapTools.ACTIVE_PIXEL_THRESHOLD = trackBarActivePixelThreshold.Value;
            labelActivePixelThreshold.Text = "Active Pixel Threshold: " + BitmapTools.ACTIVE_PIXEL_THRESHOLD.ToString();
        }

        private void textBoxPixelSumClampLow_TextChanged(object sender, EventArgs e)
        {
            int.TryParse(textBoxPixelSumClampLow.Text, out BitmapTools.PIXEL_SUM_CLAMP_LOW);
            BitmapTools.PIXEL_SUM_CLAMP_FACTOR = (256.0f * 3.0f) / (BitmapTools.PIXEL_SUM_CLAMP_HIGH - BitmapTools.PIXEL_SUM_CLAMP_LOW);
        }

        private void textBoxPixelSumClampHigh_TextChanged(object sender, EventArgs e)
        {
            int.TryParse(textBoxPixelSumClampHigh.Text, out BitmapTools.PIXEL_SUM_CLAMP_HIGH);
            BitmapTools.PIXEL_SUM_CLAMP_FACTOR = (256.0f * 3.0f) / (BitmapTools.PIXEL_SUM_CLAMP_HIGH - BitmapTools.PIXEL_SUM_CLAMP_LOW);
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
            Stopwatch sw = new Stopwatch();
            sw.Restart();
            processBitmap(openFileDialogBitmap.FileName);
            sw.Stop();
            MessageBox.Show(sw.ElapsedMilliseconds.ToString() + " ms", "Benchmark results");
        }
        #endregion

        #region private void formElementsToggle()
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
        #endregion
        #region private void processBitmap()
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
                GrayMatrix gmNormalized = new GrayMatrix(bmpSouceRaw);

                // Save the normalied version of the input bitmap
                // Used solely for debugging purposes to see what the pixel values are
                // Shouldn't be executed in production as it may slow down the process
                //gmNormalized.ToBitmap().Save("normalized.bmp", ImageFormat.Bmp);

                // Create the discovered digit counter
                // Used to generate unique file name for each sub-bitmap
                int digitCount = 0;

                // Since the progress bar updates with every row
                // it's maximum has to be set to the highest value
                // it can each, which is the height of the bitmap - 1
                progressBarBitmapProcessing.Maximum = gmNormalized.Height - 1;

                // Go through all the pixels row by row and attempt to find all sub-bitmaps
                for (int y = 0; y < gmNormalized.Height; y++)
                {
                    for (int x = 0; x < gmNormalized.Width; x++)
                        // If there's an active pixel - dark one simply put
                        if (BitmapTools.isActive(gmNormalized.ByteMatrix[x, y]))
                        {
                            // Get the boundaries that encapsulate the sub-bitmap the program has "bumped" into
                            Rectangle rectBounds = BitmapTools.getBounds(gmNormalized, x, y);

                            // Found boundaries must fulfill the requested area threshold
                            // Smaller area would be unlikely to contain any comprehensive digit
                            if (rectBounds.Width * rectBounds.Height > DIGIT_BOUNDARIES_AREA_THRESHOLD)
                            {
                                // Inflate the boundaries slightly to make sure we've got the whole digit
                                rectBounds.Inflate(DIGIT_AREA_INFLATION_X, DIGIT_AREA_INFLATION_Y);

                                // Each sub-bitmap is re-scaled and converted to grayscale to fit the algorithm as well as possible
                                // That means all the output sub-bitmaps have the same resolution and highest possible contrast
                                using (Bitmap bmpDigit = gmNormalized.ToBitmap(rectBounds))
                                {
                                    using (Bitmap bmpDigitNormalized = BitmapTools.normalizeDigitBitmap(bmpDigit))
                                    {
                                        bmpDigitNormalized.Save("digit" + digitCount++.ToString("0000") + ".bmp", ImageFormat.Bmp);
                                    }
                                }

                                // Remove the recently discovered sub-bitmap from the source bitmap
                                // to make sure it doesn't get re-discovered accidentally
                                gmNormalized.CleanArea(rectBounds);
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
                //gmNormalized.ToBitmap().Save("final.bmp", ImageFormat.Bmp);
            }

            // Reset the progress bar
            progressBarBitmapProcessing.Value = 0;

            // Re-enable form elements
            formElementsToggle(true, true, true);
        }
        #endregion
    }
}
