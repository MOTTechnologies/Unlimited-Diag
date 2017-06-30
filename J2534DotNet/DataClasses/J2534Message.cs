using System;

namespace J2534
{
    public class J2534Message
    {
        public J2534PROTOCOL ProtocolID { get; set; }
        public J2534RXFLAG RxStatus { get; set; }
        public J2534TXFLAG TxFlags { get; set; }
        public uint Timestamp { get; set; }
        public uint ExtraDataIndex { get; set; }
        public byte[] Data { get; set; }

        public J2534Message()
        {
            Data = Array.Empty<byte>();
        }

        public J2534Message(J2534PROTOCOL ProtocolID, J2534TXFLAG TxFlags, byte[] Data)
        {
            this.ProtocolID = ProtocolID;
            this.TxFlags = TxFlags;
            if (Data == null)
                this.Data = Array.Empty<byte>();
            else
                this.Data = Data;
        }
    }
}
