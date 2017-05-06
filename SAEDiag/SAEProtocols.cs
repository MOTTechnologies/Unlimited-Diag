using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J2534DotNet;

namespace SAEDiag
{
    class SAEProtocols
    {
        private List<byte> MessageTxHeader;
        private List<byte> MessageRxHeader;
        private List<byte> MessaseRxMask;

        [Flags]
        private enum J1850PriorityByte
        {
            PPP2 = 0x80,   //000 is high priority
            PPP1 = 0x40,
            PPP0 = 0x20,
            H_BIT = 0x10,  //0=three byte header, 1=1 byte header
            K_BIT = 0x08, //0=IFR required, 1=IFR not allowed
            Y_BIT = 0x04,   //0=functional addressing, 1=physical addressing
            ZZ1 = 0x02,
            ZZ0 = 0x01
        }

        private enum Protocols
        {
            ISO15765,
            J1850PWM,
            J1850VPW,
            ISO9141
        }

        J2534PhysicalDevice Device;
        public SAEProtocols(J2534PhysicalDevice Device)
        {
            this.Device = Device;
        }

        public Channel this[int enumeration]
        {
            get
            {
                switch((Protocols)enumeration)
                {
                    case Protocols.ISO15765:
                        return ISO15765(this.Device);
                    case Protocols.J1850PWM:
                        return J1850PWM(this.Device);
                    case Protocols.J1850VPW:
                        return J1850VPW(this.Device);
                    case Protocols.ISO9141:
                        return ISO9141(this.Device);
                }
                return null;
            }
        }
        public Channel ISO15765(J2534PhysicalDevice Device)
        {
                        Channel C = Device.ConstructChannel(J2534PROTOCOL.ISO15765, J2534BAUD.ISO15765, J2534CONNECTFLAG.NONE);
            C.StartMsgFilter(new MessageFilter(COMMONFILTER.STANDARDISO15765, new List<byte> { 0x00, 0x00, 0x07, 0xE0 }));
            C.StartMsgFilter(new MessageFilter(COMMONFILTER.STANDARDISO15765, new List<byte> { 0x00, 0x00, 0x07, 0xE1 }));
            C.SetConfig(J2534PARAMETER.LOOP_BACK, 0);

            return C;
        }

        public Channel J1850PWM(J2534PhysicalDevice Device)
        {
            Channel C = Device.ConstructChannel(J2534PROTOCOL.J1850PWM, J2534BAUD.J1850PWM, J2534CONNECTFLAG.NONE);
            C.SetConfig(J2534PARAMETER.NODE_ADDRESS, 0xF1);
            C.StartMsgFilter(new MessageFilter()
            {
                Mask = new List<byte>() { 0x1C, 0xFF, 0xFF },
                Pattern = new List<byte>() { 0x04, 0xF1, 0x10 },
                FilterType = J2534FILTER.PASS_FILTER
            });

            //C.StartMsgFilter(new J2534DotNet.MessageFilter(COMMONFILTER.PASS, new List<byte> { 0x04, 0xF1, 0x10 }));

            return C;
        }

        public Channel J1850VPW(J2534PhysicalDevice Device)
        {
            Channel C = Device.ConstructChannel(J2534PROTOCOL.J1850VPW, J2534BAUD.J1850VPW, J2534CONNECTFLAG.NONE);
            return C;
        }

        public Channel ISO9141(J2534PhysicalDevice Device)
        {
            Channel C = Device.ConstructChannel(J2534PROTOCOL.ISO9141, J2534BAUD.ISO9141, J2534CONNECTFLAG.NONE);
            return C;
        }
    }
}
