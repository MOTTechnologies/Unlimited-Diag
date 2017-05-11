using System;
using System.Runtime.InteropServices;

namespace J2534
{
    internal class J2534DLL
    {
        internal string FileName;
        internal bool IsLoaded;
        internal J2534APIWrapper API;
        internal J2534ERR Status;
        private bool GetNextCarDAQFlag;

        internal J2534DLL(string LibFile)
        {
            GetNextCarDAQFlag = true;
            FileName = LibFile;
            API = new J2534.J2534APIWrapper();
            Load();
        }

        internal bool Load()
        {
            IsLoaded = API.LoadJ2534Library(FileName);
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

        internal void GetNextDevice()
        {
            //API.GetNextDevice(pSDevice);
        }

        internal GetNextCarDAQResults GetNextCarDAQ()
        {
            IntPtr pName = Marshal.AllocHGlobal(4);
            IntPtr pAddr = Marshal.AllocHGlobal(4);
            IntPtr pVer = Marshal.AllocHGlobal(4);

            if (GetNextCarDAQFlag)
            {
                Status = (J2534ERR)API.GetNextCarDAQ(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                GetNextCarDAQFlag = false;
            }
            Status = (J2534ERR)API.GetNextCarDAQ(pName, pVer, pAddr);

            if ((int)Status == J2534APIWrapper.FUNCTION_NOT_ASSIGNED || Marshal.ReadIntPtr(pName) == IntPtr.Zero)
            {
                Marshal.FreeHGlobal(pName);
                Marshal.FreeHGlobal(pVer);
                Marshal.FreeHGlobal(pAddr);
                return new GetNextCarDAQResults()
                { Empty = true };
            }

            byte[] b = new byte[3];
            Marshal.Copy(pVer, b, 0, 3);

            GetNextCarDAQResults Result = new GetNextCarDAQResults()
            {
                Empty = false,
                Device = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(pName)),
                Version = string.Format("{0}.{1}.{2}", b[0], b[1], b[2]),
                Address = Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(pAddr))
            };

            Marshal.FreeHGlobal(pName);
            Marshal.FreeHGlobal(pAddr);
            Marshal.FreeHGlobal(pVer);

            return Result;
        }
    }
}
