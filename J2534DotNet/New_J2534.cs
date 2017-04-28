using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace J2534DotNet
{
    class J2534Lib
    {
        public string FileName;
        public bool IsLoaded;
        public J2534APIWrapper API;

        public J2534Lib(string LibFile)
        {
            API = new J2534DotNet.J2534APIWrapper();
            Load();
        }

        public bool Load()
        {
            IsLoaded = API.LoadJ2534Library(FileName);
            return IsLoaded;
        }

        public bool Free()
        {
            IsLoaded = API.FreeLibrary();   //Does this return a true or false????
            return IsLoaded;
        }

        public J2534PhysicalDevice ConstructDevice()
        {
            J2534PhysicalDevice dev = new J2534DotNet.J2534PhysicalDevice(this);
            if (dev.IsConnected)
                return dev;
            return null;
        }
    }

    class J2534PhysicalDevice
    {
        public J2534Err Status;
        public int DeviceID;
        public bool IsConnected;
        public J2534Lib Library;
        public string FirmwareVersion;
        public string LibraryVersion;
        public string APIVersion;

        public J2534PhysicalDevice(J2534Lib library)
        {
            Library = library;
            ConnectToDevice();
        }

        public bool ConnectToDevice()
        {
            int nada = 0;
            Status = (J2534Err)Library.API.Open(nada, ref DeviceID);
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

    class Channel
    {
        public J2534Err Status;
        public List<PassThruMsg> MsgList;
        public List<SConfig> SConfigList;
        public List<PeriodicMsg_Type> PeriodicMsgList;
        public List<MsgFilterType> FilterList;
        public bool IsConnected;
        public J2534PhysicalDevice Device;
        public int ChannelID;
        public Protocols ProtocolID;
        public int Baud;
        public ConnectFlag ConnectFlags;
        public int TxTimeout;
        public int RxTimeout;

        public Channel(J2534PhysicalDevice device)
        {
            Device = device;
            Status = Device.Status;
            MsgList = new List<J2534DotNet.PassThruMsg>();
            SConfigList = new List<J2534DotNet.SConfig>();
            PeriodicMsgList = new List<PeriodicMsg_Type>();
            FilterList = new List<J2534DotNet.MsgFilterType>();
            ConnectFlags = ConnectFlag.NONE;
            TxTimeout = 50;
            RxTimeout = 250;
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

        public bool ReadMsg()
        {
            return ReadMsgs(1, RxTimeout);
        }

        public bool ReadMsg(int NumMsgs)
        {
            return ReadMsgs(NumMsgs, RxTimeout);
        }

        public bool ReadMsgs(int NumMsgs, int Timeout)
        {
            int pNumMsgs = NumMsgs;
            IntPtr pMsgHeap = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(UnsafePassThruMsg)) * pNumMsgs);

            Status = (J2534Err)Device.Library.API.ReadMsgs(ChannelID, pMsgHeap, ref pNumMsgs, RxTimeout);

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

        public bool WriteMsg(byte [] Msg)
        {
            MsgList[0].Data = Msg;
            PassThruMsg tmp = MsgList[0];
            MsgList.Clear();
            MsgList.Add(tmp);

            if (Status == J2534Err.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool WriteMsgs()
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
            UnsafePassThruMsg uMaskMsg = ConvertPassThruMsg(FilterList[Index].MaskMsg);
            UnsafePassThruMsg uPatternMsg = ConvertPassThruMsg(FilterList[Index].PatternMsg);
            UnsafePassThruMsg uFlowControlMsg = ConvertPassThruMsg(FilterList[Index].FlowControlMsg);
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

            uMsg.ProtocolID = (int)Msg.ProtocolID;
            uMsg.RxStatus = (int)Msg.RxStatus;
            uMsg.Timestamp = Msg.Timestamp;
            uMsg.TxFlags = (int)Msg.TxFlags;
            uMsg.ExtraDataIndex = Msg.ExtraDataIndex;
            uMsg.DataSize = Msg.Data.Length;
            unsafe
            {
                for (int i = 0; i < Msg.Data.Length; i++)
                {
                    uMsg.Data[i] = Msg.Data[i];
                }
            }

            return uMsg;
        }

        private static PassThruMsg ConvertPassThruMsg(UnsafePassThruMsg uMsg)
        {
            PassThruMsg Msg = new PassThruMsg();

            Msg.ProtocolID = (Protocols)uMsg.ProtocolID;
            Msg.RxStatus = (RxStatus)uMsg.RxStatus;
            Msg.Timestamp = uMsg.Timestamp;
            Msg.TxFlags = (TxFlag)uMsg.TxFlags;
            Msg.ExtraDataIndex = uMsg.ExtraDataIndex;
            Msg.Data = new byte[uMsg.DataSize];
            unsafe
            {
                for (int i = 0; i < uMsg.DataSize; i++)
                {
                    Msg.Data[i] = uMsg.Data[i];
                }
            }

            return Msg;
        }

        public bool GetConfig()
        {
            int num_of_params = SConfigList.Count();

            IntPtr input = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SConfig)) * (num_of_params + 1));
            IntPtr list_data = (IntPtr)((int)input + 8);
            IntPtr list_data_ptr = (IntPtr)((int)input + 4);

            Marshal.StructureToPtr(num_of_params, input, false);
            Marshal.StructureToPtr((int)list_data, list_data_ptr, false);
            Marshal.StructureToPtr(SConfigList, list_data, false);

            IntPtr output = IntPtr.Zero;

            Status = (J2534Err)Device.Library.API.Ioctl(ChannelID, (int)Ioctl.GET_CONFIG, input, output);
            if(Status == J2534Err.STATUS_NOERROR)
            {
                Marshal.PtrToStructure(input, num_of_params);
                SConfigList = new List<SConfig>(num_of_params);
                list_data_ptr = (IntPtr)((int)input + 4);
                Marshal.PtrToStructure(list_data_ptr,list_data);
                Marshal.PtrToStructure(list_data, SConfigList);
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

            IntPtr input = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SConfig)) * (num_of_params + 1));
            IntPtr list_data = (IntPtr)((int)input + 8);
            IntPtr list_data_ptr = (IntPtr)((int)input + 4);

            Marshal.StructureToPtr(num_of_params, input, false);
            Marshal.StructureToPtr((int)list_data, list_data_ptr, false);
            Marshal.StructureToPtr(SConfigList, list_data, false);

            IntPtr output = IntPtr.Zero;

            Status = (J2534Err)Device.Library.API.Ioctl(ChannelID, (int)Ioctl.SET_CONFIG, input, output);
            if (Status == J2534Err.STATUS_NOERROR)
            {
                Marshal.PtrToStructure(input, num_of_params);
                SConfigList = new List<SConfig>(num_of_params);
                list_data_ptr = (IntPtr)((int)input + 4);
                Marshal.PtrToStructure(list_data_ptr, list_data);
                Marshal.PtrToStructure(list_data, SConfigList);
            }

            Marshal.FreeHGlobal(input);

            if (Status == J2534Err.STATUS_NOERROR)
            {
                return false;
            }
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
