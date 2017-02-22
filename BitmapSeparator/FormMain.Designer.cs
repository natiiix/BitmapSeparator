namespace BitmapSeparator
{
    partial class FormMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.openFileDialogBitmap = new System.Windows.Forms.OpenFileDialog();
            this.buttonLoadBitmap = new System.Windows.Forms.Button();
            this.buttonProcess = new System.Windows.Forms.Button();
            this.trackBarDigitBoundariesAreaThreshold = new System.Windows.Forms.TrackBar();
            this.labelDigitBoundariesAreaThreshold = new System.Windows.Forms.Label();
            this.label_DigitBitmapNormalizedResolution = new System.Windows.Forms.Label();
            this.textBoxDigitBitmapNormalizedWidth = new System.Windows.Forms.TextBox();
            this.textBoxDigitBitmapNormalizedHeight = new System.Windows.Forms.TextBox();
            this.label_DigitBitmapNormalizedWidth = new System.Windows.Forms.Label();
            this.label_DigitBitmapNormalizedHeight = new System.Windows.Forms.Label();
            this.label_PixelSumClampHigh = new System.Windows.Forms.Label();
            this.label_PixelSumClampLow = new System.Windows.Forms.Label();
            this.textBoxPixelSumClampHigh = new System.Windows.Forms.TextBox();
            this.textBoxPixelSumClampLow = new System.Windows.Forms.TextBox();
            this.label_PixelSumClamp = new System.Windows.Forms.Label();
            this.progressBarBitmapProcessing = new System.Windows.Forms.ProgressBar();
            this.trackBarActivePixelThreshold = new System.Windows.Forms.TrackBar();
            this.labelActivePixelThreshold = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarDigitBoundariesAreaThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarActivePixelThreshold)).BeginInit();
            this.SuspendLayout();
            // 
            // openFileDialogBitmap
            // 
            this.openFileDialogBitmap.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialogBitmap_FileOk);
            // 
            // buttonLoadBitmap
            // 
            this.buttonLoadBitmap.Location = new System.Drawing.Point(12, 12);
            this.buttonLoadBitmap.Name = "buttonLoadBitmap";
            this.buttonLoadBitmap.Size = new System.Drawing.Size(127, 48);
            this.buttonLoadBitmap.TabIndex = 0;
            this.buttonLoadBitmap.Text = "Load";
            this.buttonLoadBitmap.UseVisualStyleBackColor = true;
            this.buttonLoadBitmap.Click += new System.EventHandler(this.buttonLoadBitmap_Click);
            // 
            // buttonProcess
            // 
            this.buttonProcess.Enabled = false;
            this.buttonProcess.Location = new System.Drawing.Point(145, 12);
            this.buttonProcess.Name = "buttonProcess";
            this.buttonProcess.Size = new System.Drawing.Size(127, 48);
            this.buttonProcess.TabIndex = 1;
            this.buttonProcess.Text = "Process";
            this.buttonProcess.UseVisualStyleBackColor = true;
            this.buttonProcess.Click += new System.EventHandler(this.buttonProcess_Click);
            // 
            // trackBarDigitBoundariesAreaThreshold
            // 
            this.trackBarDigitBoundariesAreaThreshold.LargeChange = 64;
            this.trackBarDigitBoundariesAreaThreshold.Location = new System.Drawing.Point(12, 100);
            this.trackBarDigitBoundariesAreaThreshold.Maximum = 512;
            this.trackBarDigitBoundariesAreaThreshold.Name = "trackBarDigitBoundariesAreaThreshold";
            this.trackBarDigitBoundariesAreaThreshold.Size = new System.Drawing.Size(260, 45);
            this.trackBarDigitBoundariesAreaThreshold.SmallChange = 8;
            this.trackBarDigitBoundariesAreaThreshold.TabIndex = 2;
            this.trackBarDigitBoundariesAreaThreshold.TickFrequency = 16;
            this.trackBarDigitBoundariesAreaThreshold.Value = 64;
            this.trackBarDigitBoundariesAreaThreshold.ValueChanged += new System.EventHandler(this.trackBarDigitBoundariesAreaThreshold_ValueChanged);
            // 
            // labelDigitBoundariesAreaThreshold
            // 
            this.labelDigitBoundariesAreaThreshold.AutoSize = true;
            this.labelDigitBoundariesAreaThreshold.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelDigitBoundariesAreaThreshold.Location = new System.Drawing.Point(12, 81);
            this.labelDigitBoundariesAreaThreshold.Name = "labelDigitBoundariesAreaThreshold";
            this.labelDigitBoundariesAreaThreshold.Size = new System.Drawing.Size(203, 16);
            this.labelDigitBoundariesAreaThreshold.TabIndex = 3;
            this.labelDigitBoundariesAreaThreshold.Text = "Digit Boundaries Area Threshold";
            // 
            // label_DigitBitmapNormalizedResolution
            // 
            this.label_DigitBitmapNormalizedResolution.AutoSize = true;
            this.label_DigitBitmapNormalizedResolution.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label_DigitBitmapNormalizedResolution.Location = new System.Drawing.Point(12, 148);
            this.label_DigitBitmapNormalizedResolution.Name = "label_DigitBitmapNormalizedResolution";
            this.label_DigitBitmapNormalizedResolution.Size = new System.Drawing.Size(219, 16);
            this.label_DigitBitmapNormalizedResolution.TabIndex = 4;
            this.label_DigitBitmapNormalizedResolution.Text = "Digit Bitmap Normalized Resolution";
            // 
            // textBoxDigitBitmapNormalizedWidth
            // 
            this.textBoxDigitBitmapNormalizedWidth.Location = new System.Drawing.Point(82, 167);
            this.textBoxDigitBitmapNormalizedWidth.Name = "textBoxDigitBitmapNormalizedWidth";
            this.textBoxDigitBitmapNormalizedWidth.Size = new System.Drawing.Size(48, 20);
            this.textBoxDigitBitmapNormalizedWidth.TabIndex = 5;
            this.textBoxDigitBitmapNormalizedWidth.Text = "32";
            this.textBoxDigitBitmapNormalizedWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBoxDigitBitmapNormalizedWidth.TextChanged += new System.EventHandler(this.textBoxDigitBitmapNormalizedWidth_TextChanged);
            // 
            // textBoxDigitBitmapNormalizedHeight
            // 
            this.textBoxDigitBitmapNormalizedHeight.Location = new System.Drawing.Point(200, 167);
            this.textBoxDigitBitmapNormalizedHeight.Name = "textBoxDigitBitmapNormalizedHeight";
            this.textBoxDigitBitmapNormalizedHeight.Size = new System.Drawing.Size(48, 20);
            this.textBoxDigitBitmapNormalizedHeight.TabIndex = 6;
            this.textBoxDigitBitmapNormalizedHeight.Text = "64";
            this.textBoxDigitBitmapNormalizedHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBoxDigitBitmapNormalizedHeight.TextChanged += new System.EventHandler(this.textBoxDigitBitmapNormalizedHeight_TextChanged);
            // 
            // label_DigitBitmapNormalizedWidth
            // 
            this.label_DigitBitmapNormalizedWidth.AutoSize = true;
            this.label_DigitBitmapNormalizedWidth.Location = new System.Drawing.Point(38, 170);
            this.label_DigitBitmapNormalizedWidth.Name = "label_DigitBitmapNormalizedWidth";
            this.label_DigitBitmapNormalizedWidth.Size = new System.Drawing.Size(38, 13);
            this.label_DigitBitmapNormalizedWidth.TabIndex = 7;
            this.label_DigitBitmapNormalizedWidth.Text = "Width:";
            // 
            // label_DigitBitmapNormalizedHeight
            // 
            this.label_DigitBitmapNormalizedHeight.AutoSize = true;
            this.label_DigitBitmapNormalizedHeight.Location = new System.Drawing.Point(153, 170);
            this.label_DigitBitmapNormalizedHeight.Name = "label_DigitBitmapNormalizedHeight";
            this.label_DigitBitmapNormalizedHeight.Size = new System.Drawing.Size(41, 13);
            this.label_DigitBitmapNormalizedHeight.TabIndex = 7;
            this.label_DigitBitmapNormalizedHeight.Text = "Height:";
            // 
            // label_PixelSumClampHigh
            // 
            this.label_PixelSumClampHigh.AutoSize = true;
            this.label_PixelSumClampHigh.Location = new System.Drawing.Point(162, 301);
            this.label_PixelSumClampHigh.Name = "label_PixelSumClampHigh";
            this.label_PixelSumClampHigh.Size = new System.Drawing.Size(32, 13);
            this.label_PixelSumClampHigh.TabIndex = 11;
            this.label_PixelSumClampHigh.Text = "High:";
            // 
            // label_PixelSumClampLow
            // 
            this.label_PixelSumClampLow.AutoSize = true;
            this.label_PixelSumClampLow.Location = new System.Drawing.Point(46, 301);
            this.label_PixelSumClampLow.Name = "label_PixelSumClampLow";
            this.label_PixelSumClampLow.Size = new System.Drawing.Size(30, 13);
            this.label_PixelSumClampLow.TabIndex = 12;
            this.label_PixelSumClampLow.Text = "Low:";
            // 
            // textBoxPixelSumClampHigh
            // 
            this.textBoxPixelSumClampHigh.Location = new System.Drawing.Point(200, 298);
            this.textBoxPixelSumClampHigh.Name = "textBoxPixelSumClampHigh";
            this.textBoxPixelSumClampHigh.Size = new System.Drawing.Size(48, 20);
            this.textBoxPixelSumClampHigh.TabIndex = 10;
            this.textBoxPixelSumClampHigh.Text = "540";
            this.textBoxPixelSumClampHigh.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBoxPixelSumClampHigh.TextChanged += new System.EventHandler(this.textBoxPixelSumClampHigh_TextChanged);
            // 
            // textBoxPixelSumClampLow
            // 
            this.textBoxPixelSumClampLow.Location = new System.Drawing.Point(82, 298);
            this.textBoxPixelSumClampLow.Name = "textBoxPixelSumClampLow";
            this.textBoxPixelSumClampLow.Size = new System.Drawing.Size(48, 20);
            this.textBoxPixelSumClampLow.TabIndex = 9;
            this.textBoxPixelSumClampLow.Text = "240";
            this.textBoxPixelSumClampLow.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBoxPixelSumClampLow.TextChanged += new System.EventHandler(this.textBoxPixelSumClampLow_TextChanged);
            // 
            // label_PixelSumClamp
            // 
            this.label_PixelSumClamp.AutoSize = true;
            this.label_PixelSumClamp.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label_PixelSumClamp.Location = new System.Drawing.Point(12, 279);
            this.label_PixelSumClamp.Name = "label_PixelSumClamp";
            this.label_PixelSumClamp.Size = new System.Drawing.Size(109, 16);
            this.label_PixelSumClamp.TabIndex = 8;
            this.label_PixelSumClamp.Text = "Pixel Sum Clamp";
            // 
            // progressBarBitmapProcessing
            // 
            this.progressBarBitmapProcessing.Location = new System.Drawing.Point(12, 336);
            this.progressBarBitmapProcessing.Name = "progressBarBitmapProcessing";
            this.progressBarBitmapProcessing.Size = new System.Drawing.Size(260, 23);
            this.progressBarBitmapProcessing.TabIndex = 13;
            // 
            // trackBarActivePixelThreshold
            // 
            this.trackBarActivePixelThreshold.LargeChange = 128;
            this.trackBarActivePixelThreshold.Location = new System.Drawing.Point(12, 231);
            this.trackBarActivePixelThreshold.Maximum = 765;
            this.trackBarActivePixelThreshold.Name = "trackBarActivePixelThreshold";
            this.trackBarActivePixelThreshold.Size = new System.Drawing.Size(260, 45);
            this.trackBarActivePixelThreshold.SmallChange = 16;
            this.trackBarActivePixelThreshold.TabIndex = 2;
            this.trackBarActivePixelThreshold.TickFrequency = 32;
            this.trackBarActivePixelThreshold.Value = 256;
            this.trackBarActivePixelThreshold.ValueChanged += new System.EventHandler(this.trackBarActivePixelThreshold_ValueChanged);
            // 
            // labelActivePixelThreshold
            // 
            this.labelActivePixelThreshold.AutoSize = true;
            this.labelActivePixelThreshold.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelActivePixelThreshold.Location = new System.Drawing.Point(12, 212);
            this.labelActivePixelThreshold.Name = "labelActivePixelThreshold";
            this.labelActivePixelThreshold.Size = new System.Drawing.Size(141, 16);
            this.labelActivePixelThreshold.TabIndex = 3;
            this.labelActivePixelThreshold.Text = "Active Pixel Threshold";
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 371);
            this.Controls.Add(this.progressBarBitmapProcessing);
            this.Controls.Add(this.label_PixelSumClampHigh);
            this.Controls.Add(this.label_PixelSumClampLow);
            this.Controls.Add(this.textBoxPixelSumClampHigh);
            this.Controls.Add(this.textBoxPixelSumClampLow);
            this.Controls.Add(this.label_PixelSumClamp);
            this.Controls.Add(this.label_DigitBitmapNormalizedHeight);
            this.Controls.Add(this.label_DigitBitmapNormalizedWidth);
            this.Controls.Add(this.textBoxDigitBitmapNormalizedHeight);
            this.Controls.Add(this.textBoxDigitBitmapNormalizedWidth);
            this.Controls.Add(this.label_DigitBitmapNormalizedResolution);
            this.Controls.Add(this.labelActivePixelThreshold);
            this.Controls.Add(this.trackBarActivePixelThreshold);
            this.Controls.Add(this.labelDigitBoundariesAreaThreshold);
            this.Controls.Add(this.trackBarDigitBoundariesAreaThreshold);
            this.Controls.Add(this.buttonProcess);
            this.Controls.Add(this.buttonLoadBitmap);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.Text = "Bitmap Separator";
            this.Load += new System.EventHandler(this.FormMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarDigitBoundariesAreaThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarActivePixelThreshold)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialogBitmap;
        private System.Windows.Forms.Button buttonLoadBitmap;
        private System.Windows.Forms.Button buttonProcess;
        private System.Windows.Forms.TrackBar trackBarDigitBoundariesAreaThreshold;
        private System.Windows.Forms.Label labelDigitBoundariesAreaThreshold;
        private System.Windows.Forms.Label label_DigitBitmapNormalizedResolution;
        private System.Windows.Forms.TextBox textBoxDigitBitmapNormalizedWidth;
        private System.Windows.Forms.TextBox textBoxDigitBitmapNormalizedHeight;
        private System.Windows.Forms.Label label_DigitBitmapNormalizedWidth;
        private System.Windows.Forms.Label label_DigitBitmapNormalizedHeight;
        private System.Windows.Forms.Label label_PixelSumClampHigh;
        private System.Windows.Forms.Label label_PixelSumClampLow;
        private System.Windows.Forms.TextBox textBoxPixelSumClampHigh;
        private System.Windows.Forms.TextBox textBoxPixelSumClampLow;
        private System.Windows.Forms.Label label_PixelSumClamp;
        private System.Windows.Forms.ProgressBar progressBarBitmapProcessing;
        private System.Windows.Forms.TrackBar trackBarActivePixelThreshold;
        private System.Windows.Forms.Label labelActivePixelThreshold;
    }
}

