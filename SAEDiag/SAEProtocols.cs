using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J2534;
using System.Collections;

namespace SAE
{

    class SAEProtocols: IEnumerator, IEnumerable
    {
        private object HeaderClass;
        private int ProtocolIndex;
        private const int NumOfProtocols = 4;
        private IEnumerator enumerator;

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
            HeaderClass = new CANHeader(){
                SourceAddress = 0x7E0,
                TargetAddress = 0x7E8
            };
            this.Device = Device;
        }

  //      public SAEMessage ConstructMessenger()
       // {

//        }


        //**********IEnumerable IEnumerator***************
        public IEnumerator GetEnumerator()
        { return (IEnumerator)this; }

        public bool MoveNext()
        {
            ProtocolIndex++;
            return (ProtocolIndex < NumOfProtocols);
        }

        public void Reset()
        { ProtocolIndex = 0; }

        public object Current
        { get { return this[ProtocolIndex]; } }
        //**********IEnumerable IEnumerator***************

        public Channel this[int Protocol]
        {
            get
            {
                switch((Protocols)Protocol)
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
            C.StartMsgFilter(new MessageFilter(COMMONFILTER.STANDARDISO15765, new byte[] { 0x00, 0x00, 0x07, 0xE0 }));
            C.StartMsgFilter(new MessageFilter(COMMONFILTER.STANDARDISO15765, new byte[] { 0x00, 0x00, 0x07, 0xE1 }));
            C.SetConfig(J2534PARAMETER.LOOP_BACK, 0);

            HeaderClass = new CANHeader()
            {
                SourceAddress = 0x7E0,
                TargetAddress = 0x7E8
            };
            return C;
        }

        public Channel J1850PWM(J2534PhysicalDevice Device)
        {
            Channel C = Device.ConstructChannel(J2534PROTOCOL.J1850PWM, J2534BAUD.J1850PWM, J2534CONNECTFLAG.NONE);
            C.SetConfig(J2534PARAMETER.NODE_ADDRESS, 0xF1);
            C.StartMsgFilter(new MessageFilter()
            {
                Mask = new byte[] { 0x1C, 0xFF, 0xFF },
                Pattern = new byte[] { 0x04, 0xF1, 0x10 },
                FilterType = J2534FILTER.PASS_FILTER
            });

            //C.StartMsgFilter(new J2534DotNet.MessageFilter(COMMONFILTER.PASS, new List<byte> { 0x04, 0xF1, 0x10 }));

            HeaderClass = new J1850Header() {
                ToolAddress = 0xF1,
                TargetAddress = 0x10,
                Priority = 0,
                SingleByteHeader = false,
                IFRNotAllowed = false,
                PhysicalAddressing = true,
                MessageType = 0
            };

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
