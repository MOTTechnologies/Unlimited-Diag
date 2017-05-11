using System;
using System.Runtime.InteropServices;

namespace J2534
{
    public class J2534PhysicalDevice
    {
        public J2534ERR Status;
        //public object Status;
        internal int DeviceID;
        public bool IsConnected;
        internal J2534DLL Library;
        public string FirmwareVersion;
        public string LibraryVersion;
        public string APIVersion;

        internal J2534PhysicalDevice(J2534DLL Library)
        {
            //Status = _Status;
            this.Library = Library;
            ConnectToDevice("");
        }

        //Devicenames that work are "CarDAQ-Plus1331" and "192.168.43.101"
        internal J2534PhysicalDevice(J2534DLL Library, string DeviceName)
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
                voltage = Marshal.ReadInt32(output);

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
}
