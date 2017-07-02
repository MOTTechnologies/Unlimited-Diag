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
            this.J2534TreeView = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // DeviceDetails
            // 
            this.DeviceDetails.BackColor = System.Drawing.SystemColors.Menu;
            this.DeviceDetails.Location = new System.Drawing.Point(420, 12);
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
            this.LibBrowseButton.Location = new System.Drawing.Point(156, 353);
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
            this.LibOpenButton.Location = new System.Drawing.Point(817, 374);
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
            this.LibCancelButton.Location = new System.Drawing.Point(736, 374);
            this.LibCancelButton.Name = "LibCancelButton";
            this.LibCancelButton.Size = new System.Drawing.Size(75, 23);
            this.LibCancelButton.TabIndex = 4;
            this.LibCancelButton.Text = "Cancel";
            this.LibCancelButton.UseVisualStyleBackColor = true;
            this.LibCancelButton.Click += new System.EventHandler(this.LibCancelButton_Click);
            // 
            // J2534TreeView
            // 
            this.J2534TreeView.Location = new System.Drawing.Point(12, 12);
            this.J2534TreeView.Name = "J2534TreeView";
            this.J2534TreeView.Size = new System.Drawing.Size(305, 298);
            this.J2534TreeView.TabIndex = 8;
            // 
            // LibrarySelectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.LibCancelButton;
            this.ClientSize = new System.Drawing.Size(904, 410);
            this.ControlBox = false;
            this.Controls.Add(this.J2534TreeView);
            this.Controls.Add(this.LibCancelButton);
            this.Controls.Add(this.LibOpenButton);
            this.Controls.Add(this.LibBrowseButton);
            this.Controls.Add(this.DeviceDetails);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LibrarySelectionForm";
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
        private System.Windows.Forms.TreeView J2534TreeView;
    }
}