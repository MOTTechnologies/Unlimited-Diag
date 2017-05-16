using System;
using System.Runtime.InteropServices;

namespace J2534
{
    public class J2534PhysicalDevice
    {
        internal IntPtr DeviceID;
        internal J2534DLL Library;
        public string FirmwareVersion;
        public string LibraryVersion;
        public string APIVersion;
        public string DeviceName;
        public string DrewtechVersion;
        public string DrewtechAddress;
        private bool ValidDevice;   //Flag used to determine if this device failed initial connection

        public J2534ERR Status
        {
            get
            {
                return Library.Status;
            }
            set
            {
                Library.Status = value;
            }
        }

        internal J2534PhysicalDevice(J2534DLL Library)
        {
            this.Library = Library;
            DeviceID = Marshal.AllocHGlobal(4);
            ConnectToDevice("");
        }

        internal J2534PhysicalDevice(J2534DLL Library, string DeviceName)
        {
            this.Library = Library;
            this.DeviceName = DeviceName;
            DeviceID = Marshal.AllocHGlobal(4);
            ConnectToDevice(this.DeviceName);
        }

        internal J2534PhysicalDevice(J2534DLL Library, GetNextCarDAQResults CarDAQ)
        {
            this.Library = Library;
            this.DeviceName = CarDAQ.Name;
            this.DrewtechVersion = CarDAQ.Version;
            this.DrewtechAddress = CarDAQ.Address;
            DeviceID = Marshal.AllocHGlobal(4);

            ConnectToDevice(DeviceName);
        }

        public bool IsConnected
        {
            get
            {   
                if(!ValidDevice)
                    return false;
                //GetVersion is used as a 'ping'
                return !GetVersion();
            }
        }

        public bool ConnectToDevice(string Device)
        {

            IntPtr pDeviceName = IntPtr.Zero;
            if (!string.IsNullOrEmpty(Device))
                pDeviceName = Marshal.StringToHGlobalAnsi(Device);
            else
                DeviceName = string.Format("Device {0}", J2534Discovery.PhysicalDevices.FindAll(Listed => Listed.Library == this.Library).Count + 1);

            Status = (J2534ERR)Library.API.Open(pDeviceName, DeviceID);

            Marshal.FreeHGlobal(pDeviceName);

            if (Status == J2534ERR.STATUS_NOERROR || (Library.API_Signature.SAE_API == SAE_API.V202_SIGNATURE &&
                                                      J2534Discovery.PhysicalDevices.FindAll(Listed => Listed.Library == this.Library).Count == 0 &&
                                                      IsConnected))
            {
                ValidDevice = true;
                GetVersion();
                return CONST.SUCCESS;
            }
            return CONST.FAILURE;
        }

        public bool DisconnectDevice()
        {
            Status = (J2534ERR)Library.API.Close(DeviceID);
            if (Status == J2534ERR.STATUS_NOERROR || Library.API_Signature.SAE_API == SAE_API.V202_SIGNATURE)
                return CONST.SUCCESS;
            return CONST.FAILURE;
        }

        public bool SetProgrammingVoltage(J2534PIN PinNumber, int Voltage)
        {

            Status = (J2534ERR)Library.API.SetProgrammingVoltage(DeviceID, (int)PinNumber, Voltage);
            if (Status == J2534ERR.STATUS_NOERROR)
                return CONST.SUCCESS;
            return CONST.FAILURE;
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
                return CONST.SUCCESS;
            return CONST.FAILURE;
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
            //Needs to have a handler for v2.02
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
