using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace J2534
{
    [StructLayout(LayoutKind.Explicit)]
    public class SConfig
    {
        [FieldOffset(0), MarshalAs(UnmanagedType.U4)]
        public J2534PARAMETER Parameter;
        [FieldOffset(4), MarshalAs(UnmanagedType.U4)]
        public int Value;

        public SConfig(J2534PARAMETER Parameter, int Value)
        {
            this.Parameter = Parameter;
            this.Value = Value;
        }
    }

    public class HeapSConfigArray : IDisposable
    {
        private IntPtr pSConfigArrayHeap;
        private bool disposed;
        //private int count;

        public HeapSConfigArray(SConfig ConfigItem)
        {
            //Create a blob big enough for 'ConfigItems' and two longs (NumOfItems and pItems)
            pSConfigArrayHeap = Marshal.AllocHGlobal(16);
            Length = 1;  //Set Number of items

            //Write pItems.  To save complexity, the array immediately follows SConfigArray.
            Marshal.WriteIntPtr(pSConfigArrayHeap, 4, IntPtr.Add(pSConfigArrayHeap, 8));

            //Write ConfigItem to the blob
            Marshal.StructureToPtr<SConfig>(ConfigItem, IntPtr.Add(pSConfigArrayHeap, 8), false);
        }

        public HeapSConfigArray(List<SConfig> ConfigItems)
        {
            //Create a blob big enough for 'ConfigItems' and two longs (NumOfItems and pItems)
            pSConfigArrayHeap = Marshal.AllocHGlobal(ConfigItems.Count * 8 + 8);
            Length = ConfigItems.Count;

            //Write pItems.  To save complexity, the array immediately follows SConfigArray.
            Marshal.WriteIntPtr(pSConfigArrayHeap, 4, IntPtr.Add(pSConfigArrayHeap, 8));

            //Write the array to the blob
            for (int i = 0, Offset = 8; i < ConfigItems.Count; i++, Offset += 8)
                Marshal.StructureToPtr<SConfig>(ConfigItems[i], IntPtr.Add(pSConfigArrayHeap, Offset), false);
        }

        public int Length
        {
            get
            {
                return Marshal.ReadInt32(pSConfigArrayHeap);
            }
            private set //Count should only be set by the constructor after the Marshal Alloc
            {
                Marshal.WriteInt32(pSConfigArrayHeap, value);
            }
        }

        public SConfig this[int Index]
        {
            get
            {
                return Marshal.PtrToStructure<SConfig>(IntPtr.Add(pSConfigArrayHeap, (Index * 8 + 8)));
            }
        }

        public static implicit operator IntPtr(HeapSConfigArray SConfigList)
        {
            return SConfigList.pSConfigArrayHeap;
        }

        public List<SConfig> ToList()
        {
            List<SConfig> List = new List<SConfig>(Length);
            for(int i = 0;i < Length; i++)
                List.Add(this[i]);
            return List;
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
            Marshal.FreeHGlobal(pSConfigArrayHeap);
            disposed = true;
        }
    }
}
