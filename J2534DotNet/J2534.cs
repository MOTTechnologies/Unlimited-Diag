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
        internal string NextDeviceName;
        internal string NextDeviceAddr;
        internal int NextDeviceVer;
        internal int Status;
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
            return new J2534DotNet.J2534PhysicalDevice(this);
        }

        internal J2534PhysicalDevice ConstructDevice(string DeviceName)
        {
            J2534PhysicalDevice dev = new J2534DotNet.J2534PhysicalDevice(this, DeviceName);
            if (dev.IsConnected)
                return dev;
            return null;
        }

        internal void GetNextDevice()
        {
            //API.GetNextDevice(pSDevice);
        }

        internal bool GetNextCarDAQ()
        {
            IntPtr pName = Marshal.AllocHGlobal(80);
            IntPtr pAddr = Marshal.AllocHGlobal(80);
            int Ver = 0;

            Status = API.GetNextCarDAQ(pName, ref Ver, pAddr);

            if (Status == J2534APIWrapper.FUNCTION_NOT_ASSIGNED || pName == IntPtr.Zero)
            {
                Marshal.FreeHGlobal(pName);
                Marshal.FreeHGlobal(pAddr);
                return false;
            }

            NextDeviceName = Marshal.PtrToStringAnsi(pName);
            NextDeviceAddr = Marshal.PtrToStringAnsi(pAddr);
            NextDeviceVer = Ver;

            Marshal.FreeHGlobal(pName);
            Marshal.FreeHGlobal(pAddr);
            
            return true;
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
        //public object Status;
        internal int DeviceID;
        public bool IsConnected;
        internal J2534Lib Library;
        public string FirmwareVersion;
        public string LibraryVersion;
        public string APIVersion;

        internal J2534PhysicalDevice(J2534Lib Library)
        {
            //Status = _Status;
            this.Library = Library;
            ConnectToDevice("");
        }

        //Devicenames that work are "CarDAQ-Plus1331" and "192.168.43.101"
        internal J2534PhysicalDevice(J2534Lib Library, string DeviceName)
        {
            this.Library = Library;
            ConnectToDevice(DeviceName);
        }
        public bool ConnectToDevice(string Device)
        {
            IntPtr DeviceNamePtr = IntPtr.Zero;
            if (Device.Length > 0)
                DeviceNamePtr = Marshal.StringToHGlobalAnsi(Device);
            Status = (J2534ERR)Library.API.Open(DeviceNamePtr, ref DeviceID);

            Marshal.FreeHGlobal(DeviceNamePtr);

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
            IntPtr pFirmwareVersion = Marshal.AllocHGlobal(80);
            IntPtr pDllVersion = Marshal.AllocHGlobal(80);
            IntPtr pApiVersion = Marshal.AllocHGlobal(80);
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
            IntPtr pErrorDescription = Marshal.AllocHGlobal(80);
            Status = (J2534ERR)Library.API.GetLastError(pErrorDescription);
            if (Status == J2534ERR.STATUS_NOERROR)
                return_string = Marshal.PtrToStringAnsi(pErrorDescription);

            Marshal.FreeHGlobal(pErrorDescription);

            return return_string;
        }

        public int MeasureBatteryVoltage()
        {
            int voltage = 0;
            IntPtr output = Marshal.AllocHGlobal(4);

            Status = (J2534ERR)Library.API.IOCtl(DeviceID, (int)J2534IOCTL.READ_VBATT, IntPtr.Zero, output);
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
            IntPtr output = Marshal.AllocHGlobal(4);

            Status = (J2534ERR)Library.API.IOCtl(DeviceID, (int)J2534IOCTL.READ_PROG_VOLTAGE, IntPtr.Zero, output);
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

            IntPtr pJ2534MessagesHeap = Marshal.AllocHGlobal(Marshal.SizeOf<J2534Message>() * pNumMsgs);

            Status = (J2534ERR)Device.Library.API.ReadMsgs(ChannelID, pJ2534MessagesHeap, ref pNumMsgs, Timeout);

            if (Status == J2534ERR.STATUS_NOERROR)
            {
                RxMessages.Clear();
                for (int i = 0; i < pNumMsgs; i++)
                    RxMessages.Add(Marshal.PtrToStructure<J2534Message>((IntPtr)(i * Marshal.SizeOf<J2534Message>() + (int)pJ2534MessagesHeap)));
            }

            Marshal.FreeHGlobal(pJ2534MessagesHeap);

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

            IntPtr pJ2534MessagesHeap = Marshal.AllocHGlobal(Marshal.SizeOf<J2534Message>() * pNumMsgs);

            for (int i = 0; i < pNumMsgs; i++)
                Marshal.StructureToPtr<J2534Message>(Messages[i], (IntPtr)(i * Marshal.SizeOf<J2534Message>() + (int)pJ2534MessagesHeap), false);

            Status = (J2534ERR)Device.Library.API.WriteMsgs(ChannelID, pJ2534MessagesHeap, ref pNumMsgs, DefaultTxTimeout);

            Marshal.FreeHGlobal(pJ2534MessagesHeap);

            //If some messages didnt get sent, put them into the RxMessages list
            //Not sure what else to do in this case.
            if(pNumMsgs < Messages.Count)
            {
                Messages.RemoveRange(0, pNumMsgs);
                RxMessages = Messages;
            }

            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool StartPeriodicMessage(J2534Message Message, int Interval)
        {
            PeriodicMsgList.Add(new PeriodicMsg(Message, Interval));

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
            IntPtr pJ2534MessagesHeap = Marshal.AllocHGlobal(Marshal.SizeOf<J2534Message>());
            Marshal.StructureToPtr<J2534Message>(PeriodicMsgList[Index].Message, pJ2534MessagesHeap, false);

            Status = (J2534ERR)Device.Library.API.StartPeriodicMsg(ChannelID, pJ2534MessagesHeap, ref PeriodicMsgList[Index].MessageID, PeriodicMsgList[Index].Interval);

            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        /// <summary>
        /// Stops the periodic message in 'PeriodicMsgList' referenced by 'Index'.
        /// </summary>
        /// <param name="Index"></param>
        /// <returns>Returns 'false' if successful</returns>
        public bool StopPeriodicMsg(int Index)
        {
            int MsgID = PeriodicMsgList[Index].MessageID;
            Status = (J2534ERR)Device.Library.API.StopPeriodicMsg(ChannelID, MsgID);
            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
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
            IntPtr pMask = Marshal.AllocHGlobal(Marshal.SizeOf<J2534Message>());
            IntPtr pPattern = Marshal.AllocHGlobal(Marshal.SizeOf<J2534Message>());
            IntPtr pFlowControl = Marshal.AllocHGlobal(Marshal.SizeOf<J2534Message>());

            Marshal.StructureToPtr<J2534Message>(new J2534Message(ProtocolID, FilterList[Index].TxFlags, FilterList[Index].Mask), pMask, false);
            Marshal.StructureToPtr<J2534Message>(new J2534Message(ProtocolID, FilterList[Index].TxFlags, FilterList[Index].Pattern), pPattern, false);
            Marshal.StructureToPtr<J2534Message>(new J2534Message(ProtocolID, FilterList[Index].TxFlags, FilterList[Index].FlowControl), pFlowControl, false);

            if (FilterList[Index].FilterType != J2534FILTER.FLOW_CONTROL_FILTER)
            {
                Marshal.FreeHGlobal(pFlowControl);
                pFlowControl = IntPtr.Zero;
            }

            Status = (J2534ERR)Device.Library.API.StartMsgFilter(ChannelID, (int)FilterList[Index].FilterType, pMask, pPattern, pFlowControl, ref FilterList[Index].FilterId);

            Marshal.FreeHGlobal(pMask);
            Marshal.FreeHGlobal(pPattern);
            Marshal.FreeHGlobal(pFlowControl);

            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool StopMsgFilter(int Index)
        {
            Status = (J2534ERR)Device.Library.API.StopMsgFilter(ChannelID, FilterList[Index].FilterId);
            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool GetConfig(J2534PARAMETER Parameter, ref int Value)
        {
            List<SConfig> SConfig = GetConfig(new List<SConfig>() { new J2534DotNet.SConfig(Parameter, 0) });
            if (SConfig.Count > 0)
                Value = SConfig[0].Value;

            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }
        public List<SConfig> GetConfig(List<SConfig> SConfig)
        {
            IntPtr pSConfig = Marshal.AllocHGlobal(Marshal.SizeOf<SConfig>() * SConfig.Count);
            IntPtr pSConfigList = Marshal.AllocHGlobal(Marshal.SizeOf<SConfigList>());

            Marshal.StructureToPtr<SConfigList>(new SConfigList(SConfig.Count(), pSConfig), pSConfigList, false);
            for (int i = 0; i < SConfig.Count; i++)
                Marshal.StructureToPtr<SConfig>(SConfig[i], (IntPtr)(Marshal.SizeOf<SConfig>() * i + (int)pSConfig), false);

            Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.SET_CONFIG, pSConfigList, IntPtr.Zero);

            SConfigList SConfigList = Marshal.PtrToStructure<SConfigList>(pSConfigList);
            List<SConfig> ReturnList = new List<SConfig>();
            for(int i = 0;i < SConfigList.NumOfParams; i++)
                ReturnList.Add(Marshal.PtrToStructure<SConfig>((IntPtr)(i * Marshal.SizeOf<SConfig>() + (int)SConfigList.Pointer)));

            Marshal.FreeHGlobal(pSConfig);
            Marshal.FreeHGlobal(pSConfigList);

            return ReturnList;
        }

        public bool SetConfig(J2534PARAMETER Parameter, int Value)
        {
            return SetConfig(new List<SConfig>() { new SConfig(Parameter, Value) });
        }

        public bool SetConfig(List<SConfig> SConfig)
        {
            IntPtr pSConfig = Marshal.AllocHGlobal(Marshal.SizeOf<SConfig>() * SConfig.Count);
            IntPtr pSConfigList = Marshal.AllocHGlobal(Marshal.SizeOf<SConfigList>());

            Marshal.StructureToPtr<SConfigList>(new SConfigList(SConfig.Count(), pSConfig), pSConfigList, false);
            for(int i = 0;i < SConfig.Count; i++)
                Marshal.StructureToPtr<SConfig>(SConfig[i], (IntPtr)(Marshal.SizeOf<SConfig>() * i + (int)pSConfig), false);

            Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.SET_CONFIG, pSConfigList, IntPtr.Zero);

            Marshal.FreeHGlobal(pSConfig);
            Marshal.FreeHGlobal(pSConfigList);

            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool ClearTxBuffer()
        {
            Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.CLEAR_TX_BUFFER, IntPtr.Zero, IntPtr.Zero);
            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool ClearRxBuffer()
        {
            Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.CLEAR_RX_BUFFER, IntPtr.Zero, IntPtr.Zero);
            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool ClearPeriodicMsgs()
        {
            Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.CLEAR_PERIODIC_MSGS, IntPtr.Zero, IntPtr.Zero);
            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool ClearMsgFilters()
        {
            Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.CLEAR_MSG_FILTERS, IntPtr.Zero, IntPtr.Zero);
            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool ClearFunctMsgLookupTable()
        {
            Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.CLEAR_FUNCT_MSG_LOOKUP_TABLE, IntPtr.Zero, IntPtr.Zero);
            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool AddToFunctMsgLookupTable(byte Addr)
        {
            return AddToFunctMsgLookupTable(new List<byte>() { Addr });
        }

        public bool AddToFunctMsgLookupTable(List<byte> FuncAddr)
        {
            IntPtr input = Marshal.AllocHGlobal(Marshal.SizeOf<SByteArray>());
            IntPtr pFuncAddr = Marshal.AllocHGlobal(Marshal.SizeOf<byte>() * FuncAddr.Count);

            Marshal.StructureToPtr<SByteArray>(new SByteArray(FuncAddr.Count, pFuncAddr), input, false);
            for (int i = 0; i < FuncAddr.Count; i++)
                Marshal.StructureToPtr<byte>(FuncAddr[i], (IntPtr)(Marshal.SizeOf<byte>() * i + (int)pFuncAddr), false);

            Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.ADD_TO_FUNCT_MSG_LOOKUP_TABLE, input, IntPtr.Zero);

            Marshal.FreeHGlobal(input);
            Marshal.FreeHGlobal(pFuncAddr);

            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool DeleteFromFunctMsgLookupTable(byte Addr)
        {
            return DeleteFromFunctMsgLookupTable(new List<byte>() { Addr });
        }

        public bool DeleteFromFunctMsgLookupTable(List<byte> FuncAddr)
        {
            IntPtr input = Marshal.AllocHGlobal(Marshal.SizeOf<SByteArray>());
            IntPtr pFuncAddr = Marshal.AllocHGlobal(Marshal.SizeOf<byte>() * FuncAddr.Count);

            Marshal.StructureToPtr<SByteArray>(new SByteArray(FuncAddr.Count, pFuncAddr), input, false);
            Marshal.StructureToPtr<byte[]>(FuncAddr.ToArray(), pFuncAddr, false);

            Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.DELETE_FROM_FUNCT_MSG_LOOKUP_TABLE, input, IntPtr.Zero);

            Marshal.FreeHGlobal(input);
            Marshal.FreeHGlobal(pFuncAddr);

            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool FiveBaudInit(byte TargetAddress)
        {
            IntPtr input = Marshal.AllocHGlobal(Marshal.SizeOf<SByteArray>());
            IntPtr output = Marshal.AllocHGlobal(Marshal.SizeOf<SByteArray>());
            IntPtr pAddress = Marshal.AllocHGlobal(Marshal.SizeOf<byte>());
            IntPtr pKeywords = Marshal.AllocHGlobal(Marshal.SizeOf<byte>() * 2);

            Marshal.StructureToPtr<SByteArray>(new SByteArray(1, pAddress), input, false);
            Marshal.StructureToPtr<SByteArray>(new SByteArray(2, pKeywords), output, false);
            Marshal.WriteByte(pAddress, TargetAddress);

            Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.FIVE_BAUD_INIT, input, output);

            if (Status == J2534ERR.STATUS_NOERROR)
            {
                SByteArray SByteArray = Marshal.PtrToStructure<SByteArray>(output);    
                if(SByteArray.NumOfBytes == 2)
                {
                    FiveBaudKeyword1 = Marshal.ReadByte(SByteArray.Pointer, 0);
                    FiveBaudKeyword2 = Marshal.ReadByte(SByteArray.Pointer, 1);
                }
            }

            Marshal.FreeHGlobal(input);
            Marshal.FreeHGlobal(output);
            Marshal.FreeHGlobal(pAddress);
            Marshal.FreeHGlobal(pKeywords);

            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool FastInit(J2534Message TxMessage)
        {

            IntPtr input = Marshal.AllocHGlobal(Marshal.SizeOf<J2534Message>());
            IntPtr output = Marshal.AllocHGlobal(Marshal.SizeOf<J2534Message>());

            Marshal.StructureToPtr<J2534Message>(TxMessage, input, false);

            Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.FAST_INIT, input, output);

            if (Status == J2534ERR.STATUS_NOERROR)
            {                
                RxMessages.Clear();
                RxMessages.Add(Marshal.PtrToStructure<J2534Message>(output));
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
