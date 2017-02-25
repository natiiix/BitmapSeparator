using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace BitmapSeparator
{
    #region public struct BoundsHolder
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
    #endregion

    #region public class GrayMatrix
    // Used to hold the matrices of gray values
    public class GrayMatrix
    {
        public byte[,] ByteMatrix;
        public int Width;
        public int Height;

        #region public GrayMatrix()
        public GrayMatrix()
        {
            ByteMatrix = new byte[0, 0];
            Width = 0;
            Height = 0;
        }

        public GrayMatrix(Bitmap bmp, bool overContrast = false)
        {
            FromBitmap(bmp, overContrast);
            Width = bmp.Width;
            Height = bmp.Height;
        }
        #endregion
        #region public void FromBitmap()
        // Convert a bitmap into an matrix representing gray values of each pixel
        // Should be faster than repetitive manipulation with bitmaps
        public void FromBitmap(Bitmap bmp, bool overContrast = false)
        {
            // Create the matrix for gray values with the dimensions of the input bitmap
            int[,] pixelSumMatrix = new int[bmp.Width, bmp.Height];

            int bitmapStride = 0;
            byte[] bitmapBytes;

            // Bitmap must have 24bpp RGB pixel format
            if (bmp.PixelFormat == BitmapTools.PIXEL_FORMAT_DEFAULT)
                bitmapBytes = getBytes(bmp, out bitmapStride);
            // Convert the input bitmap to this pixel format if necessary
            else
            {
                using (Bitmap bmp24bpp = new Bitmap(bmp.Width, bmp.Height, BitmapTools.PIXEL_FORMAT_DEFAULT))
                {
                    using (Graphics g = Graphics.FromImage(bmp24bpp))
                    {
                        g.DrawImageUnscaled(bmp, 0, 0);
                    }
                    bitmapBytes = getBytes(bmp24bpp, out bitmapStride);
                }
            }

            // How many empty bytes are at the end of each row
            int strideOverlap = bitmapStride - (bmp.Width * BitmapTools.BYTES_PER_PIXEL);
            int ptr = 0;

            int pixelSumMin = int.MaxValue;
            int pixelSumMax = int.MinValue;

            // Retrieve the pixel sum for each pixel
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    // Sum RGB values to get the pixel sum value
                    int pixelSum = bitmapBytes[ptr] + bitmapBytes[ptr + 1] + bitmapBytes[ptr + 2];
                    pixelSumMatrix[x, y] = pixelSum;

                    // Used to get the lowest and highest pixel sum
                    // Later used for maxing out the contrast
                    BitmapTools.setIfLower(ref pixelSumMin, pixelSum);
                    BitmapTools.setIfHigher(ref pixelSumMax, pixelSum);

                    // Move to the next column
                    ptr += BitmapTools.BYTES_PER_PIXEL;
                }
                // Move to the next row
                ptr += strideOverlap;
            }

            ByteMatrix = pixelSumMatrixToGrayscale(pixelSumMatrix, pixelSumMin, pixelSumMax, overContrast);
        }

        // Extract bytes from a 24bpp RGB bitmap
        private byte[] getBytes(Bitmap bmp, out int stride)
        {
            // Lock the bitmap's bits.
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, BitmapTools.PIXEL_FORMAT_DEFAULT);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Set the stride output
            stride = bmpData.Stride;

            // Declare an array to hold the bytes of the bitmap.
            int numBytes = bmpData.Stride * bmp.Height;
            byte[] colorValues = new byte[numBytes];

            // Copy the RGB values into the array.
            Marshal.Copy(ptr, colorValues, 0, numBytes);

            // Unlock the bits.
            bmp.UnlockBits(bmpData);

            return colorValues;
        }

        // Max out the contrast of a grayscale matrix
        // Optionally the matrix can be over-contrasted
        private byte[,] pixelSumMatrixToGrayscale(int[,] pixelSumMatrix, int pixelSumMin, int pixelSumMax, bool overContrast = false)
        {
            // Pixel sum can't naturally get higher than this
            const float pixelSumLimit = 255 * 3;
            // Each value in the matrix is scaled by this value
            float scale = pixelSumLimit / (float)(pixelSumMax - pixelSumMin);

            // Get matrix dimensions
            int width = pixelSumMatrix.GetLength(0);
            int height = pixelSumMatrix.GetLength(1);

            // Create the output matrix to store gray values
            byte[,] grayMatrix = new byte[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Scale the pixel sum
                    float pixelSumScaled = (pixelSumMatrix[x, y] - pixelSumMin) * scale;

                    // Clamp the pixel sum if activated
                    if (overContrast)
                    {
                        pixelSumScaled -= BitmapTools.PIXEL_SUM_CLAMP_LOW;
                        pixelSumScaled *= BitmapTools.PIXEL_SUM_CLAMP_FACTOR;
                    }

                    // Calculate the amount of gray for each channel
                    // Since it's grayscale, all channel share the same intensity
                    int grayScaled = (int)(pixelSumScaled / 3.0f);

                    // Clamp the gray value and store it in the output matrix
                    grayMatrix[x, y] = grayClamp(grayScaled);
                }
            }

            return grayMatrix;
        }

        // Clamps an integer to fit the min/max gray value and converts it to byte
        private static byte grayClamp(int input)
        {
            // Input is below lower threshold
            if (input < 0)
                return 0;
            // Input is above upper threshold
            else if (input > 255)
                return 255;
            // Input is within the threshold
            else
                return (byte)input;
        }
        #endregion
        #region public static Bitmap ToBitmap()
        // Converts a matrix of gray values into a bitmap
        public Bitmap ToBitmap()
        {
            Bitmap bmpOutput = new Bitmap(Width, Height, BitmapTools.PIXEL_FORMAT_DEFAULT);

            Rectangle rect = new Rectangle(0, 0, bmpOutput.Width, bmpOutput.Height);
            BitmapData bmpData = bmpOutput.LockBits(rect, ImageLockMode.WriteOnly, bmpOutput.PixelFormat);

            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bmpOutput.Height;

            Marshal.Copy(ToArray(bmpData.Stride), 0, ptr, bytes);
            bmpOutput.UnlockBits(bmpData);

            return bmpOutput;
        }

        // Converts a partial area of a matrix of gray values into a bitmap
        public Bitmap ToBitmap(Rectangle rectArea)
        {
            Bitmap bmpOutput = new Bitmap(rectArea.Width, rectArea.Height, BitmapTools.PIXEL_FORMAT_DEFAULT);

            Rectangle rect = new Rectangle(0, 0, bmpOutput.Width, bmpOutput.Height);
            BitmapData bmpData = bmpOutput.LockBits(rect, ImageLockMode.WriteOnly, bmpOutput.PixelFormat);

            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * bmpOutput.Height;

            Marshal.Copy(ToArray(bmpData.Stride, rectArea), 0, ptr, bytes);
            bmpOutput.UnlockBits(bmpData);

            return bmpOutput;
        }

        // Convert a matrix of gray values into an array
        private byte[] ToArray(int stride)
        {
            int width = ByteMatrix.GetLength(0);
            int height = ByteMatrix.GetLength(1);
            // Amount of empty bytes at the end of each row
            int strideOverlap = stride - (width * BitmapTools.BYTES_PER_PIXEL);
            // The output array
            byte[] outputBytes = new byte[stride * height];

            // Array pointer, points to the currently edited byte
            int ptr = 0;
            // Row
            for (int y = 0; y < height; y++)
            {
                // Column
                for (int x = 0; x < width; x++)
                {
                    // Channel
                    for (int b = 0; b < BitmapTools.BYTES_PER_PIXEL; b++)
                    {
                        outputBytes[ptr++] = ByteMatrix[x, y];
                    }
                }
                ptr += strideOverlap;
            }

            return outputBytes;
        }

        // Convert a partial area of a matrix of gray values into an array
        private byte[] ToArray(int stride, Rectangle rectArea)
        {
            GrayMatrix gmCropped = new GrayMatrix();

            // Initialize the cropped GrayMatrix object
            gmCropped.ByteMatrix = new byte[rectArea.Width, rectArea.Height];
            gmCropped.Width = rectArea.Width;
            gmCropped.Height = rectArea.Height;

            // Copy pixels from the source matrix to the cropped area matrix
            for (int y = 0; y < rectArea.Height; y++)
            {
                for (int x = 0; x < rectArea.Width; x++)
                {
                    gmCropped.ByteMatrix[x, y] = ByteMatrix[x + rectArea.X, y + rectArea.Y];
                }
            }

            return gmCropped.ToArray(stride);
        }
        #endregion
        #region public void CleanArea()
        // Fills an area with white pixels to ensure they won't get re-discovered as active
        public void CleanArea(Rectangle rectArea)
        {
            for (int y = rectArea.Top; y < rectArea.Bottom; y++)
                for (int x = rectArea.Left; x < rectArea.Right; x++)
                    ByteMatrix[x, y] = 255;
        }
        #endregion
    }
    #endregion

    #region public static class BitmapTools
    // Tools for bitmap manipulation
    public static class BitmapTools
    {
        #region constants
        // The range at which active neighbor pixels are detected
        private const int NEIGHBOR_PIXEL_DETECTION_RANGE = 2;

        // Determines the resolution of the output sub-bitmaps
        public static int DIGIT_BITMAP_NORMALIZED_WIDTH;
        public static int DIGIT_BITMAP_NORMALIZED_HEIGHT;

        // If pixel sum value is below this value the pixel is considered to be active
        public static int ACTIVE_PIXEL_THRESHOLD;

        // Used for clamping pixel values during convertion to grayscale
        public static int PIXEL_SUM_CLAMP_LOW;
        public static int PIXEL_SUM_CLAMP_HIGH;
        public static float PIXEL_SUM_CLAMP_FACTOR;

        // All bitmaps used by the program should have the same pixel format
        public const PixelFormat PIXEL_FORMAT_DEFAULT = PixelFormat.Format24bppRgb;
        // Bitmap is always in 24 bpp format, therefore it always has 3 bytes per pixel
        public const int BYTES_PER_PIXEL = 3;
        #endregion

        // Decides whether a pixel is active or not
        // Dark pixels are considered to be active while bright pixels are supposedly inactive
        public static bool isActive(byte b)
        {
            return (b <= ACTIVE_PIXEL_THRESHOLD);
        }

        #region public static Rectangle getBounds()
        // Get boundaries for a sub-bitmap that neighbors with x, y pixel
        // Follows neighbor pixels in order to find the whole boundaries for the currently processed sub-bitmap
        public static Rectangle getBounds(GrayMatrix gm, int x, int y)
        {
            // Create bounds holder and reset its values to make sure the bounds are properly stored
            BoundsHolder bounds = new BoundsHolder();

            bounds.Left = bounds.Top = int.MaxValue;
            bounds.Right = bounds.Bottom = int.MinValue;

            // Get all the active neighbor pixels within specified range
            List<Point> activeNeighbors = new List<Point>();
            activeNeighbors.Add(new Point(x, y));

            // This matrix stores information about whether a pixel has been already visited by the followNeighbors function
            // This is used to make sure the program doesn't start running in an infinite loop
            bool[,] visited = new bool[gm.Width, gm.Height];
            visited[x, y] = true;

            // Process each discovered active neighbor
            while (activeNeighbors.Count > 0)
            {
                // Select last and remove it
                Point neighbor = activeNeighbors[activeNeighbors.Count - 1];
                activeNeighbors.RemoveAt(activeNeighbors.Count - 1);

                // Update boundaries
                setIfLower(ref bounds.Left, neighbor.X);
                setIfLower(ref bounds.Top, neighbor.Y);
                setIfHigher(ref bounds.Right, neighbor.X);
                setIfHigher(ref bounds.Bottom, neighbor.Y);

                // Add this neighbor's neighbors into the list
                findActiveNeighbors(gm, ref activeNeighbors, ref visited, neighbor.X, neighbor.Y);
            }

            // Return the boudaries as a rectangle
            return bounds.ToRectangle();
        }
        #endregion

        #region private static void findActiveNeighbors()
        // Find all the active neighbor pixels within a specified range of pixel x, y
        private static void findActiveNeighbors(GrayMatrix gm, ref List<Point> activeNeighborList, ref bool[,] visited, int x, int y)
        {
            for (int iy = y - NEIGHBOR_PIXEL_DETECTION_RANGE; iy <= y + NEIGHBOR_PIXEL_DETECTION_RANGE; iy++)
                for (int ix = x - NEIGHBOR_PIXEL_DETECTION_RANGE; ix <= x + NEIGHBOR_PIXEL_DETECTION_RANGE; ix++)
                // Only test pixels within the boudaries of the input bitmap
                // Pixel x, y itself isn't supposed to be tested either
                {
                    if (ix >= 0 && ix < gm.Width && iy >= 0 && iy < gm.Height)
                    {
                        if (!visited[ix, iy] && isActive(gm.ByteMatrix[ix, iy]))
                            activeNeighborList.Add(new Point(ix, iy));

                        // This pixel has been visited and shall not be visited again
                        visited[ix, iy] = true;
                    }
                }
        }
        #endregion

        // Sets valSet to valCompare if valCompare is higher than valSet
        public static void setIfHigher(ref int valSet, int valCompare)
        {
            if (valCompare > valSet)
                valSet = valCompare;
        }

        // Sets valSet to valCompare if valCompare is lower than valSet
        public static void setIfLower(ref int valSet, int valCompare)
        {
            if (valCompare < valSet)
                valSet = valCompare;
        }

        #region public static Bitmap normalizeDigitBitmap()
        // To make the work for the AI algorithm easier all sub-bitmaps are scaled
        // to the same resolution and converted to grayscale using this function
        public static Bitmap normalizeDigitBitmap(Bitmap bmpInput)
        {
            // Create a bitmap to store the final sub-bitmap with normalized resolution
            Bitmap bmpNormalized = new Bitmap(DIGIT_BITMAP_NORMALIZED_WIDTH, DIGIT_BITMAP_NORMALIZED_HEIGHT);
            // We need to make sure there are no black bars on sides (if the input bitmap has different aspect ration than the output)
            using (Graphics g = Graphics.FromImage(bmpNormalized))
                g.FillRectangle(new SolidBrush(Color.FromArgb(255, 255, 255, 255)), new Rectangle(0, 0, bmpNormalized.Width, bmpNormalized.Height));

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
                    g.DrawImage(bitmapToGrayscale(bmpScaled, true), new Rectangle(paddingLeft, paddingTop, bmpScaled.Width, bmpScaled.Height));
                }
            }

            // Return the normalized bitmap with proper resolution converted to grayscale
            return bmpNormalized;
        }
        #endregion

        // Convert a standard RGB bitmap to grayscale bitmap
        public static Bitmap bitmapToGrayscale(Bitmap bmp, bool overContrast = false)
        {
            return (new GrayMatrix(bmp, overContrast)).ToBitmap();
        }
    }
    #endregion
}
