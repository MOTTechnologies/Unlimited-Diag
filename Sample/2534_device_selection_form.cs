using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using J2534DotNet;

namespace UnlimitedDiag
{
    public partial class _2534_device_selection_form : Form
    {
        private List<J2534Device> availableJ2534Devices;

        public _2534_device_selection_form() : this(J2534Detect.ListDevices())
        {
        }

        public _2534_device_selection_form(List<J2534Device> device_list)
        {
            InitializeComponent();
            availableJ2534Devices = device_list;
            foreach (J2534Device device in availableJ2534Devices)
            {
                DeviceSelectList.Items.Add(device.Vendor + " - " + device.Name);
            }

        }

        private void LibBrowseButton_Click(object sender, EventArgs e)
        {
            DialogResult user_lib = LibBrowseDialog.ShowDialog();
            if(user_lib == DialogResult.OK)
            {
                if(availableJ2534Devices.Find(d => d.FunctionLibrary == LibBrowseDialog.FileName) == null)
                {
                    J2534Device new_device = new J2534Device();
                    new_device.ConfigApplication = "";
                    new_device.Vendor = "USER DEFINED";
                    new_device.Name = LibBrowseDialog.SafeFileName;
                    new_device.FunctionLibrary = LibBrowseDialog.FileName;

                    availableJ2534Devices.Add(new_device);
                    DeviceSelectList.Items.Add(new_device.Vendor + " - " + new_device.Name);
                }
            }
        }

        private void UpdateDeviceDetails(object sender, EventArgs e)
        {
            J2534Device selected_device = availableJ2534Devices[DeviceSelectList.SelectedIndex];
            DeviceDetails.Text = "\tConfig Application:\t" + selected_device.ConfigApplication + "\r\n";
            DeviceDetails.Text += "\tFunction Library:\t" + selected_device.FunctionLibrary + "\r\n\r\n";
            DeviceDetails.Text += "\tProtocol\t\tChannels\r\n";
            DeviceDetails.Text += "\tCAN\t\t" + selected_device.CANChannels + "\r\n";
            DeviceDetails.Text += "\tISO15765\t" + selected_device.ISO15765Channels + "\r\n";
            DeviceDetails.Text += "\tISO14230\t" + selected_device.ISO14230Channels + "\r\n";
            DeviceDetails.Text += "\tISO9141\t\t" + selected_device.ISO9141Channels + "\r\n";
            DeviceDetails.Text += "\tJ1850PWM\t" + selected_device.J1850PWMChannels + "\r\n";
            DeviceDetails.Text += "\tJ1850VPW\t" + selected_device.J1850VPWChannels + "\r\n";
            DeviceDetails.Text += "\tSCI_A_ENGINE\t" + selected_device.SCI_A_ENGINEChannels + "\r\n";
            DeviceDetails.Text += "\tSCI_A_TRANS\t" + selected_device.SCI_A_TRANSChannels + "\r\n";
            DeviceDetails.Text += "\tSCI_B_ENGINE\t" + selected_device.SCI_B_ENGINEChannels + "\r\n";
            DeviceDetails.Text += "\tSCI_B_TRANS\t" + selected_device.SCI_B_TRANSChannels;
        }

    }
}
