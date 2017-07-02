using System;
using System.Runtime.InteropServices;

namespace J2534
{
    public class HeapSByteArray : IDisposable
    {
        private IntPtr pSByteArray;
        private bool disposed;

        public HeapSByteArray(byte SingleByte)
        {
            pSByteArray = Marshal.AllocHGlobal(9);
            Length = 1;
            Marshal.WriteIntPtr(pSByteArray, 4, IntPtr.Add(pSByteArray, 8));
            Marshal.WriteByte(IntPtr.Add(pSByteArray, 8), SingleByte);
        }

        public HeapSByteArray(byte[] SByteArray)
        {
            pSByteArray = Marshal.AllocHGlobal(SByteArray.Length + 8);
            Length = SByteArray.Length;
            Marshal.WriteIntPtr(pSByteArray, 4, IntPtr.Add(pSByteArray, 8));
            Marshal.Copy(SByteArray, 0, IntPtr.Add(pSByteArray, 8), SByteArray.Length);
        }

        public int Length
        {
            get
            {
                return Marshal.ReadInt32(pSByteArray);
            }
            private set
            {
                Marshal.WriteInt32(pSByteArray, value);
            }
        }

        public byte this[int Index]
        {
            get
            {
                if (Index < Length)
                    return Marshal.ReadByte(IntPtr.Add(pSByteArray, Index + 8));
                throw new IndexOutOfRangeException("Index is greater than array bound");
            }
        }

        public static implicit operator IntPtr(HeapSByteArray HeapSByteArray)
        {
            return HeapSByteArray.pSByteArray;
        }

        public static implicit operator byte[] (HeapSByteArray HeapSByteArray)
        {
            int Length = Marshal.ReadInt32(HeapSByteArray.pSByteArray);
            byte[] Array = new byte[Length];
            Marshal.Copy(IntPtr.Add(HeapSByteArray.pSByteArray, 8), Array, 0, Length);
            return Array;
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
            Marshal.FreeHGlobal(pSByteArray);
            disposed = true;
        }
    }
}
