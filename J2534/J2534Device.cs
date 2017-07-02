using System;
using System.Runtime.InteropServices;

namespace J2534
{
    public class J2534Device
    {
        internal int DeviceID;
        internal J2534Library Library;
        public string FirmwareVersion;
        public string LibraryVersion;
        public string APIVersion;
        public string DeviceName;
        public string DrewtechVersion;
        public string DrewtechAddress;
        private bool ValidDevice;   //Flag used to determine if this device failed initial connection

        internal J2534Device(J2534Library Library)
        {
            this.Library = Library;
            ConnectToDevice("");
        }

        internal J2534Device(J2534Library Library, string DeviceName)
        {
            this.Library = Library;
            this.DeviceName = DeviceName;
            ConnectToDevice(this.DeviceName);
        }

        internal J2534Device(J2534Library Library, GetNextCarDAQResults CarDAQ)
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
            {   
                if(!ValidDevice)
                    return false;
                //GetVersion is used as a 'ping'
                return (GetVersion() == J2534ERR.STATUS_NOERROR);
            }
        }

        public J2534ERR ConnectToDevice(string Device)
        {
            J2534ERR Status;

            IntPtr pDeviceName = IntPtr.Zero;
            if (!string.IsNullOrEmpty(Device))
                pDeviceName = Marshal.StringToHGlobalAnsi(Device);
            else
                DeviceName = string.Format("Device {0}", J2534Discovery.PhysicalDevices.FindAll(Listed => Listed.Library == this.Library).Count + 1);

            J2534HeapInt DeviceID = new J2534HeapInt();

            lock (Library.API_LOCK)
            {
                Status = (J2534ERR)Library.API.Open(pDeviceName, DeviceID);

                Marshal.FreeHGlobal(pDeviceName);

                if (Status == J2534ERR.STATUS_NOERROR || (Library.API_Signature.SAE_API == SAE_API.V202_SIGNATURE &&
                                                            J2534Discovery.PhysicalDevices.FindAll(Listed => Listed.Library == this.Library).Count == 0 &&
                                                            IsConnected))
                {
                    this.DeviceID = DeviceID;
                    ValidDevice = true;
                    GetVersion();
                }
                return Status;
            }
        }

        public void DisconnectDevice()
        {
            J2534ERR Status;
            lock (Library.API_LOCK)
                Status = Library.API.Close(DeviceID);
            if (Status != J2534ERR.STATUS_NOERROR)
                throw new J2534Exception(Status, Library.GetLastError());
        }

        public void SetProgrammingVoltage(J2534PIN PinNumber, int Voltage)
        {
            J2534ERR Status;
            lock (Library.API_LOCK)
                Status = (J2534ERR)Library.API.SetProgrammingVoltage(DeviceID, (int)PinNumber, Voltage);
            if (Status != J2534ERR.STATUS_NOERROR)
                throw new J2534Exception(Status, Library.GetLastError());
        }

        private J2534ERR GetVersion()
        {
            J2534ERR Status;
            IntPtr pFirmwareVersion = Marshal.AllocHGlobal(80);
            IntPtr pDllVersion = Marshal.AllocHGlobal(80);
            IntPtr pApiVersion = Marshal.AllocHGlobal(80);

            lock (Library.API_LOCK)
            {
                Status = (J2534ERR)Library.API.ReadVersion(DeviceID, pFirmwareVersion, pDllVersion, pApiVersion);

                if (Status == J2534ERR.STATUS_NOERROR)
                {
                    FirmwareVersion = Marshal.PtrToStringAnsi(pFirmwareVersion);
                    LibraryVersion = Marshal.PtrToStringAnsi(pDllVersion);
                    APIVersion = Marshal.PtrToStringAnsi(pApiVersion);
                }
                //No exception is thrown because this method is used as a 'Ping' and I don't
                //want exceptions occuring just because a ping failed for any reason.
                Marshal.FreeHGlobal(pFirmwareVersion);
                Marshal.FreeHGlobal(pDllVersion);
                Marshal.FreeHGlobal(pApiVersion);
            }
            return Status;
        }

        public int MeasureBatteryVoltage()
        {
            J2534ERR Status;

            J2534HeapInt Voltage = new J2534HeapInt();
            lock (Library.API_LOCK)
            {
                Status = (J2534ERR)Library.API.IOCtl(DeviceID, (int)J2534IOCTL.READ_VBATT, IntPtr.Zero, Voltage);
                if (Status != J2534ERR.STATUS_NOERROR)
                    throw new J2534Exception(Status, Library.GetLastError());

                //The return was kept inside the lock here to ensure the conversion to INT is done before the
                //lock is released.  This is in case the API reuses the Ptr location for this data on subsequent
                //calls.  In that case, two back to back calls could interfere with each other of the second
                //call is allowed to execute before the first call marshals the Int from the heap.
                return Voltage;
            }
        }

        public int MeasureProgrammingVoltage()
        {
            J2534ERR Status;
            J2534HeapInt Voltage = new J2534HeapInt();

            lock (Library.API_LOCK)
            {
                Status = (J2534ERR)Library.API.IOCtl(DeviceID, (int)J2534IOCTL.READ_PROG_VOLTAGE, IntPtr.Zero, Voltage);
                if (Status != J2534ERR.STATUS_NOERROR)
                    throw new J2534Exception(Status, Library.GetLastError());
                return Voltage;
            }
        }

        public Channel ConstructChannel(J2534PROTOCOL ProtocolID, J2534BAUD Baud, J2534CONNECTFLAG ConnectFlags)
        {
            return new Channel(this, ProtocolID, Baud, ConnectFlags);
        }
    }
}
