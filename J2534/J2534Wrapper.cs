using System;
using System.Runtime.InteropServices;

namespace J2534
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

    internal class J2534APIWrapper:IDisposable
    {
        private bool disposed = false;
        private IntPtr pLibrary;

        //**************Prototypes for v2.02 functions that can get wrapped in v4.04 style delegates***************
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate J2534ERR PassThruConnectv202(int ProtocolID, int ConnectFlags, IntPtr ChannelID);
        internal PassThruConnectv202 Connectv202;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate J2534ERR PassThruSetProgrammingVoltagev202(int Pin, int Voltage);
        internal PassThruSetProgrammingVoltagev202 SetProgrammingVoltagev202;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate J2534ERR PassThruReadVersionv202(IntPtr pFirmwareVer, IntPtr pDllVer, IntPtr pAPIVer);
        internal PassThruReadVersionv202 ReadVersionv202;
        //********************************************************************************************************

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate J2534ERR PassThruConnect(int DeviceID, int ProtocolID, int ConnectFlags, int Baud, IntPtr ChannelID);
        internal PassThruConnect Connect = delegate (int DeviceID, int ProtocolID, int ConnectFlags, int Baud, IntPtr ChannelID) { return J2534ERR.FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate J2534ERR PassThruDisconnect(int channelId);
        internal PassThruDisconnect Disconnect = delegate (int channelId) { return J2534ERR.FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate J2534ERR PassThruReadMsgs(int ChannelID, IntPtr pUMsgArray, IntPtr NumMsgs, int Timeout);
        internal PassThruReadMsgs ReadMsgs = delegate (int ChannelID, IntPtr pMsgArray, IntPtr NumMsgs, int Timeout) { return J2534ERR.FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate J2534ERR PassThruWriteMsgs(int ChannelID, IntPtr pUMsgArray, IntPtr NumMsgs, int Timeout);
        internal PassThruWriteMsgs WriteMsgs = delegate (int ChannelID, IntPtr pMsgArray, IntPtr NumMsgs, int Timeout) { return J2534ERR.FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate J2534ERR PassThruStartPeriodicMsg(int ChannelID, IntPtr Msg, IntPtr MsgID, int Interval);
        internal PassThruStartPeriodicMsg StartPeriodicMsg = delegate (int ChannelID, IntPtr pMsg, IntPtr MsgID, int Interval) { return J2534ERR.FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate J2534ERR PassThruStopPeriodicMsg(int ChannelID, int MsgID);
        internal PassThruStopPeriodicMsg StopPeriodicMsg = delegate (int ChannelID, int MsgID) { return J2534ERR.FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate J2534ERR PassThruStartMsgFilter
        (
            int ChannelID,
            int FilterType,
            IntPtr pMaskMsg,
            IntPtr PatternMsg,
            IntPtr pFlowControlMsg,
            IntPtr FilterID
        );
        internal PassThruStartMsgFilter StartMsgFilter = delegate (
            int ChannelID,
            int FilterType,
            IntPtr pMaskMsg,
            IntPtr pPatternMsg,
            IntPtr pFlowControlMsg,
            IntPtr FilterID
        ) { return J2534ERR.FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate J2534ERR PassThruStopMsgFilter(int ChannelID, int FilterID);
        internal PassThruStopMsgFilter StopMsgFilter = delegate (int ChannelID, int FilterID) { return J2534ERR.FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate J2534ERR PassThruSetProgrammingVoltage(int DeviceID, int Pin, int Voltage);
        internal PassThruSetProgrammingVoltage SetProgrammingVoltage = delegate (int DeviceID, int Pin, int Voltage) { return J2534ERR.FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate J2534ERR PassThruReadVersion(int DeviceID, IntPtr pFirmwareVer, IntPtr pDllVer, IntPtr pAPIVer);
        internal PassThruReadVersion ReadVersion = delegate (int DeviceID, IntPtr pFirmwareVer, IntPtr pDllVer, IntPtr pAPIVer) { return J2534ERR.FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate J2534ERR PassThruGetLastError(IntPtr pErr);
        internal PassThruGetLastError GetLastError = delegate (IntPtr pErr) { return J2534ERR.FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate J2534ERR PassThruIoctl(int HandleID, int IOCtlID, IntPtr Input, IntPtr Output);
        internal PassThruIoctl IOCtl = delegate(int HandleID, int IOCtlID, IntPtr Input, IntPtr Output) { return J2534ERR.FUNCTION_NOT_ASSIGNED; } ;

        //**********v 4.04 calls****************
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate J2534ERR PassThruOpen(IntPtr pDeviceName, IntPtr DeviceID);
        internal PassThruOpen Open = delegate (IntPtr pDeviceName, IntPtr DeviceID) { return J2534ERR.FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate J2534ERR PassThruClose(int DeviceID);
        internal PassThruClose Close = delegate (int DeviceID) { return J2534ERR.FUNCTION_NOT_ASSIGNED; };


        //********************J2534 v5 and undocumented Drewtech calls*********************
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate J2534ERR PassThruScanForDevices(ref int DeviceCount);
        internal PassThruScanForDevices ScanForDevices = delegate (ref int DeviceCount) { return J2534ERR.FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate J2534ERR PassThruGetNextDevice(IntPtr pSDevice);
        internal PassThruGetNextDevice GetNextDevice = delegate (IntPtr pSDevice) { return J2534ERR.FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate J2534ERR PassThruLogicalConnect(int PhysicalChannelID, int ProtocolID, int ConnectFlags, IntPtr ChannelDescriptor, ref int pChannelID);
        internal PassThruLogicalConnect LogicalConnect = delegate (int PhysicalChannelID, int ProtocolID, int ConnectFlags, IntPtr ChannelDescriptor, ref int pChannelID) { return J2534ERR.FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate J2534ERR PassThruLogicalDisconnect(int pChannelID);
        internal PassThruLogicalDisconnect LogicalDisconnect = delegate (int pChannelID) { return J2534ERR.FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate J2534ERR PassThruSelect(IntPtr pSChannelSet, int SelectType, int Timeout);
        internal PassThruSelect Select = delegate (IntPtr pSChannelSet, int SelectType, int Timeout) { return J2534ERR.FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate J2534ERR PassThruQueueMsgs(int ChannelID, IntPtr pMsgArray, ref int NumMsgs);
        internal PassThruQueueMsgs QueueMsgs = delegate (int ChannelID, IntPtr pMsgArray, ref int NumMsgs) { return J2534ERR.FUNCTION_NOT_ASSIGNED; };
        
        //***************Drewtech only********************
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate J2534ERR PassThruGetNextCarDAQ(IntPtr pName, IntPtr pVer, IntPtr pAddress);
        internal PassThruGetNextCarDAQ GetNextCarDAQ = delegate (IntPtr pName, IntPtr pVer, IntPtr pAddress) { return J2534ERR.FUNCTION_NOT_ASSIGNED; };

        internal API_SIGNATURE LoadJ2534Library(string FileName)
        {
            API_SIGNATURE APISignature = new API_SIGNATURE();

            pLibrary = NativeMethods.LoadLibrary(FileName);

            if (pLibrary == IntPtr.Zero)
                return APISignature;

            IntPtr pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruOpen");
            if (pFunction != IntPtr.Zero)
            {
                Open = Marshal.GetDelegateForFunctionPointer<PassThruOpen>(pFunction);
                APISignature.SAE_API |= SAE_API.OPEN;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruClose");
            if (pFunction != IntPtr.Zero)
            {
                Close = Marshal.GetDelegateForFunctionPointer<PassThruClose>(pFunction);
                APISignature.SAE_API |= SAE_API.CLOSE;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruConnect");
            if (pFunction != IntPtr.Zero)
            {
                //If the API is v4.04 (because it has 'PassThruOpen')
                if (APISignature.SAE_API.HasFlag(SAE_API.OPEN))
                    //Make 'Connect' work directly with the library function
                    Connect = Marshal.GetDelegateForFunctionPointer<PassThruConnect>(pFunction);
                else
                {
                    //Otherwise, use the v202 prototype and wrap it with the v404 call
                    Connectv202 = Marshal.GetDelegateForFunctionPointer<PassThruConnectv202>(pFunction);
                    Connect = delegate (int DeviceID, int ProtocolID, int ConnectFlags, int Baud, IntPtr ChannelID)
                    {
                        if (DeviceID == 0)
                            return Connectv202(ProtocolID, ConnectFlags, ChannelID);
                        else
                            return J2534ERR.INVALID_DEVICE_ID;
                    };
                }
                APISignature.SAE_API |= SAE_API.CONNECT;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruDisconnect");
            if (pFunction != IntPtr.Zero)
            {
                Disconnect = Marshal.GetDelegateForFunctionPointer<PassThruDisconnect>(pFunction);
                APISignature.SAE_API |= SAE_API.DISCONNECT;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruReadMsgs");
            if (pFunction != IntPtr.Zero)
            {
                ReadMsgs = Marshal.GetDelegateForFunctionPointer<PassThruReadMsgs>(pFunction);
                APISignature.SAE_API |= SAE_API.READMSGS;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruWriteMsgs");
            if (pFunction != IntPtr.Zero)
            {
                WriteMsgs = Marshal.GetDelegateForFunctionPointer<PassThruWriteMsgs>(pFunction);
                APISignature.SAE_API |= SAE_API.WRITEMSGS;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruStartPeriodicMsg");
            if (pFunction != IntPtr.Zero)
            {
                StartPeriodicMsg = Marshal.GetDelegateForFunctionPointer<PassThruStartPeriodicMsg>(pFunction);
                APISignature.SAE_API |= SAE_API.STARTPERIODICMSG;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruStopPeriodicMsg");
            if (pFunction != IntPtr.Zero)
            {
                StopPeriodicMsg = Marshal.GetDelegateForFunctionPointer<PassThruStopPeriodicMsg>(pFunction);
                APISignature.SAE_API |= SAE_API.STOPPERIODICMSG;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruStartMsgFilter");
            if (pFunction != IntPtr.Zero)
            {
                StartMsgFilter = Marshal.GetDelegateForFunctionPointer<PassThruStartMsgFilter>(pFunction);
                APISignature.SAE_API |= SAE_API.STARTMSGFILTER;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruStopMsgFilter");
            if (pFunction != IntPtr.Zero)
            {
                StopMsgFilter = Marshal.GetDelegateForFunctionPointer<PassThruStopMsgFilter>(pFunction);
                APISignature.SAE_API |= SAE_API.STOPMSGFILTER;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruSetProgrammingVoltage");
            if (pFunction != IntPtr.Zero)
            {
                //If the API is v4.04 (because it has 'PassThruOpen')
                if (APISignature.SAE_API.HasFlag(SAE_API.OPEN))
                    //Make 'Connect' work directly with the library function
                    SetProgrammingVoltage = Marshal.GetDelegateForFunctionPointer<PassThruSetProgrammingVoltage>(pFunction);
                else
                {
                    //Otherwise, use the v202 prototype and wrap it with the v404 call
                    SetProgrammingVoltagev202 = Marshal.GetDelegateForFunctionPointer<PassThruSetProgrammingVoltagev202>(pFunction);
                    SetProgrammingVoltage = delegate (int DeviceID, int Pin, int Voltage)
                    {
                        if (DeviceID == 0)   //Is this necessary?
                            return SetProgrammingVoltagev202(Pin, Voltage);
                        else
                            return J2534ERR.INVALID_DEVICE_ID;
                    };
                }
                APISignature.SAE_API |= SAE_API.SETPROGRAMMINGVOLTAGE;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruReadVersion");
            if (pFunction != IntPtr.Zero)
            {
                //If the API is v4.04 (because it has 'PassThruOpen')
                if (APISignature.SAE_API.HasFlag(SAE_API.OPEN))
                    //Make 'Connect' work directly with the library function
                    ReadVersion = Marshal.GetDelegateForFunctionPointer<PassThruReadVersion>(pFunction);
                else
                {
                    //Otherwise, use the v202 prototype and wrap it with the v404 call
                    ReadVersionv202 = Marshal.GetDelegateForFunctionPointer<PassThruReadVersionv202>(pFunction);
                    ReadVersion = delegate (int DeviceID, IntPtr pFirmwareVer, IntPtr pDllVer, IntPtr pAPIVer)
                    {
                        if (DeviceID == 0)
                            return ReadVersionv202(pFirmwareVer, pDllVer, pAPIVer);
                        else
                            return J2534ERR.INVALID_DEVICE_ID;
                    };
                }
                APISignature.SAE_API |= SAE_API.READVERSION;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruGetLastError");
            if (pFunction != IntPtr.Zero)
            {
                GetLastError = Marshal.GetDelegateForFunctionPointer<PassThruGetLastError>(pFunction);
                APISignature.SAE_API |= SAE_API.GETLASTERROR;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruIoctl");
            if (pFunction != IntPtr.Zero)
            {
                IOCtl = Marshal.GetDelegateForFunctionPointer<PassThruIoctl>(pFunction);
                APISignature.SAE_API |= SAE_API.IOCTL;
            }

            //********************J2534v5*********************
            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruScanForDevices");
            if (pFunction != IntPtr.Zero)
            {
                ScanForDevices = Marshal.GetDelegateForFunctionPointer<PassThruScanForDevices>(pFunction);
                APISignature.SAE_API |= SAE_API.SCANFORDEVICES;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruGetNextDevice");
            if (pFunction != IntPtr.Zero)
            {
                GetNextDevice = Marshal.GetDelegateForFunctionPointer<PassThruGetNextDevice>(pFunction);
                APISignature.SAE_API |= SAE_API.GETNEXTDEVICE;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruLogicalConnect");
            if (pFunction != IntPtr.Zero)
            {
                LogicalConnect = Marshal.GetDelegateForFunctionPointer<PassThruLogicalConnect>(pFunction);
                APISignature.SAE_API |= SAE_API.LOGICALCONNECT;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruLogicalDisconnect");
            if (pFunction != IntPtr.Zero)
            {
                LogicalDisconnect = Marshal.GetDelegateForFunctionPointer<PassThruLogicalDisconnect>(pFunction);
                APISignature.SAE_API |= SAE_API.LOGICALDISCONNECT;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruSelect");
            if (pFunction != IntPtr.Zero)
            {
                Select = Marshal.GetDelegateForFunctionPointer<PassThruSelect>(pFunction);
                APISignature.SAE_API |= SAE_API.SELECT;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruQueueMsgs");
            if (pFunction != IntPtr.Zero)
            {
                QueueMsgs = Marshal.GetDelegateForFunctionPointer<PassThruQueueMsgs>(pFunction);
                APISignature.SAE_API |= SAE_API.QUEUEMESSAGES;
            }

            pFunction = NativeMethods.GetProcAddress(pLibrary, "PassThruGetNextCarDAQ");
            if (pFunction != IntPtr.Zero)
            {
                GetNextCarDAQ = Marshal.GetDelegateForFunctionPointer<PassThruGetNextCarDAQ>(pFunction);
                APISignature.DREWTECH_API |= DREWTECH_API.GETNEXTCARDAQ;
            }

            return APISignature;
        }

        internal bool FreeLibrary()
        {
            return NativeMethods.FreeLibrary(pLibrary);
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
            NativeMethods.FreeLibrary(pLibrary);
            disposed = true;
        }
    }
}
