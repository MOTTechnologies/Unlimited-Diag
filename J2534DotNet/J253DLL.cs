using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
namespace J2534
{
    internal class J2534DLL
    {
        internal string FileName;
        internal API_SIGNATURE API_Signature;
        internal bool IsLoaded;
        internal J2534APIWrapper API;
        internal J2534ERR Status;
        internal int NumOfOpenDevices = 0;
        public string API_Support
        {
            get
            {
                StringBuilder API_String = new StringBuilder();
                if (API_Signature.DREWTECH_API != DREWTECH_API.NONE)
                    API_String.Append("DREWTECH ");
                else if (API_Signature.SAE_API != SAE_API.NONE)
                    API_String.Append("SAE ");
                else
                    API_String.Append("NO J2534 API DETECTED");

                switch (API_Signature.SAE_API)
                {
                    case SAE_API.V202_SIGNATURE:
                        API_String.Append("J2534 v2.02");
                        break;
                    case SAE_API.V404_SIGNATURE:
                        API_String.Append("J2534 v4.04");
                        break;
                    case SAE_API.V500_SIGNATURE:
                        API_String.Append("J2534 v5.00");
                        break;
                    default:
                        API_String.Append("UNKNOWN API");
                        break;
                }
                return API_String.ToString();
            }
        }

        internal J2534DLL(string FileName)
        {
            this.FileName = FileName;
            API = new J2534.J2534APIWrapper();
            Load();
        }

        internal bool Load()
        {
            API_Signature = API.LoadJ2534Library(FileName);

            if (API_Signature.SAE_API == SAE_API.V202_SIGNATURE ||
                API_Signature.SAE_API == SAE_API.V404_SIGNATURE ||
                API_Signature.SAE_API == SAE_API.V500_SIGNATURE)
                IsLoaded = true;
            else
            {
                IsLoaded = false;
                API.FreeLibrary();
            }
            return IsLoaded;
        }

        internal bool Free()
        {
            IsLoaded = API.FreeLibrary();   //Does this return a true or false????
            return IsLoaded;
        }

        internal J2534PhysicalDevice ConstructDevice()
        {
            return new J2534.J2534PhysicalDevice(this);
        }

        internal J2534PhysicalDevice ConstructDevice(string DeviceName)
        {
            return new J2534.J2534PhysicalDevice(this, DeviceName);
        }

        internal J2534PhysicalDevice ConstructDevice(GetNextCarDAQResults CarDAQ)
        {
            return new J2534.J2534PhysicalDevice(this, CarDAQ);
        }

        internal void GetNextDevice()
        {
            //API.GetNextDevice(pSDevice);
        }

        internal bool GetNextCarDAQ_RESET()
        {
            Status = (J2534ERR)API.GetNextCarDAQ(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            if (Status == J2534ERR.STATUS_NOERROR)
                return true;
            return false;
        }

        internal GetNextCarDAQResults GetNextCarDAQ()
        {
            IntPtr pName = Marshal.AllocHGlobal(4);
            IntPtr pAddr = Marshal.AllocHGlobal(4);
            IntPtr pVer = Marshal.AllocHGlobal(4);

            Status = (J2534ERR)API.GetNextCarDAQ(pName, pVer, pAddr);

            if (Status == J2534ERR.FUNCTION_NOT_ASSIGNED || Marshal.ReadIntPtr(pName) == IntPtr.Zero)
            {
                Marshal.FreeHGlobal(pName);
                Marshal.FreeHGlobal(pVer);
                Marshal.FreeHGlobal(pAddr);
                return new GetNextCarDAQResults()
                { Exists = false };
            }

            byte[] b = new byte[3];
            Marshal.Copy(pVer, b, 0, 3);

            GetNextCarDAQResults Result = new GetNextCarDAQResults()
            {
                Exists = true,
                Name = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(pName)),
                Version = string.Format("{0}.{1}.{2}", b[0], b[1], b[2]),
                Address = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(pAddr))
            };

            //if (!DrewtechDevices.Exists(CarDAQ => CarDAQ.Name == Result.Name))
            //    DrewtechDevices.Add(Result);

            Marshal.FreeHGlobal(pName);
            Marshal.FreeHGlobal(pAddr);
            Marshal.FreeHGlobal(pVer);

            return Result;
        }
    }
}
