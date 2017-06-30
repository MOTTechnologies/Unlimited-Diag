using System.Linq;

namespace J2534
{
    public class MessageFilter
    {
        public J2534FILTER FilterType;
        public byte[] Mask;
        public byte[] Pattern;
        public byte[] FlowControl;
        public J2534TXFLAG TxFlags;
        public int FilterId;

        public MessageFilter()
        {
            TxFlags = J2534TXFLAG.NONE;
        }

        public MessageFilter(COMMONFILTER FilterType, byte[] Match)
        {
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

        private void Reset(int Length)
        {
            Mask = new byte[Length];
            Pattern = new byte[Length];
            FlowControl = new byte[Length];
        }

        public void PassAll()
        {
            Reset(1);
            Mask[0] = 0x00;
            Pattern[0] = 0x00;
            FilterType = J2534FILTER.PASS_FILTER;
        }

        public void Pass(byte[] Match)
        {
            ExactMatch(Match);
            FilterType = J2534FILTER.PASS_FILTER;
        }

        public void Block(byte[] Match)
        {
            ExactMatch(Match);
            FilterType = J2534FILTER.BLOCK_FILTER;
        }

        private void ExactMatch(byte[] Match)
        {
            Reset(Match.Length);
            Mask = Enumerable.Repeat((byte)0xFF, Match.Length).ToArray();
            Pattern = Match;
        }
        public void StandardISO15765(byte[] SourceAddress)
        {
            //Should throw exception??
            if (SourceAddress.Length != 4)
                return;
            Reset(4);
            Mask[0] = 0xFF;
            Mask[1] = 0xFF;
            Mask[2] = 0xFF;
            Mask[3] = 0xFF;

            Pattern = SourceAddress;
            Pattern[3] += 0x08;

            FlowControl = SourceAddress;

            TxFlags = J2534TXFLAG.ISO15765_FRAME_PAD;
            FilterType = J2534FILTER.FLOW_CONTROL_FILTER;
        }
    }
}
