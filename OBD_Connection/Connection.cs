using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J2534DotNet;

namespace OBD_Connection
{
    public class Channel
    {
        const bool SUCCESS = false;
        const bool FAIL = true;

        public J2534 Device;
        public Protocols Protocol;
        public ConnectFlag Flags;
        public TxFlag TFlags;
        public int BaudRate;
        public int ChannelID;
        public int TxTimeout;
        public int RxTimeout;
        public string LastStatus;
        public bool IsOpen;

        public bool Open()
        {
            J2534Err status = Device.Connect(C.Protocol, ConnectFlag.NONE, C.BaudRate, ref C.ChannelID);
            if (status == J2534Err.STATUS_NOERROR)
            {
                IsOpen = true;
                return SUCCESS;
            }
            IsOpen = false;
            return FAIL;
        }
        public bool SendMessage(byte[] msg)
        {
            PassThruMsg pMsg = new PassThruMsg(Protocol, TFlags, msg);
            J2534Err status = Device.WriteMsgs(ChannelID, ref pMsg, TxTimeout);
            LastStatus = status.ToString();
            if (status == J2534Err.STATUS_NOERROR)
                return SUCCESS;
            else
                return FAIL;
        }

        public bool SendMessage(List<byte[]> msg)
        {
            List<PassThruMsg> pMsgs = new List<PassThruMsg>();

            foreach (byte[] b in msg)
                pMsgs.Add(new PassThruMsg(Protocol, TFlags, b));

            int NumOfMsgs = pMsgs.Count;
            J2534Err status = Device.WriteMsgs(ChannelID, ref pMsgs, ref NumOfMsgs, TxTimeout);
            LastStatus = status.ToString();
            if (status == J2534Err.STATUS_NOERROR)
                return SUCCESS;
            else
                return FAIL;
        }
        public bool GetMessage(ref byte[] msg)
        {
            PassThruMsg pMsg = new PassThruMsg(Protocol, TFlags,msg);
            J2534Err status = Device.WriteMsgs(ChannelID, ref pMsg, TxTimeout);
            LastStatus = status.ToString();
            if (status == J2534Err.STATUS_NOERROR)
            {
                msg 
                return SUCCESS;
            }

            return FAIL;
        }
        public bool GetMessage(List<byte[]> msg)
        {
            return FAIL;
        }
    }

    public class Connection_Class
    {
        public List<J2534> ConnectedDevices;
        public List<Channel> Channels;
        private Device_Selection DeviceSelector;

        //When a new Connection_class is constructed, all devices for the session will be loaded.
        public Connection_Class()
        {
            //This prompts the user to select a 2534 library(s) from the registry or browse.
            DeviceSelector = new Device_Selection();
            //Should check dialogresult and end program if no selection is made
            DeviceSelector.ShowDialog();

            //Here we attempt to load the unmanaged library and open the target device.  If both are
            //successful, the connection is added to "List<J2534> ConnectedDevices".
            ConnectedDevices = new List<J2534>();
            foreach(J2534Device ListedDevice in DeviceSelector.SelectedDevices)
            {
                //TODO: Implement error messaging/handling for a failed load
                //      Also, should release the library if load failed
                J2534 ConnectedDevice = new J2534();
#warning Unmanaged resource is never released.  Dispose needs to be added
                if (ConnectedDevice.LoadLibrary(ListedDevice))
                    if (ConnectedDevice.Open() == J2534Err.STATUS_NOERROR)
                        ConnectedDevices.Add(ConnectedDevice);
            }

            //Next we will attempt to open every channel known.  If the attempt succeeds, it is added to the
            //"List<Channel> Channels" and then closed.  The channel is closed to avoid physical layer conflicts.
            //Channels = new List<Channel>();
            //foreach(J2534 Device in ConnectedDevices)
            //{
            //    Channel C = new Channel();
            //    C.Device = Device;
            //    C.Protocol = ProtocolID.ISO15765;
            //    C.BaudRate = (int)BaudRate.ISO15765;
            //    J2534Err status = Device.Connect(C.Protocol, ConnectFlag.NONE, C.BaudRate, ref C.ChannelID);
            //    if (status == J2534Err.STATUS_NOERROR)
            //    {
            //        Device.Disconnect(C.ChannelID);
            //        Channels.Add(C);
            //        C = new Channel();
            //    }

            //    C.Device = Device;
            //    C.Protocol = ProtocolID.J1850PWM;
            //    C.BaudRate = (int)BaudRate.J1850PWM;
            //    status = Device.Connect(C.Protocol, ConnectFlag.NONE, C.BaudRate, ref C.ChannelID);
            //    if (status == J2534Err.STATUS_NOERROR)
            //    {
            //        Device.Disconnect(C.ChannelID);
            //        Channels.Add(C);
            //        C = new Channel();
            //    }

            //    C.Device = Device;
            //    C.Protocol = ProtocolID.J1850VPW;
            //    C.BaudRate = (int)BaudRate.J1850VPW;
            //    status = Device.Connect(C.Protocol, ConnectFlag.NONE, C.BaudRate, ref C.ChannelID);
            //    if (status == J2534Err.STATUS_NOERROR)
            //    {
            //        Device.Disconnect(C.ChannelID);
            //        Channels.Add(C);
            //        C = new Channel();
            //    }

            //    C.Protocol = ProtocolID.ISO9141;
            //    C.BaudRate = (int)BaudRate.ISO9141;
            //    status = Device.Connect(C.Protocol, ConnectFlag.NONE, C.BaudRate, ref C.ChannelID);
            //    if (status == J2534Err.STATUS_NOERROR)
            //    {
            //        Device.Disconnect(C.ChannelID);
            //        Channels.Add(C);
            //        C = new Channel();
            //    }

            //    C.Protocol = ProtocolID.ISO14230;
            //    C.BaudRate = (int)BaudRate.ISO14230;
            //    status = Device.Connect(C.Protocol, ConnectFlag.NONE, C.BaudRate, ref C.ChannelID);
            //    if (status == J2534Err.STATUS_NOERROR)
            //    {
            //        Device.Disconnect(C.ChannelID);
            //        Channels.Add(C);
            //        C = new Channel();
            //    }
            //    C.Device = Device;
            //    C.Protocol = ProtocolID.CAN;
            //    C.BaudRate = (int)BaudRate.CAN_500000;
            //    status = Device.Connect(C.Protocol, ConnectFlag.NONE, C.BaudRate, ref C.ChannelID);
            //    if (status == J2534Err.STATUS_NOERROR)
            //    {
            //        Device.Disconnect(C.ChannelID);
            //        Channels.Add(C);
            //        C = new Channel();
            //    }

            //    C.Device = Device;
            //    C.Protocol = ProtocolID.SCI_A_ENGINE;
            //    C.BaudRate = (int)BaudRate.SCI_7812;
            //    status = Device.Connect(C.Protocol, ConnectFlag.NONE, C.BaudRate, ref C.ChannelID);
            //    if (status == J2534Err.STATUS_NOERROR)
            //    {
            //        Device.Disconnect(C.ChannelID);
            //        Channels.Add(C);
            //        C = new Channel();
            //    }

            //    C.Device = Device;
            //    C.Protocol = ProtocolID.SCI_B_ENGINE;
            //    C.BaudRate = (int)BaudRate.SCI_7812;
            //    status = Device.Connect(C.Protocol, ConnectFlag.NONE, C.BaudRate, ref C.ChannelID);
            //    if (status == J2534Err.STATUS_NOERROR)
            //    {
            //        Device.Disconnect(C.ChannelID);
            //        Channels.Add(C);
            //        C = new Channel();
            //    }
            //    C.Device = Device;
            //    C.Protocol = ProtocolID.SCI_A_TRANS;
            //    C.BaudRate = (int)BaudRate.SCI_7812;
            //    status = Device.Connect(C.Protocol, ConnectFlag.NONE, C.BaudRate, ref C.ChannelID);
            //    if (status == J2534Err.STATUS_NOERROR)
            //    {
            //        Device.Disconnect(C.ChannelID);
            //        Channels.Add(C);
            //        C = new Channel();
            //    }
            //    C.Device = Device;
            //    C.Protocol = ProtocolID.SCI_B_TRANS;
            //    C.BaudRate = (int)BaudRate.SCI_7812;
            //    status = Device.Connect(C.Protocol, ConnectFlag.NONE, C.BaudRate, ref C.ChannelID);
            //    if (status == J2534Err.STATUS_NOERROR)
            //    {
            //        Device.Disconnect(C.ChannelID);
            //        Channels.Add(C);
            //        C = new Channel();
            //    }
            //}

        }
    }
}
