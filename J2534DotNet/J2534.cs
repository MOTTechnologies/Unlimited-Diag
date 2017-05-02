using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace J2534DotNet
{
    internal class J2534Lib:IDisposable
    {
        private bool disposed = false;
        internal string FileName;
        internal bool IsLoaded;
        internal J2534APIWrapper API;

        internal J2534Lib(string LibFile)
        {
            FileName = LibFile;
            API = new J2534DotNet.J2534APIWrapper();
            Load();
        }

        internal bool Load()
        {
            IsLoaded = API.LoadJ2534Library(FileName);
            return IsLoaded;
        }

        internal bool Free()
        {
            IsLoaded = API.FreeLibrary();   //Does this return a true or false????
            return IsLoaded;
        }

        internal J2534PhysicalDevice ConstructDevice()
        {
            J2534PhysicalDevice dev = new J2534DotNet.J2534PhysicalDevice(this);
            if (dev.IsConnected)
                return dev;
            return null;
        }

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //
            API.FreeLibrary();
            disposed = true;
        }
    }

    public class J2534PhysicalDevice
    {
        public J2534ERR Status;
        internal int DeviceID;
        public bool IsConnected;
        internal J2534Lib Library;
        public string FirmwareVersion;
        public string LibraryVersion;
        public string APIVersion;

        internal J2534PhysicalDevice(J2534Lib library)
        {
            Library = library;
            ConnectToDevice();
        }

        public bool ConnectToDevice()
        {
            int nada = 0;
            Status = (J2534ERR)Library.API.Open(ref nada, ref DeviceID);
            if (Status == J2534ERR.STATUS_NOERROR)
            {
                IsConnected = true;
                GetVersion();
                return false;
            }
            return true;
        }

        public bool DisconnectDevice()
        {
            Status = (J2534ERR)Library.API.Close(DeviceID);
            if (Status == J2534ERR.STATUS_NOERROR)
            {
                IsConnected = false;
                return false;
            }
            return true;
        }

        public bool SetProgrammingVoltage(J2534PIN PinNumber, int Voltage)
        {
            Status = (J2534ERR)Library.API.SetProgrammingVoltage(DeviceID, (int)PinNumber, Voltage);
            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        private bool GetVersion()
        {
            IntPtr pFirmwareVersion = Marshal.AllocHGlobal(120);
            IntPtr pDllVersion = Marshal.AllocHGlobal(120);
            IntPtr pApiVersion = Marshal.AllocHGlobal(120);
            Status = (J2534ERR)Library.API.ReadVersion(DeviceID, pFirmwareVersion, pDllVersion, pApiVersion);
            if (Status == J2534ERR.STATUS_NOERROR)
            {
                FirmwareVersion = Marshal.PtrToStringAnsi(pFirmwareVersion);
                LibraryVersion = Marshal.PtrToStringAnsi(pDllVersion);
                APIVersion = Marshal.PtrToStringAnsi(pApiVersion);
            }

            Marshal.FreeHGlobal(pFirmwareVersion);
            Marshal.FreeHGlobal(pDllVersion);
            Marshal.FreeHGlobal(pApiVersion);

            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public string GetLastError()
        {
            string return_string = null;
            IntPtr pErrorDescription = Marshal.AllocHGlobal(120);
            Status = (J2534ERR)Library.API.GetLastError(pErrorDescription);
            if (Status == J2534ERR.STATUS_NOERROR)
                return_string = Marshal.PtrToStringAnsi(pErrorDescription);

            Marshal.FreeHGlobal(pErrorDescription);

            return return_string;
        }

        public int MeasureBatteryVoltage()
        {
            int voltage = 0;
            IntPtr input = IntPtr.Zero;
            IntPtr output = Marshal.AllocHGlobal(8);

            Status = (J2534ERR)Library.API.Ioctl(DeviceID, (int)J2534IOCTL.READ_VBATT, input, output);
            if (Status == J2534ERR.STATUS_NOERROR)
            {
                voltage = Marshal.ReadInt32(output);
            }

            Marshal.FreeHGlobal(output);

            return voltage;
        }

        public int MeasureProgrammingVoltage()
        {
            int voltage = 0;
            IntPtr input = IntPtr.Zero;
            IntPtr output = Marshal.AllocHGlobal(8);

            Status = (J2534ERR)Library.API.Ioctl(DeviceID, (int)J2534IOCTL.READ_PROG_VOLTAGE, input, output);
            if (Status == J2534ERR.STATUS_NOERROR)
            {
                voltage = Marshal.ReadInt32(output);
            }

            Marshal.FreeHGlobal(output);

            return voltage;
        }

        public Channel ConstructChannel(J2534PROTOCOL ProtocolID, J2534BAUD Baud, J2534CONNECTFLAG ConnectFlags)
        {
            return new Channel(this, ProtocolID, Baud, ConnectFlags);
        }


    }

    public class Channel
    {
        private J2534PhysicalDevice Device;
        private int ChannelID;
        public J2534ERR Status { get; private set; }
        public bool IsConnected { get; private set; }
        public J2534PROTOCOL ProtocolID { get; private set; }
        public int Baud { get; private set; }
        public J2534CONNECTFLAG ConnectFlags { get; internal set; }
        public List<J2534Message> RxMessages;
        public List<PeriodicMsg> PeriodicMsgList;
        public List<MessageFilter> FilterList;
        public int DefaultTxTimeout { get; set; }
        public int DefaultRxTimeout { get; set; }
        public J2534TXFLAG DefaultTxFlag { get; set; }
        public byte FiveBaudKeyword1 { get; set; }
        public byte FiveBaudKeyword2 { get; set; }

        internal Channel(J2534PhysicalDevice Device, J2534PROTOCOL ProtocolID, J2534BAUD Baud, J2534CONNECTFLAG ConnectFlags)
        {
            this.Device = Device;
            this.ProtocolID = ProtocolID;
            this.Baud = (int)Baud;
            this.ConnectFlags = ConnectFlags;
            Connect();
            Status = Device.Status;
            RxMessages = new List<J2534DotNet.J2534Message>();
            PeriodicMsgList = new List<PeriodicMsg>();
            FilterList = new List<J2534DotNet.MessageFilter>();
            DefaultTxTimeout = 50;
            DefaultRxTimeout = 250;
            DefaultTxFlag = J2534TXFLAG.NONE;
        }

        private void Connect()
        {
            Status = (J2534ERR)Device.Library.API.Connect(Device.DeviceID, (int)ProtocolID, (int)ConnectFlags, Baud, ref ChannelID);
        }

        public bool Disconnect()
        {
            Status = (J2534ERR)Device.Library.API.Disconnect(ChannelID);
            if (Status == J2534ERR.STATUS_NOERROR)
            {
                IsConnected = false;
                return false;
            }
            return true;
        }

        public bool GetMessage()
        {
            return GetMessages(1, DefaultRxTimeout);
        }

        /// <summary>
        /// Reads 'NumMsgs' messages from the input buffer and then the device.  Will block
        /// until it gets 'NumMsgs' messages, or 'DefaultRxTimeout' expires.
        /// </summary>
        /// <param name="NumMsgs"></param>
        /// <returns>Returns 'false' if successful</returns>
        public bool GetMessages(int NumMsgs)
        {
            return GetMessages(NumMsgs, DefaultRxTimeout);
        }

        /// <summary>
        /// Reads 'NumMsgs' messages from the input buffer and then the device.  Will block
        /// until it gets 'NumMsgs' messages, or 'Timeout' expires.
        /// </summary>
        /// <param name="NumMsgs"></param>
        /// <param name="Timeout"></param>
        /// <returns>Returns 'false' if successful</returns>
        public bool GetMessages(int NumMsgs, int Timeout)
        {
            int pNumMsgs = NumMsgs;
            IntPtr pMsgHeap = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(UnsafePassThruMsg)) * pNumMsgs);

            Status = (J2534ERR)Device.Library.API.ReadMsgs(ChannelID, pMsgHeap, ref pNumMsgs, DefaultRxTimeout);

            if (Status == J2534ERR.STATUS_NOERROR)
            {
                RxMessages.Clear();
                for (int i = 0; i < pNumMsgs; i++)
                {
                    IntPtr pNextMsg = (IntPtr)(Marshal.SizeOf(typeof(UnsafePassThruMsg)) * i + (int)pMsgHeap);
                    UnsafePassThruMsg uMsg = (UnsafePassThruMsg)Marshal.PtrToStructure(pNextMsg, typeof(UnsafePassThruMsg));
                    RxMessages.Add(ConvertPassThruMsg(uMsg));
                }
            }

            Marshal.FreeHGlobal(pMsgHeap);

            if(Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        /// <summary>
        /// Sends a single message 'Message'
        /// </summary>
        /// <param name="Message"></param>
        /// <returns>Returns 'false' if successful</returns>
        public bool SendMessage(List<byte> Message)
        {
            List<J2534Message> MessageList = new List<J2534DotNet.J2534Message>();
            MessageList.Add(new J2534DotNet.J2534Message(ProtocolID, DefaultTxFlag, Message));

            return SendMessages(MessageList);
        }

        /// <summary>
        /// Sends all messages contained in 'MsgList'
        /// </summary>
        /// <returns>Returns 'false' if successful</returns>
        public bool SendMessages(List<J2534Message> Messages)
        {
            int pNumMsgs = Messages.Count;

            IntPtr pMsg_heap = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(UnsafePassThruMsg)) * pNumMsgs);

            for (int i = 0; i < pNumMsgs; i++)
            {
                IntPtr pNextMsg = (IntPtr)(Marshal.SizeOf(typeof(UnsafePassThruMsg)) * i + (int)pMsg_heap);
                UnsafePassThruMsg u_msg = ConvertPassThruMsg(Messages[i]);
                Marshal.StructureToPtr(u_msg, pNextMsg, false);
            }

            Status = (J2534ERR)Device.Library.API.WriteMsgs(ChannelID, pMsg_heap, ref pNumMsgs, DefaultTxTimeout);

            Marshal.FreeHGlobal(pMsg_heap);


            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool StartPeriodicMessage(List<byte> Message, int Interval)
        {
            PeriodicMsg PeriodicMessage = new J2534DotNet.PeriodicMsg();
            PeriodicMessage.Message = new J2534DotNet.J2534Message(ProtocolID, DefaultTxFlag, Message);
            PeriodicMessage.Interval = Interval;
            //If success
            if (!StartPeriodicMsg(PeriodicMsgList.Count - 1))
                return false;
            //Otherwise, remove it from the list, and return fail
            PeriodicMsgList.RemoveAt(PeriodicMsgList.Count - 1);
            return true;
        }

        /// <summary>
        /// Starts the periodic message in 'PeriodicMsgList' referenced by 'Index'
        /// </summary>
        /// <param name="Index"></param>
        /// <returns>Returns 'false' if successful</returns>
        public bool StartPeriodicMsg(int Index)
        {
            int pMsgID = PeriodicMsgList[Index].MessageID;
            UnsafePassThruMsg uMsg = ConvertPassThruMsg(PeriodicMsgList[Index].Message);
            Status = (J2534ERR)Device.Library.API.StartPeriodicMsg(ChannelID, ref uMsg, ref pMsgID, PeriodicMsgList[Index].Interval);
            if(Status == J2534ERR.STATUS_NOERROR)
            {
                PeriodicMsgList[Index].MessageID = pMsgID;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Stops the periodic message in 'PeriodicMsgList' referenced by 'Index'.
        /// </summary>
        /// <param name="Index"></param>
        /// <returns>Returns 'false' if successful</returns>
        public bool StopPeriodicMsg(int Index)
        {
            int pMsgID = PeriodicMsgList[Index].MessageID;
            Status = (J2534ERR)Device.Library.API.StopPeriodicMsg(ChannelID, pMsgID);
            if (Status == J2534ERR.STATUS_NOERROR)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Starts a single message filter and if successful, adds it to the FilterList.
        /// </summary>
        /// <param name="Filter"></param>
        /// <returns>Returns false if successful</returns>
        public bool StartMsgFilter(MessageFilter Filter)
        {
            FilterList.Add(Filter);
            if(StartMsgFilter(FilterList.Count - 1))
            {
                FilterList.RemoveAt(FilterList.Count - 1);
                return true;
            }
            return false;
        }

        public bool StartMsgFilter(int Index)
        {
            UnsafePassThruMsg uMaskMsg = ConvertPassThruMsg(new J2534Message(ProtocolID, FilterList[Index].TxFlags, FilterList[Index].Mask));
            UnsafePassThruMsg uPatternMsg = ConvertPassThruMsg(new J2534Message(ProtocolID, FilterList[Index].TxFlags, FilterList[Index].Pattern));
            UnsafePassThruMsg uFlowControlMsg = ConvertPassThruMsg(new J2534Message(ProtocolID, FilterList[Index].TxFlags, FilterList[Index].FlowControl));
            int FID = FilterList[Index].FilterId;

            Status = (J2534ERR)Device.Library.API.StartMsgFilter(ChannelID,
                                                  (int)FilterList[Index].FilterType,
                                                  ref uMaskMsg,
                                                  ref uPatternMsg,
                                                  ref uFlowControlMsg,
                                                  ref FID);

            FilterList[Index].FilterId = FID;
            if (Status == J2534ERR.STATUS_NOERROR)
            {
                return false;
            }
            return true;
        }

        public bool StopMsgFilter(int Index)
        {
            Status = (J2534ERR)Device.Library.API.StopMsgFilter(ChannelID, FilterList[Index].FilterId);
            if (Status == J2534ERR.STATUS_NOERROR)
            {
                return false;
            }
            return true;
        }

        private UnsafePassThruMsg ConvertPassThruMsg(J2534Message Msg)
        {
            UnsafePassThruMsg uMsg = new UnsafePassThruMsg();

            uMsg.ProtocolID = (uint)Msg.ProtocolID;
            uMsg.RxStatus = (uint)Msg.RxFlags;
            uMsg.Timestamp = Msg.Timestamp;
            uMsg.TxFlags = (uint)Msg.TxFlags;
            uMsg.ExtraDataIndex = Msg.ExtraDataIndex;
            uMsg.DataSize = (uint)Msg.Data.Count;
            unsafe
            {
                for (int i = 0; i < Msg.Data.Count; i++)
                {
                    uMsg.Data[i] = Msg.Data[i];
                }
            }

            return uMsg;
        }

        private J2534Message ConvertPassThruMsg(UnsafePassThruMsg uMsg)
        {
            J2534Message Msg = new J2534Message();

            Msg.ProtocolID = (J2534PROTOCOL)uMsg.ProtocolID;
            Msg.RxFlags = (J2534RXFLAG)uMsg.RxStatus;
            Msg.Timestamp = uMsg.Timestamp;
            Msg.TxFlags = (J2534TXFLAG)uMsg.TxFlags;
            Msg.ExtraDataIndex = uMsg.ExtraDataIndex;
            unsafe
            {
                for (int i = 0; i < uMsg.DataSize; i++)
                {
                    Msg.Data.Add(uMsg.Data[i]);
                }
            }

            return Msg;
        }

        public int GetConfig(SConfig Parameter)
        {
            int value = 0;
            IntPtr input = Marshal.AllocHGlobal(16);

            Marshal.WriteInt32(input, 0, 1);
            Marshal.WriteInt32(input, 4, (int)input + 8);
            Marshal.WriteInt32(input, 8, (int)Parameter.Parameter);
            Marshal.WriteInt32(input, 12, Parameter.Value);

            Status = (J2534ERR)Device.Library.API.Ioctl(ChannelID, (int)J2534IOCTL.GET_CONFIG, input, IntPtr.Zero);

            if (Status == J2534ERR.STATUS_NOERROR)
            {
                //int num_of_params = Marshal.ReadInt32(input);
                IntPtr data_ptr = Marshal.ReadIntPtr(input, 4);
                //int parameter = Marshal.ReadInt32(data_ptr);
                value = Marshal.ReadInt32(data_ptr, 4);
            }

            Marshal.FreeHGlobal(input);

            return value;
        }

        public bool SetConfig(SConfig Parameter)
        {

            IntPtr input = Marshal.AllocHGlobal(16);
            IntPtr output = IntPtr.Zero;

            Marshal.WriteInt32(input, 0, 1);
            Marshal.WriteInt32(input, 4, (int)input + 8);
            Marshal.WriteInt32(input, 8, (int)Parameter.Parameter);
            Marshal.WriteInt32(input, 12, Parameter.Value);

            Status = (J2534ERR)Device.Library.API.Ioctl(ChannelID, (int)J2534IOCTL.SET_CONFIG, input, output);

            Marshal.FreeHGlobal(input);

            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool ClearTxBuffer()
        {
            IntPtr input = IntPtr.Zero;
            IntPtr output = IntPtr.Zero;

            Status = (J2534ERR)Device.Library.API.Ioctl(ChannelID, (int)J2534IOCTL.CLEAR_TX_BUFFER, input, output);
            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool ClearRxBuffer()
        {
            IntPtr input = IntPtr.Zero;
            IntPtr output = IntPtr.Zero;

            Status = (J2534ERR)Device.Library.API.Ioctl(ChannelID, (int)J2534IOCTL.CLEAR_RX_BUFFER, input, output);
            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool ClearPeriodicMsgs()
        {
            IntPtr input = IntPtr.Zero;
            IntPtr output = IntPtr.Zero;

            Status = (J2534ERR)Device.Library.API.Ioctl(ChannelID, (int)J2534IOCTL.CLEAR_PERIODIC_MSGS, input, output);
            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool ClearMsgFilters()
        {
            IntPtr input = IntPtr.Zero;
            IntPtr output = IntPtr.Zero;

            Status = (J2534ERR)Device.Library.API.Ioctl(ChannelID, (int)J2534IOCTL.CLEAR_MSG_FILTERS, input, output);
            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool ClearFunctMsgLookupTable()
        {
            IntPtr input = IntPtr.Zero;
            IntPtr output = IntPtr.Zero;

            Status = (J2534ERR)Device.Library.API.Ioctl(ChannelID, (int)J2534IOCTL.CLEAR_FUNCT_MSG_LOOKUP_TABLE, input, output);

            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool AddToFunctMsgLookupTable(int Address)
        {
            IntPtr input = Marshal.AllocHGlobal(12);
            IntPtr output = IntPtr.Zero;

            Marshal.WriteInt32(input, 0, 4);
            Marshal.WriteInt32(input, 4, (int)input + 8);
            Marshal.WriteInt32(input, 8, Address);

            Status = (J2534ERR)Device.Library.API.Ioctl(ChannelID, (int)J2534IOCTL.ADD_TO_FUNCT_MSG_LOOKUP_TABLE, input, output);

            Marshal.FreeHGlobal(input);

            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool DeleteFromFunctMsgLookupTable(int Address)
        {
            IntPtr input = Marshal.AllocHGlobal(12);
            IntPtr output = IntPtr.Zero;

            Marshal.WriteInt32(input, 0, 4);
            Marshal.WriteInt32(input, 4, (int)input + 8);
            Marshal.WriteInt32(input, 8, Address);

            Status = (J2534ERR)Device.Library.API.Ioctl(ChannelID, (int)J2534IOCTL.DELETE_FROM_FUNCT_MSG_LOOKUP_TABLE, input, output);

            Marshal.FreeHGlobal(input);

            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool FiveBaudInit(int TargetAddress)
        {
            IntPtr input = Marshal.AllocHGlobal(9);
            IntPtr output = Marshal.AllocHGlobal(10);

            Marshal.WriteInt32(input, 0, 1);
            Marshal.WriteInt32(input, 4, (int)input + 8);
            Marshal.WriteInt32(input, 8, TargetAddress);

            Marshal.WriteInt32(output, 0, 2);
            Marshal.WriteInt32(output, 4, (int)output + 8);
            Marshal.WriteInt32(output, 8, FiveBaudKeyword1);
            Marshal.WriteInt32(output, 9, FiveBaudKeyword2);

            Status = (J2534ERR)Device.Library.API.Ioctl(ChannelID, (int)J2534IOCTL.FIVE_BAUD_INIT, input, output);
            if(Status == J2534ERR.STATUS_NOERROR)
            {
                IntPtr data0 = Marshal.ReadIntPtr((IntPtr)((int)output + 4));
                FiveBaudKeyword1 = Marshal.ReadByte(data0);
                FiveBaudKeyword2 = Marshal.ReadByte((IntPtr)((int)data0 + 1));
            }
            Marshal.FreeHGlobal(input);
            Marshal.FreeHGlobal(output);

            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool FastInit(J2534Message TxMessage)
        {
            IntPtr input = IntPtr.Zero;
            IntPtr output = IntPtr.Zero;
            UnsafePassThruMsg uTxMsg = ConvertPassThruMsg(TxMessage);
            UnsafePassThruMsg uRxMsg = new UnsafePassThruMsg();

            Marshal.StructureToPtr(uTxMsg, input, true);
            Marshal.StructureToPtr(uRxMsg, output, true);

            J2534ERR returnValue = (J2534ERR)Device.Library.API.Ioctl(ChannelID, (int)J2534IOCTL.FAST_INIT, input, output);
            if (returnValue == J2534ERR.STATUS_NOERROR)
            {
                Marshal.PtrToStructure(output, uRxMsg);
                RxMessages.Clear();
                RxMessages.Add(ConvertPassThruMsg(uRxMsg));
            }
            Marshal.FreeHGlobal(input);
            Marshal.FreeHGlobal(output);
            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool SetProgrammingVoltage(J2534PIN PinNumber, int Voltage)
        {
            return Device.SetProgrammingVoltage(PinNumber, Voltage);
        }

        public int MeasureProgrammingVoltage()
        {
            return Device.MeasureProgrammingVoltage();
        }

        public int MeasureBatteryVoltage()
        {
            return Device.MeasureBatteryVoltage();
        }

    }
}
