namespace UnlimitedDiag
{
    partial class _2534_device_selection_form
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
            this.DeviceSelectList = new System.Windows.Forms.CheckedListBox();
            this.DeviceDetails = new System.Windows.Forms.TextBox();
            this.LibBrowseDialog = new System.Windows.Forms.OpenFileDialog();
            this.LibBrowseButton = new System.Windows.Forms.Button();
            this.LibOpenButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // DeviceSelectList
            // 
            this.DeviceSelectList.FormattingEnabled = true;
            this.DeviceSelectList.Location = new System.Drawing.Point(13, 13);
            this.DeviceSelectList.Name = "DeviceSelectList";
            this.DeviceSelectList.Size = new System.Drawing.Size(332, 214);
            this.DeviceSelectList.TabIndex = 0;
            this.DeviceSelectList.SelectedValueChanged += new System.EventHandler(this.UpdateDeviceDetails);
            // 
            // DeviceDetails
            // 
            this.DeviceDetails.Location = new System.Drawing.Point(351, 13);
            this.DeviceDetails.Multiline = true;
            this.DeviceDetails.Name = "DeviceDetails";
            this.DeviceDetails.Size = new System.Drawing.Size(472, 214);
            this.DeviceDetails.TabIndex = 1;
            // 
            // LibBrowseButton
            // 
            this.LibBrowseButton.Location = new System.Drawing.Point(245, 254);
            this.LibBrowseButton.Name = "LibBrowseButton";
            this.LibBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.LibBrowseButton.TabIndex = 2;
            this.LibBrowseButton.Text = "Browse";
            this.LibBrowseButton.UseVisualStyleBackColor = true;
            this.LibBrowseButton.Click += new System.EventHandler(this.LibBrowseButton_Click);
            // 
            // LibOpenButton
            // 
            this.LibOpenButton.Location = new System.Drawing.Point(13, 254);
            this.LibOpenButton.Name = "LibOpenButton";
            this.LibOpenButton.Size = new System.Drawing.Size(75, 23);
            this.LibOpenButton.TabIndex = 3;
            this.LibOpenButton.Text = "Open";
            this.LibOpenButton.UseVisualStyleBackColor = true;
            // 
            // _2534_device_selection_form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(835, 313);
            this.Controls.Add(this.LibOpenButton);
            this.Controls.Add(this.LibBrowseButton);
            this.Controls.Add(this.DeviceDetails);
            this.Controls.Add(this.DeviceSelectList);
            this.Name = "_2534_device_selection_form";
            this.Text = "_2534_device_selection_form";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox DeviceSelectList;
        private System.Windows.Forms.TextBox DeviceDetails;
        private System.Windows.Forms.OpenFileDialog LibBrowseDialog;
        private System.Windows.Forms.Button LibBrowseButton;
        private System.Windows.Forms.Button LibOpenButton;
    }
}