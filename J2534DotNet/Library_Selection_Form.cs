using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO; 

namespace J2534
{
    public partial class LibrarySelectionForm : Form
    {
        public LibrarySelectionForm()
        {
            InitializeComponent();
            J2534TreeView.BeginUpdate();
            foreach(J2534DLL Library in J2534Discovery.Librarys)
            {
                TreeNode NewRootNode = new TreeNode(Library.FileName);
                NewRootNode.Tag = Library;
                foreach (J2534PhysicalDevice Device in J2534Discovery.PhysicalDevices.Where(Device => Device.Library == Library))
                {
                    TreeNode ChildNode = new TreeNode(Device.DeviceName);
                    ChildNode.Tag = Device;
                    NewRootNode.Nodes.Add(ChildNode);
                }
                J2534TreeView.Nodes.Add(NewRootNode);
            }

            //AvailableDevices  = J2534Discovery.GetRegisteryEntries();
            //SelectedDevices = new List<J2534RegisteryEntry>();

            //foreach (J2534RegisteryEntry device in AvailableDevices)
            //    DeviceSelectList.Items.Add(device.Vendor + " - " + device.Name);

            //DeviceSelectList.SelectedIndex = 0;
            //UpdateDeviceDetails(new object(), new EventArgs());
        }

        private void LibBrowseButton_Click(object sender, EventArgs e)
        {
            DialogResult user_lib = LibBrowseDialog.ShowDialog();
            if(user_lib == DialogResult.OK)
            {
                //if(AvailableDevices.Find(d => d.FunctionLibrary == LibBrowseDialog.FileName) == null)
                //{
                //    J2534RegisteryEntry new_device = new J2534RegisteryEntry();
                //    new_device.ConfigApplication = "Unknown";
                //    new_device.Vendor = "USER DEFINED";
                //    new_device.Name = LibBrowseDialog.SafeFileName;
                //    new_device.FunctionLibrary = LibBrowseDialog.FileName;

                //    AvailableDevices.Add(new_device);
                //    DeviceSelectList.Items.Add(new_device.Vendor + " - " + new_device.Name);
                //}
            }
        }

        private void UpdateDeviceDetails(object sender, EventArgs e)
        {
            //J2534RegisteryEntry selected_device = AvailableDevices[DeviceSelectList.SelectedIndex];

            //DeviceDetails.Text =  " Config Application\t" + selected_device.ConfigApplication + "\r\n";
            //DeviceDetails.Text += " Function Library\t" + selected_device.FunctionLibrary + "\r\n\r\n";
            //DeviceDetails.Text += " Protocol\t\tChannels\r\n";
            //DeviceDetails.Text += " CAN\t\t" + selected_device.CANChannels + "\r\n";
            //DeviceDetails.Text += " ISO15765\t" + selected_device.ISO15765Channels + "\r\n";
            //DeviceDetails.Text += " ISO14230\t" + selected_device.ISO14230Channels + "\r\n";
            //DeviceDetails.Text += " ISO9141\t\t" + selected_device.ISO9141Channels + "\r\n";
            //DeviceDetails.Text += " J1850PWM\t" + selected_device.J1850PWMChannels + "\r\n";
            //DeviceDetails.Text += " J1850VPW\t" + selected_device.J1850VPWChannels + "\r\n";
            //DeviceDetails.Text += " SCI_A_ENGINE\t" + selected_device.SCI_A_ENGINEChannels + "\r\n";
            //DeviceDetails.Text += " SCI_A_TRANS\t" + selected_device.SCI_A_TRANSChannels + "\r\n";
            //DeviceDetails.Text += " SCI_B_ENGINE\t" + selected_device.SCI_B_ENGINEChannels + "\r\n";
            //DeviceDetails.Text += " SCI_B_TRANS\t" + selected_device.SCI_B_TRANSChannels;
        }

        private void LibOpenButton_Click(object sender, EventArgs e)
        {
            //SelectedDevices.Clear();
            //foreach(int i in DeviceSelectList.CheckedIndices)
            //    SelectedDevices.Add(AvailableDevices[i]);
            //this.DialogResult = DialogResult.OK;
            //this.Close();
        }

        private void LibCancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
