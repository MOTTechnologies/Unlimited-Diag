using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace J2534
{
    public class J2534HeapMessageArray : IDisposable
    {
        private int array_max_length;
        private IntPtr pMessages;
        private J2534HeapInt length;
        private bool disposed;

        public J2534HeapMessageArray(int Length)
        {
            array_max_length = Length;
            length = new J2534HeapInt();
            pMessages = Marshal.AllocHGlobal(CONST.J2534MESSAGESIZE * Length);
        }

        public J2534HeapInt Length
        {
            get
            {
                return length;
            }
            set
            {
                if (value > array_max_length)
                {
                    throw new IndexOutOfRangeException("Length is greater than array bound");
                }
                else
                    length = value;
            }
        }

        public J2534Message this[int index]
        {
            get
            {
                if (index > length)
                {
                    throw new IndexOutOfRangeException("Index is greater than array bound");
                }
                IntPtr pMessage = IntPtr.Add(pMessages, index * CONST.J2534MESSAGESIZE);
                return new J2534Message()
                {
                    ProtocolID = (J2534PROTOCOL)Marshal.ReadInt32(pMessage),
                    RxStatus = (J2534RXFLAG)Marshal.ReadInt32(pMessage, 4),
                    TxFlags = (J2534TXFLAG)Marshal.ReadInt32(pMessage, 8),
                    Timestamp = (uint)Marshal.ReadInt32(pMessage, 12),
                    ExtraDataIndex = (uint)Marshal.ReadInt32(pMessage, 20),
                    Data = MarshalDataArray(pMessage),
                };
            }
            set
            {
                if (index > length)
                {
                    throw new IndexOutOfRangeException("Index is greater than array bound");
                }
                IntPtr pMessage = IntPtr.Add(pMessages, index * CONST.J2534MESSAGESIZE);
                Marshal.WriteInt32(pMessage, (int)value.ProtocolID);
                Marshal.WriteInt32(pMessage, 4, (int)value.RxStatus);
                Marshal.WriteInt32(pMessage, 8, (int)value.TxFlags);
                Marshal.WriteInt32(pMessage, 12, (int)value.Timestamp);
                Marshal.WriteInt32(pMessage, 16, value.Data.Length);
                Marshal.WriteInt32(pMessage, 20, (int)value.ExtraDataIndex);
                Marshal.Copy(value.Data, 0, IntPtr.Add(pMessage, 24), value.Data.Length);
            }
        }

        public List<J2534Message> ToList()
        {
            List<J2534Message> return_list = new List<J2534Message>();
            for (int i = 0; i < Length; i++)
                return_list.Add(this[i]);
            return return_list;
        }

        private byte[] MarshalDataArray(IntPtr pData)
        {
            int Length = Marshal.ReadInt32(pData, 16);
            byte[] data = new byte[Length];
            Marshal.Copy(IntPtr.Add(pData, 24), data, 0, Length);
            return data;
        }

        public static implicit operator IntPtr(J2534HeapMessageArray HeapMessageArray)
        {
            return HeapMessageArray.pMessages;
        }

        public void PopulateWith(List<J2534Message> Messages)
        {
            Length = Messages.Count;
            for (int i = 0; i < Messages.Count; i++)
                this[i] = Messages[i];
        }

        public void PopulateWith(J2534Message Message)
        {
            Length = 1;
            this[0] = Message;
        }

        public void PopulateWith(J2534PROTOCOL ProtocolID, J2534TXFLAG TxFlags, byte[] Data)
        {
            Length = 1;
            Marshal.WriteInt32(pMessages, (int)ProtocolID);
            Marshal.WriteInt32(pMessages, 8, (int)TxFlags);
            Marshal.WriteInt32(pMessages, 16, Data.Length);
            Marshal.Copy(Data, 0, IntPtr.Add(pMessages, 24), Data.Length);
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
            Marshal.FreeHGlobal(pMessages);
            length.Dispose();
            disposed = true;
        }
    }
}
