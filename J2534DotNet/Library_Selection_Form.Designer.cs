namespace J2534
{
    partial class LibrarySelectionForm
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
            this.DeviceDetails = new System.Windows.Forms.TextBox();
            this.LibBrowseDialog = new System.Windows.Forms.OpenFileDialog();
            this.LibBrowseButton = new System.Windows.Forms.Button();
            this.LibOpenButton = new System.Windows.Forms.Button();
            this.LibCancelButton = new System.Windows.Forms.Button();
            this.DeviceSelectList = new System.Windows.Forms.CheckedListBox();
            this.SuspendLayout();
            // 
            // DeviceDetails
            // 
            this.DeviceDetails.Location = new System.Drawing.Point(351, 13);
            this.DeviceDetails.Multiline = true;
            this.DeviceDetails.Name = "DeviceDetails";
            this.DeviceDetails.Size = new System.Drawing.Size(472, 183);
            this.DeviceDetails.TabIndex = 1;
            // 
            // LibBrowseDialog
            // 
            this.LibBrowseDialog.Filter = "Dynamic Link Libraries|*.dll";
            this.LibBrowseDialog.InitialDirectory = ".";
            // 
            // LibBrowseButton
            // 
            this.LibBrowseButton.Location = new System.Drawing.Point(270, 173);
            this.LibBrowseButton.Name = "LibBrowseButton";
            this.LibBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.LibBrowseButton.TabIndex = 2;
            this.LibBrowseButton.Text = "Browse";
            this.LibBrowseButton.UseVisualStyleBackColor = true;
            this.LibBrowseButton.Click += new System.EventHandler(this.LibBrowseButton_Click);
            // 
            // LibOpenButton
            // 
            this.LibOpenButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.LibOpenButton.Location = new System.Drawing.Point(13, 173);
            this.LibOpenButton.Name = "LibOpenButton";
            this.LibOpenButton.Size = new System.Drawing.Size(75, 23);
            this.LibOpenButton.TabIndex = 3;
            this.LibOpenButton.Text = "Open";
            this.LibOpenButton.UseVisualStyleBackColor = true;
            this.LibOpenButton.Click += new System.EventHandler(this.LibOpenButton_Click);
            // 
            // LibCancelButton
            // 
            this.LibCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.LibCancelButton.Location = new System.Drawing.Point(13, 204);
            this.LibCancelButton.Name = "LibCancelButton";
            this.LibCancelButton.Size = new System.Drawing.Size(75, 23);
            this.LibCancelButton.TabIndex = 4;
            this.LibCancelButton.Text = "Cancel";
            this.LibCancelButton.UseVisualStyleBackColor = true;
            this.LibCancelButton.Click += new System.EventHandler(this.LibCancelButton_Click);
            // 
            // DeviceSelectList
            // 
            this.DeviceSelectList.CheckOnClick = true;
            this.DeviceSelectList.FormattingEnabled = true;
            this.DeviceSelectList.Location = new System.Drawing.Point(13, 13);
            this.DeviceSelectList.Name = "DeviceSelectList";
            this.DeviceSelectList.Size = new System.Drawing.Size(332, 154);
            this.DeviceSelectList.TabIndex = 6;
            this.DeviceSelectList.SelectedIndexChanged += new System.EventHandler(this.UpdateDeviceDetails);
            // 
            // Device_Selection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.LibCancelButton;
            this.ClientSize = new System.Drawing.Size(824, 239);
            this.ControlBox = false;
            this.Controls.Add(this.DeviceSelectList);
            this.Controls.Add(this.LibCancelButton);
            this.Controls.Add(this.LibOpenButton);
            this.Controls.Add(this.LibBrowseButton);
            this.Controls.Add(this.DeviceDetails);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Device_Selection";
            this.ShowIcon = false;
            this.Text = "Select 2534 Device(s) to Open";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox DeviceDetails;
        private System.Windows.Forms.OpenFileDialog LibBrowseDialog;
        private System.Windows.Forms.Button LibBrowseButton;
        private System.Windows.Forms.Button LibOpenButton;
        private System.Windows.Forms.Button LibCancelButton;
        private System.Windows.Forms.CheckedListBox DeviceSelectList;
    }
}