using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J2534;
using System.Collections;

namespace SAE
{

    public class SAEProtocols: IEnumerator, IEnumerable
    {
        private J2534PhysicalDevice device;
        private const int num_of_protocols = 4; //This should be updated to test what protocols are supported
                                                //and only enumerate those that are.
        private int protocol_index;

        private enum Protocols
        {
            ISO15765,
            J1850PWM,
            J1850VPW,
            ISO9141
        }

        public SAEProtocols(J2534PhysicalDevice Device)
        {
            protocol_index = -1;
            this.device = Device;
        }


        //**********IEnumerable IEnumerator***************
        public IEnumerator GetEnumerator()
        { return (IEnumerator)this; }

        public bool MoveNext()
        {
            protocol_index ++;
            return (protocol_index < num_of_protocols);
        }

        public void Reset()
        { protocol_index = -1; }

        public object Current
        { get { return this[protocol_index]; } }
        //**********IEnumerable IEnumerator***************

        public Channel this[int Protocol]
        {
            get
            {
                switch((Protocols)Protocol)
                {
                    case Protocols.ISO15765:
                        return device.ConstructChannel(J2534PROTOCOL.ISO15765, J2534BAUD.ISO15765, J2534CONNECTFLAG.NONE);
                    case Protocols.J1850PWM:
                        return device.ConstructChannel(J2534PROTOCOL.J1850PWM, J2534BAUD.J1850PWM, J2534CONNECTFLAG.NONE);
                    case Protocols.J1850VPW:
                        return device.ConstructChannel(J2534PROTOCOL.J1850VPW, J2534BAUD.J1850VPW, J2534CONNECTFLAG.NONE);
                    case Protocols.ISO9141:
                        //Should I include the 5 baud init here?
                        return device.ConstructChannel(J2534PROTOCOL.ISO9141, J2534BAUD.ISO9141, J2534CONNECTFLAG.NONE);
                }
                return null;
            }
        }

        //public Channel ISO15765(J2534PhysicalDevice Device)
        //{
        //    Channel C = Device.ConstructChannel(J2534PROTOCOL.ISO15765, J2534BAUD.ISO15765, J2534CONNECTFLAG.NONE);
        //    C.StartMsgFilter(new MessageFilter(COMMONFILTER.STANDARDISO15765, new byte[] { 0x00, 0x00, 0x07, 0xE0 }));
        //    C.StartMsgFilter(new MessageFilter(COMMONFILTER.STANDARDISO15765, new byte[] { 0x00, 0x00, 0x07, 0xE1 }));
        //    C.SetConfig(J2534PARAMETER.LOOP_BACK, 0);

        //    HeaderClass = new CANHeader()
        //    {
        //        SourceAddress = 0x7E0,
        //        TargetAddress = 0x7E8
        //    };
        //    return C;
        //}

        //public Channel J1850PWM(J2534PhysicalDevice Device)
        //{
        //    Channel C = device.ConstructChannel(J2534PROTOCOL.J1850PWM, J2534BAUD.J1850PWM, J2534CONNECTFLAG.NONE);
        //    C.SetConfig(J2534PARAMETER.NODE_ADDRESS, 0xF1);
        //    C.AddToFunctMsgLookupTable(0x6B);
        //    C.StartMsgFilter(new MessageFilter()
        //    {
        //        Mask = new byte[] { 0x00, 0xFF, 0x00 },
        //        Pattern = new byte[] { 0x00, 0x6B, 0x00 },
        //        FilterType = J2534FILTER.PASS_FILTER
        //    });
        //    C.StartMsgFilter(new MessageFilter()
        //    {
        //        Mask = new byte[] { 0x1C, 0xFF, 0xFF },
        //        Pattern = new byte[] { 0x04, 0xF1, 0x10 },
        //        FilterType = J2534FILTER.PASS_FILTER
        //    });


        //    HeaderClass = new J1850Header() {
        //        ToolAddress = 0xF1,
        //        TargetAddress = 0x10,
        //        Priority = 0,
        //        SingleByteHeader = false,
        //        IFRNotAllowed = false,
        //        PhysicalAddressing = true,
        //        MessageType = 0
        //    };

        //    return C;
        //}

    //    public Channel J1850VPW(J2534PhysicalDevice Device)
    //    {
    //        Channel C = Device.ConstructChannel(J2534PROTOCOL.J1850VPW, J2534BAUD.J1850VPW, J2534CONNECTFLAG.NONE);
    //        return C;
    //    }

    //    public Channel ISO9141(J2534PhysicalDevice Device)
    //    {
    //        Channel C = Device.ConstructChannel(J2534PROTOCOL.ISO9141, J2534BAUD.ISO9141, J2534CONNECTFLAG.NONE);
    //        return C;
    //    }
    }
}
