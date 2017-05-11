using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAE
{
    public interface ISAEHeader
    {
        List<byte> BareRxMessage { get; }
        bool CheckRxHeader(List<byte> MessageToTest);
        List<byte> AppendTxHeader(List<byte> RawMessage);
    }

    class CANHeader:ISAEHeader
    {
        public bool Is29Bit;
        public List<byte> BareRxMessage { get; private set; }
        private byte Src0;
        private byte Src1;
        private byte Src2;
        private byte Src3;

        private byte Tgt0;
        private byte Tgt1;
        private byte Tgt2;
        private byte Tgt3;

        public int SourceAddress
        {
            get
            {
                return (Src3 << 24) + (Src2 << 16) + (Src1 << 8) + (Src0);
            }
            set
            {
                Src0 = (byte)value;
                Src1 = (byte)(value >> 8);
                Src2 = (byte)(value >> 16);
                Src3 = (byte)(value >> 24);
            }
        }

        public int TargetAddress
        {
            get
            {
                return (Tgt3 << 24) + (Tgt2 << 16) + (Tgt1 << 8) + (Tgt0);
            }
            set
            {
                Tgt0 = (byte)value;
                Tgt1 = (byte)(value >> 8);
                Tgt2 = (byte)(value >> 16);
                Tgt3 = (byte)(value >> 24);
            }
        }

        public bool CheckRxHeader(List<byte> MessageToTest)
        {
            if (MessageToTest.Count < 4)
                return false;
            if (Is29Bit)
            {
                if (MessageToTest[3] != Src0)
                    return false;
                if (MessageToTest[2] != Src1)
                    return false;
                if (MessageToTest[1] != Src2)
                    return false;
                if (MessageToTest[0] != Src3)
                    return false;
                BareRxMessage = MessageToTest.Skip(4).ToList();
                return true;
            }
            if (MessageToTest[3] != Src0)
                return false;
            if (MessageToTest[2] != Src1)
                return false;
            BareRxMessage = MessageToTest.Skip(4).ToList();
            return true;
        }

        public List<byte> AppendTxHeader(List<byte> BareMessage)
        {
            BareMessage.Insert(0, Tgt3);
            BareMessage.Insert(1, Tgt2);
            BareMessage.Insert(2, Tgt1);
            BareMessage.Insert(3, Tgt0);
            return BareMessage;
        }

    }

    class J1850Header:ISAEHeader
    {
        public int HeaderLength { get; private set; }
        public byte TargetAddress;  //Address of the downstream device
        public byte ToolAddress;    //Address of this tool
        public byte FieldByte;
        public byte Priority;       //0-7 lower number is greator priority
        private bool _SingleByteHeader;
        public bool IFRNotAllowed;
        public bool PhysicalAddressing;
        public byte MessageType;
        public List<byte> BareRxMessage { get; private set; }

        public bool SingleByteHeader
        {
            get
            {
                return _SingleByteHeader;
            }
            set
            {
                _SingleByteHeader = value;
                if (_SingleByteHeader)
                    HeaderLength = 1;
                else
                    HeaderLength = 3;
            }
        }

        private void ParseField(byte PCIByte)
        {
            Priority = (byte)(PCIByte >> 5);
            SingleByteHeader = (((PCIByte >> 4) & 1) == 1 ? true : false);
            IFRNotAllowed = (((PCIByte >> 3) & 1) == 1 ? true : false);
            PhysicalAddressing = (((PCIByte >> 2) & 1) == 1 ? true : false);
            MessageType = (byte)(PCIByte & 0x03);
        }

        private byte EncodeField()
        {
            byte FieldByte = 0;
            FieldByte |= (byte)(Priority << 5);
            if (SingleByteHeader)
                FieldByte |= 0x10;
            if (IFRNotAllowed)
                FieldByte |= 0x08;
            if (PhysicalAddressing)
                FieldByte |= 0x04;
            FieldByte |= (byte)(MessageType & 0x03);
            return FieldByte;
        }

        public bool CheckRxHeader(List<byte> MessageToTest)
        {
            if (MessageToTest.Count < HeaderLength)
                return false;
            if (!SingleByteHeader)
            {
                if (MessageToTest[1] != ToolAddress)
                    return false;
                if (MessageToTest[2] != TargetAddress)
                    return false;
                if ((MessageToTest[0] & 0x1C) != (FieldByte & 0x1C))
                    return false;
                BareRxMessage = MessageToTest.Skip(3).ToList();
                return true;
            }
            //I dont know about J1850 single byte header messages.
            return false;
        }

        public List<byte> AppendTxHeader(List<byte> BareMessage)
        {
            if (!SingleByteHeader)
            {
                BareMessage.Insert(0, FieldByte);
                BareMessage.Insert(1, TargetAddress);
                BareMessage.Insert(2, ToolAddress);
            }
            else
                BareMessage.Insert(0, FieldByte);
            return BareMessage;
        }
    }

}
