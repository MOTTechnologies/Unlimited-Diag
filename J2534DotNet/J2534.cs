using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace J2534DotNet
{
    internal class J2534Lib
    {
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
    }

    public class J2534PhysicalDevice
    {
        public J2534Err Status;
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
            Status = (J2534Err)Library.API.Open(ref nada, ref DeviceID);
            if (Status == J2534Err.STATUS_NOERROR)
            {
                IsConnected = true;
                GetVersion();
                return false;
            }
            return true;
        }

        public bool DisconnectDevice()
        {
            Status = (J2534Err)Library.API.Close(DeviceID);
            if (Status == J2534Err.STATUS_NOERROR)
            {
                IsConnected = false;
                return false;
            }
            return true;
        }

        public bool SetProgrammingVoltage(Pin PinNumber, int Voltage)
        {
            Status = (J2534Err)Library.API.SetProgrammingVoltage(DeviceID, (int)PinNumber, Voltage);
            if (Status == J2534Err.STATUS_NOERROR)
                return false;
            return true;
        }

        private bool GetVersion()
        {
            IntPtr pFirmwareVersion = Marshal.AllocHGlobal(120);
            IntPtr pDllVersion = Marshal.AllocHGlobal(120);
            IntPtr pApiVersion = Marshal.AllocHGlobal(120);
            Status = (J2534Err)Library.API.ReadVersion(DeviceID, pFirmwareVersion, pDllVersion, pApiVersion);
            if (Status == J2534Err.STATUS_NOERROR)
            {
                FirmwareVersion = Marshal.PtrToStringAnsi(pFirmwareVersion);
                LibraryVersion = Marshal.PtrToStringAnsi(pDllVersion);
                APIVersion = Marshal.PtrToStringAnsi(pApiVersion);
            }

            Marshal.FreeHGlobal(pFirmwareVersion);
            Marshal.FreeHGlobal(pDllVersion);
            Marshal.FreeHGlobal(pApiVersion);

            if (Status == J2534Err.STATUS_NOERROR)
                return false;
            return true;
        }

        public string GetLastError()
        {
            string return_string = null;
            IntPtr pErrorDescription = Marshal.AllocHGlobal(120);
            Status = (J2534Err)Library.API.GetLastError(pErrorDescription);
            if (Status == J2534Err.STATUS_NOERROR)
                return_string = Marshal.PtrToStringAnsi(pErrorDescription);

            Marshal.FreeHGlobal(pErrorDescription);

            return return_string;
        }

        public int MeasureBatteryVoltage()
        {
            int voltage = 0;
            IntPtr input = IntPtr.Zero;
            IntPtr output = Marshal.AllocHGlobal(8);

            Status = (J2534Err)Library.API.Ioctl(DeviceID, (int)Ioctl.READ_VBATT, input, output);
            if (Status == J2534Err.STATUS_NOERROR)
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

            Status = (J2534Err)Library.API.Ioctl(DeviceID, (int)Ioctl.READ_PROG_VOLTAGE, input, output);
            if (Status == J2534Err.STATUS_NOERROR)
            {
                voltage = Marshal.ReadInt32(output);
            }

            Marshal.FreeHGlobal(output);

            return voltage;
        }

        public Channel ConstructChannel()
        {
            return new Channel(this);
        }


    }

    public class Channel
    {
        public J2534Err Status;
        public List<PassThruMsg> MsgList;
        public List<SConfig> SConfigList;
        public List<PeriodicMsg_Type> PeriodicMsgList;
        public List<MsgFilterType> FilterList;
        public bool IsConnected;
        private J2534PhysicalDevice Device;
        public int ChannelID;
        public Protocols ProtocolID;
        public int Baud;
        public ConnectFlag ConnectFlags;
        public int TxTimeout;
        public int DefaultRxTimeout;
        public TxFlag DefaultTxFlag;
        public byte FiveBaudTargetAddress;
        public byte FiveBaudKeyword1;
        public byte FiveBaudKeyword2;
        public byte[] Header;   //Header to append when using Read/WriteMessagesNoHeader
        public Channel(J2534PhysicalDevice device)
        {
            Device = device;
            Status = Device.Status;
            MsgList = new List<J2534DotNet.PassThruMsg>();
            SConfigList = new List<J2534DotNet.SConfig>();
            PeriodicMsgList = new List<PeriodicMsg_Type>();
            FilterList = new List<J2534DotNet.MsgFilterType>();
            FilterList.Add(new MsgFilterType());
            ConnectFlags = ConnectFlag.NONE;
            TxTimeout = 50;
            DefaultRxTimeout = 250;
            DefaultTxFlag = TxFlag.NONE;
            IsConnected = false;
        }

        public bool Connect()
        {
            Status = (J2534Err)Device.Library.API.Connect(Device.DeviceID, (int)ProtocolID, (int)ConnectFlags, Baud, ref ChannelID);
            if (Status == J2534Err.STATUS_NOERROR)
            {
                IsConnected = true;
                return false;
            }
            return true;
        }

        public bool Disconnect()
        {
            Status = (J2534Err)Device.Library.API.Disconnect(ChannelID);
            if (Status == J2534Err.STATUS_NOERROR)
            {
                IsConnected = false;
                return false;
            }
            return true;
        }

        public List<byte> GetMessage()
        {
            if(!GetMessages(1, DefaultRxTimeout))
            {
                if(MsgList.Any())
                    return MsgList[0].Data;
            }
            return null;
        }

        public bool GetMessages(int NumMsgs)
        {
            return GetMessages(NumMsgs, DefaultRxTimeout);
        }

        public bool GetMessages(int NumMsgs, int Timeout)
        {
            int pNumMsgs = NumMsgs;
            IntPtr pMsgHeap = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(UnsafePassThruMsg)) * pNumMsgs);

            Status = (J2534Err)Device.Library.API.ReadMsgs(ChannelID, pMsgHeap, ref pNumMsgs, DefaultRxTimeout);

            if (Status == J2534Err.STATUS_NOERROR)
            {
                MsgList.Clear();
                for (int i = 0; i < pNumMsgs; i++)
                {
                    IntPtr pNextMsg = (IntPtr)(Marshal.SizeOf(typeof(UnsafePassThruMsg)) * i + (int)pMsgHeap);
                    UnsafePassThruMsg uMsg = (UnsafePassThruMsg)Marshal.PtrToStructure(pNextMsg, typeof(UnsafePassThruMsg));
                    MsgList.Add(ConvertPassThruMsg(uMsg));
                }
            }

            Marshal.FreeHGlobal(pMsgHeap);

            if(Status == J2534Err.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool SendMessage(List<byte> Message)
        {
            MsgList.Clear();
            MsgList.Add(new J2534DotNet.PassThruMsg(ProtocolID, DefaultTxFlag, Message));

            return SendMessages();
        }

        public bool SendMessages()
        {
            int pNumMsgs = MsgList.Count;

            IntPtr pMsg_heap = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(UnsafePassThruMsg)) * pNumMsgs);

            for (int i = 0; i < pNumMsgs; i++)
            {
                IntPtr pNextMsg = (IntPtr)(Marshal.SizeOf(typeof(UnsafePassThruMsg)) * i + (int)pMsg_heap);
                UnsafePassThruMsg u_msg = ConvertPassThruMsg(MsgList[i]);
                Marshal.StructureToPtr(u_msg, pNextMsg, false);
            }

            Status = (J2534Err)Device.Library.API.WriteMsgs(ChannelID, pMsg_heap, ref pNumMsgs, TxTimeout);

            Marshal.FreeHGlobal(pMsg_heap);


            if (Status == J2534Err.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool StartPeriodicMsg(int Index)
        {
            int pMsgID = PeriodicMsgList[Index].pMsgID;
            UnsafePassThruMsg uMsg = ConvertPassThruMsg(PeriodicMsgList[Index].Msg);
            Status = (J2534Err)Device.Library.API.StartPeriodicMsg(ChannelID, ref uMsg, ref pMsgID, PeriodicMsgList[Index].Interval);
            if(Status == J2534Err.STATUS_NOERROR)
            {
                PeriodicMsgList[Index].pMsgID = pMsgID;
                return false;
            }
            return true;
        }

        public bool StopPeriodicMsg(int Index)
        {
            int pMsgID = PeriodicMsgList[Index].pMsgID;
            Status = (J2534Err)Device.Library.API.StopPeriodicMsg(ChannelID, pMsgID);
            if (Status == J2534Err.STATUS_NOERROR)
            {
                return false;
            }
            return true;
        }

        public bool StartMsgFilter(int Index)
        {
            UnsafePassThruMsg uMaskMsg = ConvertPassThruMsg(new PassThruMsg(ProtocolID, TxFlag.NONE, FilterList[Index].Mask));
            UnsafePassThruMsg uPatternMsg = ConvertPassThruMsg(new PassThruMsg(ProtocolID, TxFlag.NONE, FilterList[Index].Pattern));
            UnsafePassThruMsg uFlowControlMsg = ConvertPassThruMsg(new PassThruMsg(ProtocolID, FilterList[Index].FlowControlTxFlags, FilterList[Index].FlowControl));
            int FID = FilterList[Index].FilterId;

            Status = (J2534Err)Device.Library.API.StartMsgFilter(ChannelID,
                                                  (int)FilterList[Index].FilterType,
                                                  ref uMaskMsg,
                                                  ref uPatternMsg,
                                                  ref uFlowControlMsg,
                                                  ref FID);

            FilterList[Index].FilterId = FID;
            if (Status == J2534Err.STATUS_NOERROR)
            {
                return false;
            }
            return true;
        }

        public bool StopMsgFilter(int Index)
        {
            Status = (J2534Err)Device.Library.API.StopMsgFilter(ChannelID, FilterList[Index].FilterId);
            if (Status == J2534Err.STATUS_NOERROR)
            {
                return false;
            }
            return true;
        }

        private UnsafePassThruMsg ConvertPassThruMsg(PassThruMsg Msg)
        {
            UnsafePassThruMsg uMsg = new UnsafePassThruMsg();

            uMsg.ProtocolID = (uint)Msg.ProtocolID;
            uMsg.RxStatus = (uint)Msg.RxStatus;
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

        private PassThruMsg ConvertPassThruMsg(UnsafePassThruMsg uMsg)
        {
            PassThruMsg Msg = new PassThruMsg();

            Msg.ProtocolID = (Protocols)uMsg.ProtocolID;
            Msg.RxStatus = (RxStatus)uMsg.RxStatus;
            Msg.Timestamp = uMsg.Timestamp;
            Msg.TxFlags = (TxFlag)uMsg.TxFlags;
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

        public bool GetConfig()
        {
            int num_of_params = SConfigList.Count();

            IntPtr input = Marshal.AllocHGlobal(8 * (num_of_params + 1));

            Marshal.WriteInt32(input, 0, num_of_params);
            Marshal.WriteInt32(input, 4, (int)input + 8);
            Marshal.StructureToPtr(SConfigList, (IntPtr)((int)input + 8), false);

            Status = (J2534Err)Device.Library.API.Ioctl(ChannelID, (int)Ioctl.GET_CONFIG, input, IntPtr.Zero);

            if (Status == J2534Err.STATUS_NOERROR)
            {
                num_of_params = Marshal.ReadInt32(input);
                SConfigList = new List<SConfig>(num_of_params);
                IntPtr ListDataPtr = Marshal.ReadIntPtr((IntPtr)((int)input + 4));
                Marshal.PtrToStructure(ListDataPtr, SConfigList);
            }

            Marshal.FreeHGlobal(input);

            if (Status == J2534Err.STATUS_NOERROR)
            {
                return false;
            }
            return true;
        }

        public bool SetConfig()
        {
            int num_of_params = SConfigList.Count();

            IntPtr input = Marshal.AllocHGlobal(8 * (num_of_params + 1));
            IntPtr output = IntPtr.Zero;

            Marshal.WriteInt32(input, 0, num_of_params);
            Marshal.WriteInt32(input, 4, (int)input + 8);
            Marshal.StructureToPtr(SConfigList, (IntPtr)((int)input + 8), false);

            Status = (J2534Err)Device.Library.API.Ioctl(ChannelID, (int)Ioctl.SET_CONFIG, input, output);

            Marshal.FreeHGlobal(input);

            if (Status == J2534Err.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool ClearTxBuffer()
        {
            IntPtr input = IntPtr.Zero;
            IntPtr output = IntPtr.Zero;

            Status = (J2534Err)Device.Library.API.Ioctl(ChannelID, (int)Ioctl.CLEAR_TX_BUFFER, input, output);
            if (Status == J2534Err.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool ClearRxBuffer()
        {
            IntPtr input = IntPtr.Zero;
            IntPtr output = IntPtr.Zero;

            Status = (J2534Err)Device.Library.API.Ioctl(ChannelID, (int)Ioctl.CLEAR_RX_BUFFER, input, output);
            if (Status == J2534Err.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool ClearPeriodicMsgs()
        {
            IntPtr input = IntPtr.Zero;
            IntPtr output = IntPtr.Zero;

            Status = (J2534Err)Device.Library.API.Ioctl(ChannelID, (int)Ioctl.CLEAR_PERIODIC_MSGS, input, output);
            if (Status == J2534Err.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool ClearMsgFilters()
        {
            IntPtr input = IntPtr.Zero;
            IntPtr output = IntPtr.Zero;

            Status = (J2534Err)Device.Library.API.Ioctl(ChannelID, (int)Ioctl.CLEAR_MSG_FILTERS, input, output);
            if (Status == J2534Err.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool ClearFunctMsgLookupTable()
        {
            IntPtr input = IntPtr.Zero;
            IntPtr output = IntPtr.Zero;

            Status = (J2534Err)Device.Library.API.Ioctl(ChannelID, (int)Ioctl.CLEAR_FUNCT_MSG_LOOKUP_TABLE, input, output);

            if (Status == J2534Err.STATUS_NOERROR)
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

            Status = (J2534Err)Device.Library.API.Ioctl(ChannelID, (int)Ioctl.ADD_TO_FUNCT_MSG_LOOKUP_TABLE, input, output);

            Marshal.FreeHGlobal(input);

            if (Status == J2534Err.STATUS_NOERROR)
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

            Status = (J2534Err)Device.Library.API.Ioctl(ChannelID, (int)Ioctl.DELETE_FROM_FUNCT_MSG_LOOKUP_TABLE, input, output);

            Marshal.FreeHGlobal(input);

            if (Status == J2534Err.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool FiveBaudInit()
        {
            IntPtr input = Marshal.AllocHGlobal(9);
            IntPtr output = Marshal.AllocHGlobal(10);

            Marshal.WriteInt32(input, 0, 1);
            Marshal.WriteInt32(input, 4, (int)input + 8);
            Marshal.WriteInt32(input, 8, FiveBaudTargetAddress);

            Marshal.WriteInt32(output, 0, 2);
            Marshal.WriteInt32(output, 4, (int)output + 8);
            Marshal.WriteInt32(output, 8, FiveBaudKeyword1);
            Marshal.WriteInt32(output, 9, FiveBaudKeyword2);

            Status = (J2534Err)Device.Library.API.Ioctl(ChannelID, (int)Ioctl.FIVE_BAUD_INIT, input, output);
            if(Status == J2534Err.STATUS_NOERROR)
            {
                IntPtr data0 = Marshal.ReadIntPtr((IntPtr)((int)output + 4));
                FiveBaudKeyword1 = Marshal.ReadByte(data0);
                FiveBaudKeyword2 = Marshal.ReadByte((IntPtr)((int)data0 + 1));
            }
            Marshal.FreeHGlobal(input);
            Marshal.FreeHGlobal(output);

            if (Status == J2534Err.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool FastInit()
        {
            IntPtr input = IntPtr.Zero;
            IntPtr output = IntPtr.Zero;
            UnsafePassThruMsg uTxMsg = ConvertPassThruMsg(MsgList[0]);
            UnsafePassThruMsg uRxMsg = new UnsafePassThruMsg();

            Marshal.StructureToPtr(uTxMsg, input, true);
            Marshal.StructureToPtr(uRxMsg, output, true);

            J2534Err returnValue = (J2534Err)Device.Library.API.Ioctl(ChannelID, (int)Ioctl.FAST_INIT, input, output);
            if (returnValue == J2534Err.STATUS_NOERROR)
            {
                Marshal.PtrToStructure(output, uRxMsg);
            }
            Marshal.FreeHGlobal(input);
            Marshal.FreeHGlobal(output);

            MsgList[0] = ConvertPassThruMsg(uRxMsg);
            if (Status == J2534Err.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool SetProgrammingVoltage(Pin PinNumber, int Voltage)
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
