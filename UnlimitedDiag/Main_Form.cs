using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using J2534;
using SAE;
using System.Runtime.InteropServices;

namespace UnlimitedDiag
{
    public partial class Main_Form : Form
    {
        List<J2534Device> PhysicalDevices;
        public Main_Form()
        {
            InitializeComponent();

            PhysicalDevices = J2534Discovery.OpenEverything();
            List<J1979Session> S = SAEDiscovery.ConnectEverything(PhysicalDevices);

        }

        private void CmdDetectVehicleClick(object sender, EventArgs e)
        {
            if (!PhysicalDevices.Any())
                return;
            if (!PhysicalDevices[0].IsConnected)
                return;
            /* SAESession DiagSession = new SAESession(PhysicalDevices[0]); //Will do ping discovery on construction
             * Mode01Results Results = DiagSession.Mode01();
            */
            Channel Ch = PhysicalDevices[0].ConstructChannel(J2534PROTOCOL.ISO15765, J2534BAUD.ISO15765, J2534CONNECTFLAG.NONE);

            if (Ch == null)
                return;

            Ch.StartMsgFilter(new MessageFilter(COMMONFILTER.STANDARDISO15765, new byte[] { 0x00, 0x00, 0x07, 0xE0 }));
            Ch.SetConfig(J2534PARAMETER.LOOP_BACK, 0);

            SAE.SAEDiag Diagnostic = new SAE.SAEDiag();

            if (Diagnostic.Ping(Ch))
                MessageBox.Show("We have a successful ping!");
            Ch.Disconnect();
        }

        private void CmdReadVoltageClick(object sender, EventArgs e)
        {
            float voltage = 0;
            if (!PhysicalDevices.Any())
                return;
            if (!PhysicalDevices[0].IsConnected)
                return;
            voltage = (float)PhysicalDevices[0].MeasureBatteryVoltage() / 1000;
            txtVoltage.Text = voltage.ToString("F3") + "v";
        }

        private void CmdReadVinClick(object sender, EventArgs e)
        {
            //txtReadVin.Text = vin;
        }
    }
}
