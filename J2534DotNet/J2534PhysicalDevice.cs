using System;
using System.Runtime.InteropServices;

namespace J2534
{
    public class J2534PhysicalDevice
    {
        public J2534ERR Status;
        //public object Status;
        internal int DeviceID;
        internal J2534DLL Library;
        public string FirmwareVersion;
        public string LibraryVersion;
        public string APIVersion;

        public string DeviceName;
        public string DrewtechVersion;
        public string DrewtechAddress;

        internal J2534PhysicalDevice(J2534DLL Library)
        {
            this.Library = Library;
            ConnectToDevice("");
        }

        internal J2534PhysicalDevice(J2534DLL Library, string DeviceName)
        {
            this.Library = Library;
            this.DeviceName = DeviceName;
            ConnectToDevice(this.DeviceName);
        }

        internal J2534PhysicalDevice(J2534DLL Library, GetNextCarDAQResults CarDAQ)
        {
            this.Library = Library;
            this.DeviceName = CarDAQ.Name;
            this.DrewtechVersion = CarDAQ.Version;
            this.DrewtechAddress = CarDAQ.Address;

            ConnectToDevice(DeviceName);
        }

        public bool IsConnected
        {
            get
            {   //This is a hack to make a 2nd device opened with v2.02 show as "not connected"
                if (string.IsNullOrEmpty(DeviceName))
                    return false;
                //I use GetVersion as a ping to the target device
                return GetVersion();
            }
        }

        public bool ConnectToDevice(string Device)
        {
            //Do not allow more than one device connection when using v2.02 API
            if (Library.API_Signature.SAE_API == SAE_API.V202_SIGNATURE && Library.NumOfOpenDevices > 0)
                return true;
            //DeviceID is set to zero as a default in the event of a v2.02 connect event
            DeviceID = 0;
            IntPtr pDeviceName = IntPtr.Zero;
            if (!string.IsNullOrEmpty(Device))
                pDeviceName = Marshal.StringToHGlobalAnsi(Device);

            Status = (J2534ERR)Library.API.Open(pDeviceName, ref DeviceID);

            string TestName = Marshal.PtrToStringAnsi(pDeviceName);

            Marshal.FreeHGlobal(pDeviceName);

            if (Status == J2534ERR.STATUS_NOERROR)
            {
                GetVersion();
                Library.NumOfOpenDevices++;
                if (string.IsNullOrEmpty(DeviceName))
                    DeviceName = string.Format("Device {0}", Library.NumOfOpenDevices);
                return false;
            }
            else if (Library.API_Signature.SAE_API == SAE_API.V202_SIGNATURE &&
                     IsConnected)
            {
                Library.NumOfOpenDevices++;
                if (string.IsNullOrEmpty(DeviceName))
                    DeviceName = string.Format("Device {0}", Library.NumOfOpenDevices);
                return false;
            }
            return true;
        }

        public bool DisconnectDevice()
        {
            Status = (J2534ERR)Library.API.Close(DeviceID);
            if (Status == J2534ERR.STATUS_NOERROR ||
                (Library.API_Signature.SAE_API == SAE_API.V202_SIGNATURE && Library.NumOfOpenDevices > 0))
            {
                Library.NumOfOpenDevices--;
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
