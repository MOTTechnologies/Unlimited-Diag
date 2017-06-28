using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J2534;

namespace SAE
{
    public class J1979Session
    {

        private Channel session_channel;
        private int _tool_address;
        public int target_address { get; set; }
        private SAE_NETWORK network;
        private OBDMessage TxMessage;
        private OBDMessage RxMessage;

        public string Status { get; private set; }
        public List<ModuleData> OBDModuleList;

        public J1979Session(Channel SessionChannel, bool Init)
        {
            TxMessage = new OBDMessage();
            RxMessage = new OBDMessage();
            OBDModuleList = new List<ModuleData>();

            session_channel = SessionChannel;
            if(Init)
                set_default_session_parameters();
        }

        public bool Broadcast()
        {
            bool OBD_Module_Present = false;

            GetMessageResults Results = session_channel.MessageTransaction(TxMessage, 200, (msg =>
            {
                OBDMessage RxMessage = new OBDMessage(msg.Data);
                return (RxMessage.SAEMode == SAEModes.REQ_DIAG_DATA_RESPONSE && RxMessage.PID == 0x00);
            }));

            Results.Messages.ForEach(msg =>
            {
                OBDMessage RxMessage = new OBDMessage(msg.Data);
                OBD_Module_Present = true;
                if (!OBDModuleList.Where(m => m.Address == RxMessage.TargetAddress).Any())
                {
                    ModuleData Module = new ModuleData(RxMessage.TargetAddress);
                    Module.PID_Validation_Bytes(0, RxMessage.Data);
                    OBDModuleList.Add(Module);
                }
            });
            return OBD_Module_Present;
        }

        private void ValidatePIDS(ModuleData Module)
        {
            if(Module.ValidatedPIDS.Where(p => p.Number == 32).Any())
            {
                TxMessage.SAEMode = SAEModes.REQ_DIAG_DATA;
                TxMessage.PID = 0x20;

                session_channel.SendMessage(TxMessage);
                session_channel.GetMessage();
            }
        }
        private int tool_address
        {
            get { return _tool_address; }
            set
            {
                _tool_address = value;
                if (network == SAE_NETWORK.J1850)
                    session_channel.SetConfig(J2534PARAMETER.NODE_ADDRESS, (byte)_tool_address);
            }
        }
        private void set_default_session_parameters()
        {
            switch (session_channel.ProtocolID)
            {
                case J2534.J2534PROTOCOL.ISO15765:
                    network = SAE_NETWORK.ISO15765;
                    break;
                case J2534.J2534PROTOCOL.J1850PWM:
                    TxMessage.J1850_byte0 = 0x61;
                    break;
                case J2534.J2534PROTOCOL.J1850VPW:
                case J2534.J2534PROTOCOL.ISO9141:
                    TxMessage.J1850_byte0 = 0x68;
                    break;
                case J2534.J2534PROTOCOL.ISO14230:
                    network = SAE_NETWORK.J1850;
                    break;
                case J2534.J2534PROTOCOL.SCI_A_ENGINE:
                case J2534.J2534PROTOCOL.SCI_A_TRANS:
                case J2534.J2534PROTOCOL.SCI_B_ENGINE:
                case J2534.J2534PROTOCOL.SCI_B_TRANS:
                    network = SAE_NETWORK.SCI;
                    break;
            }

            TxMessage.SAEMode = SAEModes.REQ_DIAG_DATA;
            TxMessage.PID = 0x00;

            switch (network)
            {
                case SAE_NETWORK.ISO15765:
                    session_channel.ClearMsgFilters();
                    for (int i = 0;i < 8; i++)
                        session_channel.StartMsgFilter(new MessageFilter
                        (COMMONFILTER.STANDARDISO15765, new byte[4] { 0x00, 0x00, 0x07, (byte)(0xE0 + i) }));
                    session_channel.SetConfig(J2534PARAMETER.LOOP_BACK, 0);
                    TxMessage.Network = network;
                    TxMessage.TargetAddress = 0x07DF;
                    break;
                case SAE_NETWORK.J1850:
                    session_channel.ClearMsgFilters();
                    session_channel.ClearFunctMsgLookupTable();
                    session_channel.AddToFunctMsgLookupTable(0x6B);
                    session_channel.StartMsgFilter(new MessageFilter()
                    {
                        Mask = new byte[] { 0x00, 0xFF, 0x00 },
                        Pattern = new byte[] { 0x00, 0x6B, 0x00 },
                        FilterType = J2534FILTER.PASS_FILTER
                    });
                    target_address = 0x6A;
                    tool_address = 0x6B;
                    break;
                case SAE_NETWORK.SCI:
                    break;
            }
        }
    }
}
