namespace WinFormsClient
{
    partial class UserInfoControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserInfoControl));
            this.lblUserName = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblUserName
            // 
            this.lblUserName.BackColor = System.Drawing.Color.White;
            this.lblUserName.Location = new System.Drawing.Point(73, 12);
            this.lblUserName.Margin = new System.Windows.Forms.Padding(0);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(209, 25);
            this.lblUserName.TabIndex = 0;
            this.lblUserName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblUserName.DoubleClick += new System.EventHandler(this.UserInfoControl_DoubleClick);
            // 
            // UserInfoControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.Controls.Add(this.lblUserName);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "UserInfoControl";
            this.Size = new System.Drawing.Size(360, 50);
            this.DoubleClick += new System.EventHandler(this.UserInfoControl_DoubleClick);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblUserName;
    }
}
