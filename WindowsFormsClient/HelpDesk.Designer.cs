namespace WinFormsClient
{
    partial class HelpDesk
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Label btnClose;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HelpDesk));
            this.btnLogOut = new System.Windows.Forms.Button();
            this.btnList = new System.Windows.Forms.Button();
            this.btnMemo = new System.Windows.Forms.Button();
            this.btnSetting = new System.Windows.Forms.Button();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.panel = new System.Windows.Forms.FlowLayoutPanel();
            this.lblCount = new System.Windows.Forms.Label();
            btnClose = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            btnClose.Font = new System.Drawing.Font("Gulim", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            btnClose.ForeColor = System.Drawing.Color.White;
            btnClose.Image = ((System.Drawing.Image)(resources.GetObject("btnClose.Image")));
            btnClose.Location = new System.Drawing.Point(343, 4);
            btnClose.Margin = new System.Windows.Forms.Padding(0);
            btnClose.Name = "btnClose";
            btnClose.Size = new System.Drawing.Size(13, 14);
            btnClose.TabIndex = 6;
            btnClose.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnLogOut
            // 
            this.btnLogOut.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnLogOut.BackgroundImage")));
            this.btnLogOut.FlatAppearance.BorderColor = System.Drawing.Color.Blue;
            this.btnLogOut.FlatAppearance.BorderSize = 0;
            this.btnLogOut.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogOut.Location = new System.Drawing.Point(13, 541);
            this.btnLogOut.Margin = new System.Windows.Forms.Padding(0);
            this.btnLogOut.Name = "btnLogOut";
            this.btnLogOut.Size = new System.Drawing.Size(44, 44);
            this.btnLogOut.TabIndex = 0;
            this.btnLogOut.UseVisualStyleBackColor = true;
            this.btnLogOut.Click += new System.EventHandler(this.btnLogOut_Click);
            // 
            // btnList
            // 
            this.btnList.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnList.BackgroundImage")));
            this.btnList.FlatAppearance.BorderColor = System.Drawing.Color.Blue;
            this.btnList.FlatAppearance.BorderSize = 0;
            this.btnList.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnList.Location = new System.Drawing.Point(103, 541);
            this.btnList.Margin = new System.Windows.Forms.Padding(0);
            this.btnList.Name = "btnList";
            this.btnList.Size = new System.Drawing.Size(44, 44);
            this.btnList.TabIndex = 1;
            this.btnList.UseVisualStyleBackColor = true;
            this.btnList.Click += new System.EventHandler(this.btnList_Click);
            // 
            // btnMemo
            // 
            this.btnMemo.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnMemo.BackgroundImage")));
            this.btnMemo.FlatAppearance.BorderColor = System.Drawing.Color.Blue;
            this.btnMemo.FlatAppearance.BorderSize = 0;
            this.btnMemo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMemo.Location = new System.Drawing.Point(197, 541);
            this.btnMemo.Margin = new System.Windows.Forms.Padding(0);
            this.btnMemo.Name = "btnMemo";
            this.btnMemo.Size = new System.Drawing.Size(44, 44);
            this.btnMemo.TabIndex = 2;
            this.btnMemo.UseVisualStyleBackColor = true;
            this.btnMemo.Click += new System.EventHandler(this.btnMemo_Click);
            // 
            // btnSetting
            // 
            this.btnSetting.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnSetting.BackgroundImage")));
            this.btnSetting.FlatAppearance.BorderColor = System.Drawing.Color.Blue;
            this.btnSetting.FlatAppearance.BorderSize = 0;
            this.btnSetting.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSetting.Location = new System.Drawing.Point(288, 541);
            this.btnSetting.Margin = new System.Windows.Forms.Padding(0);
            this.btnSetting.Name = "btnSetting";
            this.btnSetting.Size = new System.Drawing.Size(44, 44);
            this.btnSetting.TabIndex = 3;
            this.btnSetting.UseVisualStyleBackColor = true;
            this.btnSetting.Click += new System.EventHandler(this.btnSetting_Click);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.BalloonTipClicked += new System.EventHandler(this.notifyIcon1_BalloonTipClicked);
            this.notifyIcon1.Click += new System.EventHandler(this.notifyIcon1_Click);
            // 
            // panel
            // 
            this.panel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.panel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.panel.Location = new System.Drawing.Point(0, 59);
            this.panel.Name = "panel";
            this.panel.Size = new System.Drawing.Size(358, 477);
            this.panel.TabIndex = 4;
            // 
            // lblCount
            // 
            this.lblCount.Font = new System.Drawing.Font("Gulim", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblCount.ForeColor = System.Drawing.Color.White;
            this.lblCount.Image = ((System.Drawing.Image)(resources.GetObject("lblCount.Image")));
            this.lblCount.Location = new System.Drawing.Point(211, 22);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(15, 18);
            this.lblCount.TabIndex = 5;
            this.lblCount.Text = "1";
            this.lblCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // HelpDesk
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(360, 590);
            this.ControlBox = false;
            this.Controls.Add(btnClose);
            this.Controls.Add(this.lblCount);
            this.Controls.Add(this.panel);
            this.Controls.Add(this.btnSetting);
            this.Controls.Add(this.btnMemo);
            this.Controls.Add(this.btnList);
            this.Controls.Add(this.btnLogOut);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "HelpDesk";
            this.ShowIcon = false;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnLogOut;
        private System.Windows.Forms.Button btnList;
        private System.Windows.Forms.Button btnMemo;
        private System.Windows.Forms.Button btnSetting;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.FlowLayoutPanel panel;
        private System.Windows.Forms.Label lblCount;

    }
}