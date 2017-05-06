#region Copyright (c) 2010, Michael Kelly
/* 
 * Copyright (c) 2010, Michael Kelly
 * michael.e.kelly@gmail.com
 * http://michael-kelly.com/
 * 
 * All rights reserved.
 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
 * Neither the name of the organization nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * 
 */
#endregion License
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using J2534DotNet;
using SAE;

namespace UnlimitedDiag
{
    public partial class Root_Form : Form
    {
        List<J2534PhysicalDevice> PhysicalDevices;
        public Root_Form()
        {
            InitializeComponent();
            PhysicalDevices = J2534Discovery.Discover();
        }

        private void CmdDetectVehicleClick(object sender, EventArgs e)
        {
            if (!PhysicalDevices.Any())
                return;
            if (!PhysicalDevices[0].IsConnected)
                return;

            Channel Ch = PhysicalDevices[0].ConstructChannel(J2534PROTOCOL.ISO15765, J2534BAUD.ISO15765, J2534CONNECTFLAG.NONE);

            if (Ch == null)
                return;

            Ch.StartMsgFilter(new MessageFilter(COMMONFILTER.STANDARDISO15765, new List<byte>{ 0x00, 0x00, 0x07, 0xE0 }));
            //Ch.SetConfig(new J2534DotNet.SConfig(J2534PARAMETER.LOOP_BACK, 0));

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
