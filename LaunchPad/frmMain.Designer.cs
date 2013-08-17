namespace LaunchPad
{
    partial class frmMain
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
      this.SuspendLayout();
      // 
      // frmMain
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(121, 128);
      this.DoubleBuffered = true;
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "frmMain";
      this.ShowInTaskbar = false;
      this.Text = "LaunchPad";
      this.TopMost = true;
      this.Deactivate += new System.EventHandler(this.frmMain_Deactivate);
      this.Load += new System.EventHandler(this.frmMain_Load);
      this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmMain_KeyDown);
      this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.frmMain_MouseClick);
      this.ResumeLayout(false);

        }

        #endregion
    }
}

