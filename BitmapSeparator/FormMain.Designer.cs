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
            this.buttonLoadBitmap.Size = new System.Drawing.Size(75, 23);
            this.buttonLoadBitmap.TabIndex = 0;
            this.buttonLoadBitmap.Text = "Load";
            this.buttonLoadBitmap.UseVisualStyleBackColor = true;
            this.buttonLoadBitmap.Click += new System.EventHandler(this.buttonLoadBitmap_Click);
            // 
            // buttonProcess
            // 
            this.buttonProcess.Enabled = false;
            this.buttonProcess.Location = new System.Drawing.Point(12, 41);
            this.buttonProcess.Name = "buttonProcess";
            this.buttonProcess.Size = new System.Drawing.Size(75, 23);
            this.buttonProcess.TabIndex = 1;
            this.buttonProcess.Text = "Process";
            this.buttonProcess.UseVisualStyleBackColor = true;
            this.buttonProcess.Click += new System.EventHandler(this.buttonProcess_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.buttonProcess);
            this.Controls.Add(this.buttonLoadBitmap);
            this.Name = "FormMain";
            this.Text = "Bitmap Separator";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialogBitmap;
        private System.Windows.Forms.Button buttonLoadBitmap;
        private System.Windows.Forms.Button buttonProcess;
    }
}

