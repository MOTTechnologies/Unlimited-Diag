using System;
using System.Runtime.InteropServices;

namespace J2534
{
    public class J2534HeapInt : IDisposable
    {
        private bool disposed;
        private IntPtr pInt;

        public J2534HeapInt()
        {
            pInt = Marshal.AllocHGlobal(4);
        }

        public J2534HeapInt(int i)
        {
            pInt = Marshal.AllocHGlobal(4);
            Marshal.WriteInt32(pInt, i);
        }

        public static implicit operator IntPtr(J2534HeapInt HeapInt)
        {
            return HeapInt.pInt;
        }

        public static implicit operator int(J2534HeapInt HeapInt)
        {
            return Marshal.ReadInt32(HeapInt.pInt);
        }

        public static implicit operator J2534HeapInt(int i)
        {
            return new J2534HeapInt(i);
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
            Marshal.FreeHGlobal(pInt);
            disposed = true;
        }
    }
}
