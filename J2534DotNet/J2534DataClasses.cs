using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J2534DotNet
{
    public class J2534Message
    {
        public J2534Message()
        {
            Data = new List<byte>();
        }
        public J2534Message(J2534PROTOCOL ProtocolID, J2534TXFLAG TxFlags, List<byte> Data)
        {
            this.ProtocolID = ProtocolID;
            this.TxFlags = TxFlags;
            this.Data = Data;
        }
        public J2534PROTOCOL ProtocolID { get; set; }
        public J2534RXFLAG RxFlags { get; set; }
        public J2534TXFLAG TxFlags { get; set; }
        public uint Timestamp { get; set; }
        public uint ExtraDataIndex { get; set; }
        private List<byte> data;
        public List<byte> Data
        {
            get
            {
                return data;
            }
            set
            {
                if (value.Count < 4129)
                    data = value;
                else
                    throw new ArgumentException("Message data length greater than 4128");
            }
        }
    }

    public class PeriodicMsg
    {
        public J2534Message Message { get; set; }
        public int Interval { get; set; }
        internal int MessageID { get; set; }
    }

    public class MessageFilter
    {
        public J2534FILTER FilterType;
        public List<byte> Mask;
        public List<byte> Pattern;
        public List<byte> FlowControl;
        public J2534TXFLAG TxFlags;
        public int FilterId;

        public MessageFilter()
        {
            Mask = new List<byte>();
            Pattern = new List<byte>();
            FlowControl = new List<byte>();
            TxFlags = J2534TXFLAG.NONE;
        }

        public MessageFilter(COMMONFILTER FilterType, List<byte> Match)
        {
            Mask = new List<byte>();
            Pattern = new List<byte>();
            FlowControl = new List<byte>();
            TxFlags = J2534TXFLAG.NONE;

            switch (FilterType)
            {
                case COMMONFILTER.PASSALL:
                    PassAll();
                    break;
                case COMMONFILTER.PASS:
                    Pass(Match);
                    break;
                case COMMONFILTER.BLOCK:
                    Block(Match);
                    break;
                case COMMONFILTER.STANDARDISO15765:
                    StandardISO15765(Match);
                    break;
                case COMMONFILTER.NONE:
                    break;
            }

        }

        public void Clear()
        {
            Mask.Clear();
            Pattern.Clear();
            FlowControl.Clear();
        }

        public void PassAll()
        {
            Clear();
            Mask.Add(0x00);
            Pattern.Add(0x00);
            FilterType = J2534FILTER.PASS_FILTER;
        }

        public void Pass(List<byte> Match)
        {
            ExactMatch(Match);
            FilterType = J2534FILTER.PASS_FILTER;
        }

        public void Block(List<byte> Match)
        {
            ExactMatch(Match);
            FilterType = J2534FILTER.BLOCK_FILTER;
        }

        private void ExactMatch(List<byte> Match)
        {
            Clear();
            Mask = Enumerable.Repeat((byte)0xFF, Match.Count).ToList();
            Pattern = Match;
        }
        public void StandardISO15765(List<byte> SourceAddress)
        {
            //Should throw exception??
            if (SourceAddress.Count != 4)
                return;
            Clear();
            Mask.Add(0xFF);
            Mask.Add(0xFF);
            Mask.Add(0xFF);
            Mask.Add(0xFF);

            Pattern.AddRange(SourceAddress);
            Pattern[3] += 0x08;

            FlowControl.AddRange(SourceAddress);

            TxFlags = J2534TXFLAG.ISO15765_FRAME_PAD;
            FilterType = J2534FILTER.FLOW_CONTROL_FILTER;
        }
    }

    public class SConfig
    {
        public J2534PARAMETER Parameter { get; set; }
        public int Value { get; set; }
        public SConfig(J2534PARAMETER Parameter, int Value)
        {
            this.Parameter = Parameter;
            this.Value = Value;
        }
    }
}
