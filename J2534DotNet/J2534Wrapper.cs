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
using System.Runtime.InteropServices;

namespace J2534DotNet
{
    internal unsafe struct UnsafePassThruMsg
    {
        public uint ProtocolID;
        public uint RxStatus;
        public uint TxFlags;
        public uint Timestamp;
        public uint DataSize;
        public uint ExtraDataIndex;
        public fixed byte Data[4128];
    }

    internal static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);
    }

    internal class J2534APIWrapper
    {
        const int FUNCTION_NOT_ASSIGNED = 0xFFFE;

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

        private IntPtr DLL_Handle;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruConnect(int DeviceID, int ProtocolID, int ConnectFlags, int Baud, ref int ChannelID);
        internal PassThruConnect Connect = delegate (int DeviceID, int ProtocolID, int ConnectFlags, int Baud, ref int ChannelID) { return FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruDisconnect(int channelId);
        internal PassThruDisconnect Disconnect = delegate (int channelId) { return FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruReadMsgs(int ChannelID, IntPtr pUMsgArray, ref int NumMsgs, int Timeout);
        internal PassThruReadMsgs ReadMsgs = delegate (int ChannelID, IntPtr pUMsgArray, ref int NumMsgs, int Timeout) { return FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruWriteMsgs(int ChannelID, IntPtr pUMsgArray, ref int NumMsgs, int Timeout);
        internal PassThruWriteMsgs WriteMsgs = delegate (int ChannelID, IntPtr pUMsgArray, ref int NumMsgs, int Timeout) { return FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruStartPeriodicMsg(int ChannelID, ref UnsafePassThruMsg pUMsg, ref int MsgID, int Interval);
        internal PassThruStartPeriodicMsg StartPeriodicMsg = delegate (int ChannelID, ref UnsafePassThruMsg pUMsg, ref int MsgID, int Interval) { return FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruStopPeriodicMsg(int ChannelID, int MsgID);
        internal PassThruStopPeriodicMsg StopPeriodicMsg = delegate (int ChannelID, int MsgID) { return FUNCTION_NOT_ASSIGNED; };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruStartMsgFilter
        (
            int ChannelID,
            int FilterType,
            ref UnsafePassThruMsg uMaskMsg,
            ref UnsafePassThruMsg uPatternMsg,
            //ref UnsafePassThruMsg uFlowControlMsg,
            IntPtr puFlowControlMsg,
            ref int FilterID
        );
        internal PassThruStartMsgFilter StartMsgFilter = delegate (
            int ChannelID,
            int FilterType,
            ref UnsafePassThruMsg uMaskMsg,
            ref UnsafePassThruMsg uPatternMsg,
            IntPtr puFlowControlMsg,
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
        internal delegate int PassThruQueueMsgs(int ChannelID, IntPtr pUMsgArray, ref int NumMsgs);
        internal PassThruQueueMsgs QueueMsgs = delegate (int ChannelID, IntPtr pUMsgArray, ref int NumMsgs) { return FUNCTION_NOT_ASSIGNED; };
        
        //***************Drewtech only********************
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruGetNextCarDAQ(IntPtr pName, ref int Version, IntPtr pAddress);
        internal PassThruGetNextCarDAQ GetNextCarDAQ = delegate (IntPtr pName, ref int Version, IntPtr pAddress) { return FUNCTION_NOT_ASSIGNED; };

        internal bool LoadJ2534Library(string FileName)
        {
            API_SIGNATURE APIsignature = API_SIGNATURE.NONE;

            DLL_Handle = NativeMethods.LoadLibrary(FileName);

            if (DLL_Handle == IntPtr.Zero)
                return false;

            IntPtr pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruOpen");
            if (pFunction != IntPtr.Zero)
            {
                Open = (PassThruOpen)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruOpen));
                APIsignature |= API_SIGNATURE.OPEN;
            }

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruClose");
            if (pFunction != IntPtr.Zero)
            {
                Close = (PassThruClose)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruClose));
                APIsignature |= API_SIGNATURE.CLOSE;
            }

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruConnect");
            if (pFunction != IntPtr.Zero)
            {
                Connect = (PassThruConnect)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruConnect));
                APIsignature |= API_SIGNATURE.CONNECT;
            }

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruDisconnect");
            if (pFunction != IntPtr.Zero)
            {
                Disconnect = (PassThruDisconnect)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruDisconnect));
                APIsignature |= API_SIGNATURE.DISCONNECT;
            }

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruReadMsgs");
            if (pFunction != IntPtr.Zero)
            {
                ReadMsgs = (PassThruReadMsgs)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruReadMsgs));
                APIsignature |= API_SIGNATURE.READMSGS;
            }

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruWriteMsgs");
            if (pFunction != IntPtr.Zero)
            {
                WriteMsgs = (PassThruWriteMsgs)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruWriteMsgs));
                APIsignature |= API_SIGNATURE.WRITEMSGS;
            }

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruStartPeriodicMsg");
            if (pFunction != IntPtr.Zero)
            {
                StartPeriodicMsg = (PassThruStartPeriodicMsg)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruStartPeriodicMsg));
                APIsignature |= API_SIGNATURE.STARTPERIODICMSG;
            }

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruStopPeriodicMsg");
            if (pFunction != IntPtr.Zero)
            {
                StopPeriodicMsg = (PassThruStopPeriodicMsg)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruStopPeriodicMsg));
                APIsignature |= API_SIGNATURE.STOPPERIODICMSG;
            }

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruStartMsgFilter");
            if (pFunction != IntPtr.Zero)
            {
                StartMsgFilter = (PassThruStartMsgFilter)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruStartMsgFilter));
                APIsignature |= API_SIGNATURE.STARTMSGFILTER;
            }

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruStopMsgFilter");
            if (pFunction != IntPtr.Zero)
            {
                StopMsgFilter = (PassThruStopMsgFilter)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruStopMsgFilter));
                APIsignature |= API_SIGNATURE.STOPMSGFILTER;
            }

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruSetProgrammingVoltage");
            if (pFunction != IntPtr.Zero)
            {
                SetProgrammingVoltage = (PassThruSetProgrammingVoltage)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruSetProgrammingVoltage));
                APIsignature |= API_SIGNATURE.SETPROGRAMMINGVOLTAGE;
            }

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruReadVersion");
            if (pFunction != IntPtr.Zero)
            {
                ReadVersion = (PassThruReadVersion)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruReadVersion));
                APIsignature |= API_SIGNATURE.READVERSION;
            }

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruGetLastError");
            if (pFunction != IntPtr.Zero)
            {
                GetLastError = (PassThruGetLastError)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruGetLastError));
                APIsignature |= API_SIGNATURE.GETLASTERROR;
            }

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruIoctl");
            if (pFunction != IntPtr.Zero)
            {
                IOCtl = (PassThruIoctl)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruIoctl));
                APIsignature |= API_SIGNATURE.IOCTL;
            }

            //********************J2534v5*********************
            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruScanForDevices");
            if (pFunction != IntPtr.Zero)
            {
                ScanForDevices = (PassThruScanForDevices)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruScanForDevices));
                APIsignature |= API_SIGNATURE.SCANFORDEVICES;
            }

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruGetNextDevice");
            if (pFunction != IntPtr.Zero)
            {
                GetNextDevice = (PassThruGetNextDevice)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruGetNextDevice));
                APIsignature |= API_SIGNATURE.GETNEXTDEVICE;
            }

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruLogicalConnect");
            if (pFunction != IntPtr.Zero)
            {
                LogicalConnect = (PassThruLogicalConnect)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruLogicalConnect));
                APIsignature |= API_SIGNATURE.LOGICALCONNECT;
            }

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruLogicalDisconnect");
            if (pFunction != IntPtr.Zero)
            {
                LogicalDisconnect = (PassThruLogicalDisconnect)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruLogicalDisconnect));
                APIsignature |= API_SIGNATURE.LOGICALDISCONNECT;
            }

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruSelect");
            if (pFunction != IntPtr.Zero)
            {
                Select = (PassThruSelect)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruSelect));
                APIsignature |= API_SIGNATURE.SELECT;
            }

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruQueueMsgs");
            if (pFunction != IntPtr.Zero)
            {
                QueueMsgs = (PassThruQueueMsgs)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruQueueMsgs));
                APIsignature |= API_SIGNATURE.QUEUEMESSAGES;
            }

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruGetNextCarDAQ");
            if (pFunction != IntPtr.Zero)
                IOCtl = (PassThruIoctl)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruIoctl));

            if(APIsignature == V202_SIGNATURE ||
                APIsignature == V404_SIGNATURE||
                APIsignature == V5_SIGNATURE)
                return true;
            return false;
        }

        internal bool FreeLibrary()
        {
            return NativeMethods.FreeLibrary(DLL_Handle);
        }
    }
}
