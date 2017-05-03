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
            C.SetConfig(new J2534DotNet.SConfig(J2534PARAMETER.LOOP_BACK, 0));
            return C;
        }

        public Channel J1850PWM(J2534PhysicalDevice Device)
        {
            Channel C = Device.ConstructChannel(J2534PROTOCOL.J1850PWM, J2534BAUD.J1850PWM, J2534CONNECTFLAG.NONE);
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
