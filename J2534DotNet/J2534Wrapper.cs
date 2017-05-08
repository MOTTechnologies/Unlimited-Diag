using System;
using System.Runtime.InteropServices;

namespace J2534DotNet
{
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string Library);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr pLibrary, string FunctionName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr pLibrary);
    }

    internal class J2534APIWrapper
    {
        public const int FUNCTION_NOT_ASSIGNED = 0x7EADBEEF;

        [Flags]
        private enum API_SIGNATURE
        {
            //2.02 calls
            NONE = 0x00000000,
            CONNECT = 0x00000001,
            DISCONNECT = 0x00000002,
            READMSGS = 0x00000004,
            WRITEMSGS = 0x00000008,
            STARTPERIODICMSG = 0x00000010,
            STOPPERIODICMSG = 0x00000020,
            STARTMSGFILTER = 0x00000040,
            STOPMSGFILTER = 0x00000080,
            SETPROGRAMMINGVOLTAGE = 0x00000100,
            READVERSION = 0x00000200,
            GETLASTERROR = 0x00000400,
            IOCTL = 0x00000800,
            //4.04 calls
            OPEN = 0x00001000,
            CLOSE = 0x00002000,
            //5.00 calls
            SCANFORDEVICES = 0x00004000,
            GETNEXTDEVICE = 0x00008000,
            LOGICALCONNECT = 0x00010000,
            LOGICALDISCONNECT = 0x00020000,
            SELECT = 0x00040000,
            QUEUEMESSAGES = 0x00080000
        }
        const API_SIGNATURE V202_SIGNATURE = (API_SIGNATURE)0x0FFF;
        const API_SIGNATURE V404_SIGNATURE = (API_SIGNATURE)0x3FFF;
        const API_SIGNATURE V5_SIGNATURE = (API_SIGNATURE)0xFFFFF;

        private IntPtr pLibrary;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruConnect(int DeviceID, int ProtocolID, int ConnectFlags, int Baud, ref int ChannelID);
        internal PassThruConnect Connect = delegate (int DeviceID, int ProtocolID, int ConnectFlags, int Baud, ref int ChannelID) { return FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruDisconnect(int channelId);
        internal PassThruDisconnect Disconnect = delegate (int channelId) { return FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruReadMsgs(int ChannelID, IntPtr pUMsgArray, ref int NumMsgs, int Timeout);
        internal PassThruReadMsgs ReadMsgs = delegate (int ChannelID, IntPtr pMsgArray, ref int NumMsgs, int Timeout) { return FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruWriteMsgs(int ChannelID, IntPtr pUMsgArray, ref int NumMsgs, int Timeout);
        internal PassThruWriteMsgs WriteMsgs = delegate (int ChannelID, IntPtr pMsgArray, ref int NumMsgs, int Timeout) { return FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruStartPeriodicMsg(int ChannelID, IntPtr Msg, ref int MsgID, int Interval);
        internal PassThruStartPeriodicMsg StartPeriodicMsg = delegate (int ChannelID, IntPtr pMsg, ref int MsgID, int Interval) { return FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruStopPeriodicMsg(int ChannelID, int MsgID);
        internal PassThruStopPeriodicMsg StopPeriodicMsg = delegate (int ChannelID, int MsgID) { return FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruStartMsgFilter
        (
            int ChannelID,
            int FilterType,
            IntPtr pMaskMsg,
            IntPtr PatternMsg,
            IntPtr pFlowControlMsg,
            ref int FilterID
        );
        internal PassThruStartMsgFilter StartMsgFilter = delegate (
            int ChannelID,
            int FilterType,
            IntPtr pMaskMsg,
            IntPtr pPatternMsg,
            IntPtr pFlowControlMsg,
            ref int FilterID
        ) { return FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruStopMsgFilter(int ChannelID, int FilterID);
        internal PassThruStopMsgFilter StopMsgFilter = delegate (int ChannelID, int FilterID) { return FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruSetProgrammingVoltage(int DeviceID, int Pin, int Voltage);
        internal PassThruSetProgrammingVoltage SetProgrammingVoltage = delegate (int DeviceID, int Pin, int Voltage) { return FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruReadVersion(int DeviceID, IntPtr FirmwareVer, IntPtr DllVer, IntPtr APIVer);
        internal PassThruReadVersion ReadVersion = delegate (int DeviceID, IntPtr FirmwareVer, IntPtr DllVer, IntPtr APIVer) { return FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruGetLastError(IntPtr pErr);
        internal PassThruGetLastError GetLastError = delegate (IntPtr pErr) { return FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruIoctl(int ChannelID, int IOCtlID, IntPtr Input, IntPtr Output);
        internal PassThruIoctl IOCtl = delegate(int ChannelID, int IOCtlID, IntPtr Input, IntPtr Output) { return FUNCTION_NOT_ASSIGNED; } ;

        //**********v 4.04 calls****************
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruOpen(IntPtr pDeviceName, ref int DeviceID);
        internal PassThruOpen Open = delegate (IntPtr pDeviceName, ref int DeviceID) { return FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruClose(int DeviceID);
        internal PassThruClose Close = delegate (int DeviceID) { return FUNCTION_NOT_ASSIGNED; };


        //********************J2534 v5 and undocumented Drewtech calls*********************
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruScanForDevices(ref int DeviceCount);
        internal PassThruScanForDevices ScanForDevices = delegate (ref int DeviceCount) { return FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruGetNextDevice(IntPtr pSDevice);
        internal PassThruGetNextDevice GetNextDevice = delegate (IntPtr pSDevice) { return FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruLogicalConnect(int PhysicalChannelID, int ProtocolID, int ConnectFlags, IntPtr ChannelDescriptor, ref int pChannelID);
        internal PassThruLogicalConnect LogicalConnect = delegate (int PhysicalChannelID, int ProtocolID, int ConnectFlags, IntPtr ChannelDescriptor, ref int pChannelID) { return FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruLogicalDisconnect(int pChannelID);
        internal PassThruLogicalDisconnect LogicalDisconnect = delegate (int pChannelID) { return FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruSelect(IntPtr pSChannelSet, int SelectType, int Timeout);
        internal PassThruSelect Select = delegate (IntPtr pSChannelSet, int SelectType, int Timeout) { return FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruQueueMsgs(int ChannelID, IntPtr pMsgArray, ref int NumMsgs);
        internal PassThruQueueMsgs QueueMsgs = delegate (int ChannelID, IntPtr pMsgArray, ref int NumMsgs) { return FUNCTION_NOT_ASSIGNED; };
        
        //***************Drewtech only********************
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruGetNextCarDAQ(IntPtr pName, ref int Version, IntPtr pAddress);
        internal PassThruGetNextCarDAQ GetNextCarDAQ = delegate (IntPtr pName, ref int Version, IntPtr pAddress) { return FUNCTION_NOT_ASSIGNED; };

        internal bool LoadJ2534Library(string FileName)
        {
            API_SIGNATURE APIsignature = API_SIGNATURE.NONE;

            pLibrary = NativeMethods.LoadLibrary(FileName);

            if (pLibrary == IntPtr.Zero)
                return false;

            IntPtr pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruOpen");
            if (pFunction != IntPtr.Zero)
            {
                Open = (PassThruOpen)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruOpen));
                APIsignature |= API_SIGNATURE.OPEN;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruClose");
            if (pFunction != IntPtr.Zero)
            {
                Close = (PassThruClose)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruClose));
                APIsignature |= API_SIGNATURE.CLOSE;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruConnect");
            if (pFunction != IntPtr.Zero)
            {
                Connect = (PassThruConnect)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruConnect));
                APIsignature |= API_SIGNATURE.CONNECT;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruDisconnect");
            if (pFunction != IntPtr.Zero)
            {
                Disconnect = (PassThruDisconnect)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruDisconnect));
                APIsignature |= API_SIGNATURE.DISCONNECT;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruReadMsgs");
            if (pFunction != IntPtr.Zero)
            {
                ReadMsgs = (PassThruReadMsgs)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruReadMsgs));
                APIsignature |= API_SIGNATURE.READMSGS;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruWriteMsgs");
            if (pFunction != IntPtr.Zero)
            {
                WriteMsgs = (PassThruWriteMsgs)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruWriteMsgs));
                APIsignature |= API_SIGNATURE.WRITEMSGS;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruStartPeriodicMsg");
            if (pFunction != IntPtr.Zero)
            {
                StartPeriodicMsg = (PassThruStartPeriodicMsg)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruStartPeriodicMsg));
                APIsignature |= API_SIGNATURE.STARTPERIODICMSG;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruStopPeriodicMsg");
            if (pFunction != IntPtr.Zero)
            {
                StopPeriodicMsg = (PassThruStopPeriodicMsg)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruStopPeriodicMsg));
                APIsignature |= API_SIGNATURE.STOPPERIODICMSG;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruStartMsgFilter");
            if (pFunction != IntPtr.Zero)
            {
                StartMsgFilter = (PassThruStartMsgFilter)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruStartMsgFilter));
                APIsignature |= API_SIGNATURE.STARTMSGFILTER;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruStopMsgFilter");
            if (pFunction != IntPtr.Zero)
            {
                StopMsgFilter = (PassThruStopMsgFilter)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruStopMsgFilter));
                APIsignature |= API_SIGNATURE.STOPMSGFILTER;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruSetProgrammingVoltage");
            if (pFunction != IntPtr.Zero)
            {
                SetProgrammingVoltage = (PassThruSetProgrammingVoltage)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruSetProgrammingVoltage));
                APIsignature |= API_SIGNATURE.SETPROGRAMMINGVOLTAGE;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruReadVersion");
            if (pFunction != IntPtr.Zero)
            {
                ReadVersion = (PassThruReadVersion)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruReadVersion));
                APIsignature |= API_SIGNATURE.READVERSION;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruGetLastError");
            if (pFunction != IntPtr.Zero)
            {
                GetLastError = (PassThruGetLastError)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruGetLastError));
                APIsignature |= API_SIGNATURE.GETLASTERROR;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruIoctl");
            if (pFunction != IntPtr.Zero)
            {
                IOCtl = (PassThruIoctl)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruIoctl));
                APIsignature |= API_SIGNATURE.IOCTL;
            }

            //********************J2534v5*********************
            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruScanForDevices");
            if (pFunction != IntPtr.Zero)
            {
                ScanForDevices = (PassThruScanForDevices)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruScanForDevices));
                APIsignature |= API_SIGNATURE.SCANFORDEVICES;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruGetNextDevice");
            if (pFunction != IntPtr.Zero)
            {
                GetNextDevice = (PassThruGetNextDevice)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruGetNextDevice));
                APIsignature |= API_SIGNATURE.GETNEXTDEVICE;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruLogicalConnect");
            if (pFunction != IntPtr.Zero)
            {
                LogicalConnect = (PassThruLogicalConnect)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruLogicalConnect));
                APIsignature |= API_SIGNATURE.LOGICALCONNECT;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruLogicalDisconnect");
            if (pFunction != IntPtr.Zero)
            {
                LogicalDisconnect = (PassThruLogicalDisconnect)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruLogicalDisconnect));
                APIsignature |= API_SIGNATURE.LOGICALDISCONNECT;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruSelect");
            if (pFunction != IntPtr.Zero)
            {
                Select = (PassThruSelect)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruSelect));
                APIsignature |= API_SIGNATURE.SELECT;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruQueueMsgs");
            if (pFunction != IntPtr.Zero)
            {
                QueueMsgs = (PassThruQueueMsgs)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruQueueMsgs));
                APIsignature |= API_SIGNATURE.QUEUEMESSAGES;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruGetNextCarDAQ");
            if (pFunction != IntPtr.Zero)
                GetNextCarDAQ = (PassThruGetNextCarDAQ)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruGetNextCarDAQ));

            if(APIsignature == V202_SIGNATURE ||
                APIsignature == V404_SIGNATURE||
                APIsignature == V5_SIGNATURE)
                return true;
            return false;
        }

        internal bool FreeLibrary()
        {
            return NativeMethods.FreeLibrary(pLibrary);
        }
    }
}
