using System;
using System.Runtime.InteropServices;

namespace J2534
{
    //Class for creating a single message on the heap.  Used for Periodic messages, filters, etc.
    public class J2534HeapMessage : IDisposable
    {
        private IntPtr pMessage;
        private bool disposed;

        public J2534HeapMessage()
        {
            pMessage = Marshal.AllocHGlobal(CONST.J2534MESSAGESIZE);
        }

        public J2534HeapMessage(J2534PROTOCOL ProtocolID, J2534TXFLAG TxFlags, byte[] Data)
        {
            pMessage = Marshal.AllocHGlobal(CONST.J2534MESSAGESIZE);
            this.ProtocolID = ProtocolID;
            this.TxFlags = TxFlags;
            this.Data = Data;
        }

        public J2534HeapMessage(J2534Message Message)
        {
            pMessage = Marshal.AllocHGlobal(CONST.J2534MESSAGESIZE);
            this.Message = Message;
        }

        public J2534Message Message
        {
            get
            {
                return new J2534Message()
                {
                    ProtocolID = this.ProtocolID,
                    RxStatus = this.RxStatus,
                    TxFlags = this.TxFlags,
                    Timestamp = this.Timestamp,
                    ExtraDataIndex = this.ExtraDataIndex,
                    Data = MarshalDataArray(pMessage),
                };
            }
            set
            {
                this.ProtocolID = value.ProtocolID;
                this.RxStatus = value.RxStatus;
                this.TxFlags = value.TxFlags;
                this.Timestamp = value.Timestamp;
                this.ExtraDataIndex = value.ExtraDataIndex;
                this.Data = value.Data;
            }
        }

        public J2534PROTOCOL ProtocolID
        {
            get
            {
                return (J2534PROTOCOL)Marshal.ReadInt32(pMessage); 
            }
            set
            {
                Marshal.WriteInt32(pMessage, (int)value);
            }
        }

        public J2534RXFLAG RxStatus
        {
            get
            {
                return (J2534RXFLAG)Marshal.ReadInt32(pMessage, 4);
            }
            set
            {
                Marshal.WriteInt32(pMessage, 4, (int)value);
            }
        }

        public J2534TXFLAG TxFlags
        {
            get
            {
                return (J2534TXFLAG)Marshal.ReadInt32(pMessage, 8);
            }
            set
            {
                Marshal.WriteInt32(pMessage, 8, (int)value);
            }
        }

        public uint Timestamp
        {
            get
            {
                return (uint)Marshal.ReadInt32(pMessage, 12);
            }
            set
            {
                Marshal.WriteInt32(pMessage, 12, (int)value);
            }
        }

        public uint ExtraDataIndex
        {
            get
            {
                return (uint)Marshal.ReadInt32(pMessage, 20);
            }
            set
            {
                Marshal.WriteInt32(pMessage, 20, (int)value);
            }
        }

        public int Length
        {
            get
            {
                return Marshal.ReadInt32(pMessage, 16);
            }
            private set
            {
                Marshal.WriteInt32(pMessage, 16, value);
            }
        }

        public byte[] Data
        {
            get
            {
                return MarshalDataArray(pMessage);
            }
            set
            {
                if (value.Length > (CONST.J2534MESSAGESIZE - 24))
                {
                    throw new ArgumentException("Message Data.Length is greator than fixed maximum");
                }
                else
                {
                    Length = value.Length;
                    Marshal.Copy(value, 0, IntPtr.Add(pMessage, 24), value.Length);
                }
            }
        }

        private static byte[] MarshalDataArray(IntPtr pData)
        {
            int Length = Marshal.ReadInt32(pData, 16);
            byte[] data = new byte[Length];
            Marshal.Copy(IntPtr.Add(pData, 24), data, 0, Length);
            return data;
        }

        public static implicit operator IntPtr(J2534HeapMessage HeapMessage)
        {
            return HeapMessage.pMessage;
        }

        public static implicit operator J2534Message(J2534HeapMessage HeapMessage)
        {
            return new J2534Message()
            {
                ProtocolID = (J2534PROTOCOL)Marshal.ReadInt32(HeapMessage.pMessage),
                RxStatus = (J2534RXFLAG)Marshal.ReadInt32(HeapMessage.pMessage, 4),
                TxFlags = (J2534TXFLAG)Marshal.ReadInt32(HeapMessage.pMessage, 8),
                Timestamp = (uint)Marshal.ReadInt32(HeapMessage.pMessage, -12),
                ExtraDataIndex = (uint)Marshal.ReadInt32(HeapMessage.pMessage, 20),
                Data = MarshalDataArray(HeapMessage.pMessage),
            };
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
            Marshal.FreeHGlobal(pMessage);
            disposed = true;
        }
    }
}
