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

        private IntPtr DLL_Handle;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruOpen(IntPtr pDeviceName, ref int DeviceID);
        internal PassThruOpen Open;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruClose(int DeviceID);
        internal PassThruClose Close;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruConnect(int DeviceID, int ProtocolID, int ConnectFlags, int Baud, ref int ChannelID);
        internal PassThruConnect Connect;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruDisconnect(int channelId);
        internal PassThruDisconnect Disconnect;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruReadMsgs(int ChannelID, IntPtr pUMsgArray, ref int NumMsgs, int Timeout);
        internal PassThruReadMsgs ReadMsgs;
        //extern “C” long WINAPI PassThruReadMsgs
        //(
        //unsigned long ChannelID,
        //PASSTHRU_MSG *pMsg,
        //unsigned long *pNumMsgs,
        //unsigned long Timeout
        //)

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruWriteMsgs(int ChannelID, IntPtr pUMsgArray, ref int NumMsgs, int Timeout);
        internal PassThruWriteMsgs WriteMsgs;
        //extern “C” long WINAPI PassThruWriteMsgs
        //(
        //unsigned long ChannelID,
        //PASSTHRU_MSG *pMsg,
        //unsigned long *pNumMsgs,
        //unsigned long Timeout
        //)

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruStartPeriodicMsg(int ChannelID, ref UnsafePassThruMsg pUMsg, ref int MsgID, int Interval);
        internal PassThruStartPeriodicMsg StartPeriodicMsg;
        //extern “C” long WINAPI PassThruStartPeriodicMsg
        //(
        //unsigned long ChannelID,
        //PASSTHRU_MSG *pMsg,
        //unsigned long *pMsgID,
        //unsigned long TimeInterval
        //)

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruStopPeriodicMsg(int ChannelID, int MsgID);
        internal PassThruStopPeriodicMsg StopPeriodicMsg;
        //extern “C” long WINAPI PassThruStopPeriodicMsg
        //(
        //unsigned long ChannelID,
        //unsigned long MsgID
        //)

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruStartMsgFilter
        (
            int ChannelID,
            int FilterType,
            ref UnsafePassThruMsg uMaskMsg,
            ref UnsafePassThruMsg uPatternMsg,
            ref UnsafePassThruMsg uFlowControlMsg,
            ref int FilterID
        );
        internal PassThruStartMsgFilter StartMsgFilter;
        //extern “C” long WINAPI PassThruStartMsgFilter
        //(
        //unsigned long ChannelID,
        //unsigned long FilterType,
        //PASSTHRU_MSG *pMaskMsg,
        //PASSTHRU_MSG *pPatternMsg,
        //PASSTHRU_MSG *pFlowControlMsg,
        //unsigned long *pFilterID
        //)

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruStopMsgFilter(int ChannelID, int FilterID);
        internal PassThruStopMsgFilter StopMsgFilter;
        //extern “C” long WINAPI PassThruStopMsgFilter
        //(
        //unsigned long ChannelID,
        //unsigned long FilterID
        //)

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruSetProgrammingVoltage(int DeviceID, int Pin, int Voltage);
        internal PassThruSetProgrammingVoltage SetProgrammingVoltage;
        //extern “C” long WINAPI PassThruSetProgrammingVoltage
        //(
        //unsigned long DeviceID,
        //unsigned long PinNumber,
        //unsigned long Voltage
        //)        

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruReadVersion(int DeviceID, IntPtr FirmwareVer, IntPtr DllVer, IntPtr APIVer);
        internal PassThruReadVersion ReadVersion;
        //extern “C” long WINAPI PassThruReadVersion
        //(
        //unsigned long DeviceID
        //char *pFirmwareVersion,
        //char *pDllVersion,
        //char *pApiVersion
        //)

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruGetLastError(IntPtr pErr);
        internal PassThruGetLastError GetLastError;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        internal delegate int PassThruIoctl(int ChannelID, int IOCtlID, IntPtr Input, IntPtr Output);
        internal PassThruIoctl IOCtl;
        //extern “C” long WINAPI PassThruIoctl
        //(
        //unsigned long ChannelID,
        //unsigned long IoctlID,
        //void *pInput,
        //void *pOutput
        //)

        internal bool LoadJ2534Library(string FileName)
        {
            DLL_Handle = NativeMethods.LoadLibrary(FileName);

            if (DLL_Handle == IntPtr.Zero)
                return false;

            IntPtr pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruOpen");
            if (pFunction != IntPtr.Zero)
                Open = (PassThruOpen)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruOpen));

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruClose");
            if (pFunction != IntPtr.Zero)
                Close = (PassThruClose)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruClose));

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruConnect");
            if (pFunction != IntPtr.Zero)
                Connect = (PassThruConnect)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruConnect));

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruDisconnect");
            if (pFunction != IntPtr.Zero)
                Disconnect = (PassThruDisconnect)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruDisconnect));

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruReadMsgs");
            if (pFunction != IntPtr.Zero)
                ReadMsgs = (PassThruReadMsgs)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruReadMsgs));

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruWriteMsgs");
            if (pFunction != IntPtr.Zero)
                WriteMsgs = (PassThruWriteMsgs)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruWriteMsgs));

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruStartPeriodicMsg");
            if (pFunction != IntPtr.Zero)
                StartPeriodicMsg = (PassThruStartPeriodicMsg)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruStartPeriodicMsg));

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruStopPeriodicMsg");
            if (pFunction != IntPtr.Zero)
                StopPeriodicMsg = (PassThruStopPeriodicMsg)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruStopPeriodicMsg));

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruStartMsgFilter");
            if (pFunction != IntPtr.Zero)
                StartMsgFilter = (PassThruStartMsgFilter)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruStartMsgFilter));

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruStopMsgFilter");
            if (pFunction != IntPtr.Zero)
                StopMsgFilter = (PassThruStopMsgFilter)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruStopMsgFilter));

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruSetProgrammingVoltage");
            if (pFunction != IntPtr.Zero)
                SetProgrammingVoltage = (PassThruSetProgrammingVoltage)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruSetProgrammingVoltage));

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruReadVersion");
            if (pFunction != IntPtr.Zero)
                ReadVersion = (PassThruReadVersion)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruReadVersion));

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruGetLastError");
            if (pFunction != IntPtr.Zero)
                GetLastError = (PassThruGetLastError)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruGetLastError));

            pFunction = NativeMethods.GetProcAddress(DLL_Handle, "PassThruIoctl");
            if (pFunction != IntPtr.Zero)
                IOCtl = (PassThruIoctl)Marshal.GetDelegateForFunctionPointer(pFunction, typeof(PassThruIoctl));

            return true;
        }

        internal bool FreeLibrary()
        {
            return NativeMethods.FreeLibrary(DLL_Handle);
        }
    }
}
