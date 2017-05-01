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
            SAEDiag Diagnostic = new SAEDiag();
            if(PhysicalDevices[0].IsConnected)
            {
                Channel Ch = PhysicalDevices[0].ConstructChannel();

                Ch.ProtocolID = Protocols.ISO15765;
                Ch.Baud = (int)BaudRate.ISO15765;

                Ch.FilterList[0].PlainISO15765(0x07, 0xE0);

                if (!Ch.IsConnected)
                {
                    if (Ch.Connect())
                        MessageBox.Show("This thing didnt connect!");

                    Ch.StartMsgFilter(0);
                }
                if (Diagnostic.Ping(Ch))
                    MessageBox.Show("We have a successful ping!");
            }
        }

        /*
         * 
         *  Example 2:
         *      Use the J2534 protocol to send and receive a message (w/o error checking)
         * 
         */
        //private void SendReceiveNoErrorChecking(object sender, EventArgs e)
        //{
        //    J2534 passThru = new J2534();

        //    // Find all of the installed J2534 passthru devices
        //    List<J2534RegisteredDevice> availableJ2534Devices = J2534Discovery.GetRegisteredDevices();

        //    // We will always choose the first J2534 device in the list, if there are multiple devices
        //    //   installed, you should do something more intelligent.
        //    passThru.LoadLibrary(availableJ2534Devices[0]);

        //    // Attempt to open a communication link with the pass thru device
        //    int deviceId = 0;
        //    passThru.Open(ref deviceId);

        //    // Open a new channel configured for ISO15765 (CAN)
        //    int channelId = 0;
        //    passThru.Connect(deviceId, Protocols.ISO15765, ConnectFlag.NONE, BaudRate.ISO15765, ref channelId);

        //    // Set up a message filter to watch for response messages
        //    int filterId = 0;
        //    PassThruMsg maskMsg = new PassThruMsg(
        //        Protocols.ISO15765,
        //        TxFlag.ISO15765_FRAME_PAD,
        //        new byte[] { 0xff, 0xff, 0xff, 0xff });
        //    PassThruMsg patternMsg = new PassThruMsg(
        //        Protocols.ISO15765,
        //        TxFlag.ISO15765_FRAME_PAD,
        //        new byte[] { 0x00, 0x00, 0x07, 0xE8});
        //    PassThruMsg flowControlMsg = new PassThruMsg(
        //        Protocols.ISO15765,
        //        TxFlag.ISO15765_FRAME_PAD,
        //        new byte[] { 0x00, 0x00, 0x07, 0xE0});
        //    passThru.StartMsgFilter(channelId, FilterEnum.FLOW_CONTROL_FILTER, ref maskMsg, ref patternMsg, ref flowControlMsg, ref filterId);

        //    // Clear out the response buffer so we know we're getting the freshest possible data
        //    passThru.ClearRxBuffer(channelId);

        //    // Finally we can send the message!
        //    PassThruMsg txMsg = new PassThruMsg(
        //        Protocols.ISO15765,
        //        TxFlag.ISO15765_FRAME_PAD,
        //        new byte[] { 0x00, 0x00, 0x07, 0xdf, 0x01, 0x00 });
        //    int numMsgs = 1;
        //    passThru.WriteMsgs(channelId, ref txMsg, 50);

        //    // Read messages in a loop until we either timeout or we receive data
        //    List<PassThruMsg> rxMsgs = new List<PassThruMsg>();
        //    J2534Err status = J2534Err.STATUS_NOERROR;
        //    numMsgs = 1;
        //    while (J2534Err.STATUS_NOERROR == status)
        //        status = passThru.ReadMsgs(channelId, ref rxMsgs, ref numMsgs, 200);

        //    // If we received data, we want to extract the data of interest.  I'm removing the reflection of the transmitted message.
        //    List<byte> responseData;
        //    if ((J2534Err.ERR_BUFFER_EMPTY == status || J2534Err.ERR_TIMEOUT == status) && rxMsgs.Count > 1)
        //    {
        //        responseData = rxMsgs[rxMsgs.Count - 1].Data.ToList();
        //        responseData.RemoveRange(0, txMsg.Data.Length);
        //    }

        //    //
        //    //
        //    // Now do something with the data!
        //    //
        //    //

        //    // Disconnect this channel
        //    passThru.Disconnect(channelId);

        //    // When we are done with the device, we can free the library.
        //    passThru.FreeLibrary();
        //}

        /*
         * 
         *  Use the J2534 protocol to read voltage
         * 
         */
        private void CmdReadVoltageClick(object sender, EventArgs e)
        {
            //J2534 passThru = new J2534();
            //double voltage = 0;

            //// Find all of the installed J2534 passthru devices
            //List<J2534RegisteredDevice> availableJ2534Devices = J2534Discovery.GetRegisteredDevices();
            //if (availableJ2534Devices.Count == 0)
            //{
            //    MessageBox.Show("Could not find any installed J2534 devices.");
            //    return;
            //}

            //// We will always choose the first J2534 device in the list, if there are multiple devices
            ////   installed, you should do something more intelligent.
            //passThru.LoadLibrary(availableJ2534Devices[0]);

            //ObdComm comm = new ObdComm(passThru);
            //if (!comm.DetectProtocol())
            //{
            //    MessageBox.Show(String.Format("Error connecting to device. Error: {0}", comm.GetLastError()));
            //    comm.Disconnect();
            //    return;
            //}
            //if (!comm.GetBatteryVoltage(ref voltage))
            //{
            //    MessageBox.Show(String.Format("Error reading voltage.  Error: {0}", comm.GetLastError()));
            //    comm.Disconnect();
            //    return;
            //}
            //comm.Disconnect();

            //// When we are done with the device, we can free the library.
            //passThru.FreeLibrary();
            //txtVoltage.Text = voltage + @" V";
        }

        private void CmdReadVinClick(object sender, EventArgs e)
        {
            //J2534 passThru = new J2534();
            //string vin = "";

            //// Find all of the installed J2534 passthru devices
            //List<J2534RegisteredDevice> availableJ2534Devices = J2534Discovery.GetRegisteredDevices();
            //if (availableJ2534Devices.Count == 0)
            //{
            //    MessageBox.Show("Could not find any installed J2534 devices.");
            //    return;
            //}

            //// We will always choose the first J2534 device in the list, if there are multiple devices
            ////   installed, you should do something more intelligent.
            //passThru.LoadLibrary(availableJ2534Devices[0]);

            //ObdComm comm = new ObdComm(passThru);
            //if (!comm.DetectProtocol())
            //{
            //    MessageBox.Show(String.Format("Error connecting to device. Error: {0}", comm.GetLastError()));
            //    comm.Disconnect();
            //    return;
            //}
            //if (!comm.GetVin(ref vin))
            //{
            //    MessageBox.Show(String.Format("Error reading VIN.  Error: {0}", comm.GetLastError()));
            //    comm.Disconnect();
            //    return;
            //}
            //comm.Disconnect();

            //// When we are done with the device, we can free the library.
            //passThru.FreeLibrary();
            //txtReadVin.Text = vin;
        }
    }
}
