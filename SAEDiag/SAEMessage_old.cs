using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAE_old
{
    public class SAEMessage
    {

        public enum OBD_PARSE_ERROR
        {
            NO_ERROR,
            CAN_MESSAGE_INSUFFICIENT_BYTES,
            J1850_MESSAGE_INSUFFICIENT_BYTES,
            J1979_MESSAGE_INSUFFICIENT_BYTES
        }

        private byte[] raw_message;
        private int source_address;
        private int target_address;
        private bool functional_addressing;
        private bool physical_addressing;
        public SAE_NETWORK Network { get; set; }
        public int J1850_priority { get; set; }
        public bool J1850_h_bit { get; set; }   //1 = single byte header
        public bool J1850_k_bit { get; set; }   //1 = IFR not allowed
        public bool J1850_y_bit { get; set; }   //1 = Physical addressing
        public int J1850_zz { get; set; }  //message type/single byte header address
        public SAEModeData SAEMode;
        public byte PID { get; set; }
        public byte[] SAEData { get; set; }
        public SAE_responses SAEResponse { get; private set; }
        public OBD_PARSE_ERROR Error_Code { get; private set; }
        public bool IsValid { get; private set; }

        public SAEMessage(J2534.J2534PROTOCOL Protocol)
        {
            SAEData = Array.Empty<byte>();
            SAEMode = new SAEModeData();
            this.Protocol = Protocol;
            raw_message = RawMessage;
            IsValid = true;
        }

        public SAEMessage(byte[] RawMessage, J2534.J2534PROTOCOL Protocol)
        {
            SAEData = Array.Empty<byte>();
            SAEMode = new SAEModeData();
            this.Protocol = Protocol;
            this.RawMessage = RawMessage;
        }

        public byte[] RawMessage
        {
            get
            {
                return header.Concat(sae_message).ToArray();
            }
            set
            {
                IsValid = true;
                Error_Code = OBD_PARSE_ERROR.NO_ERROR;
                raw_message = value;
                parse_raw_message();
            }
        }

        public byte J1850_byte0
        {
            get
            {
                byte byte0 = (byte)(J1850_priority << 5);
                byte0 |= J1850_h_bit ? (byte)0x10 : (byte)0x00;
                byte0 |= J1850_k_bit ? (byte)0x08 : (byte)0x00;
                byte0 |= J1850_y_bit ? (byte)0x04 : (byte)0x00;
                byte0 |= (byte)(J1850_zz &  0x03);
                return byte0;
            }
            set
            {
                J1850_priority = value >> 5;
                J1850_h_bit = ((value & 0x10) == 0x10) ? true : false;
                J1850_k_bit = ((value & 0x08) == 0x08) ? true : false;
                functional_addressing = !(physical_addressing = J1850_y_bit = ((value & 0x04) == 0x04) ? true : false);
                J1850_zz = (value & 0x03);
            }
        }
        public int SourceAddress
        {
            get { return source_address; }
            set
            {
                source_address = value;
                if(Network == SAE_NETWORK.ISO15765)
                {
                    if ((source_address & 0x08) == 0x08)
                        target_address = source_address & 0x7F7;
                    else
                        target_address = source_address | 0x08;
                }
            }
        }

        public int TargetAddress
        {
            get { return target_address; }
            set
            {
                target_address = value;
                if (Network == SAE_NETWORK.ISO15765)
                {
                    if ((target_address & 0x08) == 0x08)
                        source_address = target_address & 0x7F7;
                    else
                        source_address = target_address | 0x08;
                }
            }
        }

        public bool FunctionalAddressing
        {
            get { return functional_addressing; }
            set
            {
                if(Network == SAE_NETWORK.J1850)
                    J1850_y_bit = physical_addressing = !(functional_addressing = value);
            }
        }

        public bool PhysicalAddressing
        {
            get { return physical_addressing; }
            set
            {
                if (Network == SAE_NETWORK.J1850)
                    functional_addressing = !(J1850_y_bit = physical_addressing = value);
            }
        }

        //It may be worth using protocol directly vs reducing it to 'network'.
        //This will become clearer as things flesh out
        public J2534.J2534PROTOCOL Protocol
        {
            set
            {
                switch (value)
                {
                    case J2534.J2534PROTOCOL.ISO15765:
                        Network = SAE_NETWORK.ISO15765;
                        break;
                    case J2534.J2534PROTOCOL.J1850PWM:
                    case J2534.J2534PROTOCOL.J1850VPW:
                    case J2534.J2534PROTOCOL.ISO9141:
                    case J2534.J2534PROTOCOL.ISO14230:
                        Network = SAE_NETWORK.J1850;
                        break;
                    case J2534.J2534PROTOCOL.SCI_A_ENGINE:
                    case J2534.J2534PROTOCOL.SCI_A_TRANS:
                    case J2534.J2534PROTOCOL.SCI_B_ENGINE:
                    case J2534.J2534PROTOCOL.SCI_B_TRANS:
                        Network = SAE_NETWORK.SCI;
                        break;
                    default:
                        break;
                }
            }
        }

        //public bool UsePIDByte
        //{
        //    get
        //    {
        //        switch (SAEMode.BaseMode)
        //        {
        //            case SAEModes.REQ_DIAG_DATA:                //0x01                        
        //            case SAEModes.REQ_FREEZE_FRAME_DATA:        //0x02
        //                return true;
        //            case SAEModes.REQ_EMISSION_DIAG_DATA:       //0x03
        //            case SAEModes.CLEAR_EMISSION_DIAG_DATA:     //0x04
        //                return false;

        //            case SAEModes.REQ_O2_MON_RESULTS:           //0x05
        //            case SAEModes.REQ_SYSTEM_MON_RESULTS:       //0x06
        //            case SAEModes.REQ_CURRENT_DTC:              //0x07
        //            case SAEModes.REQ_SYSTEM_CTL:               //0x08
        //            case SAEModes.REQ_VEHICLE_INFO:             //0x09
        //            case SAEModes.REQ_PERMANENT_EMISSION_DTC:   //0x0A
        //            case SAEModes.INITIATE_DIAG_OP:             //0x10
        //            case SAEModes.MODULE_RESET:                 //0x11
        //            case SAEModes.REQ_FREEZE_FRAME:             //0x12
        //            case SAEModes.REQ_DTC:                      //0x13
        //            case SAEModes.CLEAR_DIAG_INFO:              //0x14
        //            case SAEModes.DTC_STATUS:                   //0x15
        //            case SAEModes.REQ_DTC_BY_STATUS:            //0x16
        //            case SAEModes.NORMAL_OPERATION:             //0x20
        //        }
        //    }
        //}

        private void parse_raw_message()
        {
            switch (Network)
            {
                case SAE_NETWORK.ISO15765:
                    if (raw_message.Length < 5)
                    {
                        IsValid = false;
                        Error_Code = OBD_PARSE_ERROR.CAN_MESSAGE_INSUFFICIENT_BYTES;
                        break;
                    }

                    TargetAddress = (raw_message[2] << 8) + raw_message[3];

                    sae_message = raw_message.Skip(4).ToArray();
                    break;
                case SAE_NETWORK.J1850:
                    if (raw_message.Length < 2)
                    {
                        IsValid = false;
                        Error_Code = OBD_PARSE_ERROR.J1850_MESSAGE_INSUFFICIENT_BYTES;
                        break;
                    }

                    J1850_byte0 = raw_message[0];

                    if (!J1850_h_bit && (raw_message.Length < 4))
                    {
                        IsValid = false;
                        Error_Code = OBD_PARSE_ERROR.J1850_MESSAGE_INSUFFICIENT_BYTES;
                        break;
                    }

                    if (J1850_h_bit)    //Single byte header
                    {
                        target_address = J1850_zz;  //Need to review J2178-1 to verify this
                        sae_message = raw_message.Skip(1).ToArray();
                        break;
                    }

                    target_address = raw_message[1];
                    source_address = raw_message[2];
                    sae_message = raw_message.Skip(3).ToArray();
                    break;
                case SAE_NETWORK.SCI:   //TODO: figure out how to parse SCI data
                    IsValid = false;
                    break;
            }
        }

        private byte[] header
        {
            get
            {
                byte[] header_array = Array.Empty<byte>();
                switch (Network)
                {
                    case SAE_NETWORK.ISO15765:
                        physical_addressing = !(functional_addressing = true);
                        header_array = new byte[4];
                        header_array[2] = (byte)(target_address >> 8);
                        header_array[3] = (byte)target_address;
                        break;
                    case SAE_NETWORK.J1850:
                        if (J1850_h_bit)    //Single byte header_array
                        {
                            header_array = new byte[1] { J1850_byte0 };
                        }
                        else//3 byte header_array
                        {
                            header_array = new byte[3] { J1850_byte0, (byte)target_address, (byte)source_address };
                        }
                        break;
                    case SAE_NETWORK.SCI:
                        IsValid = false;
                        break;
                }
                return header_array;
            }
        }

        private byte[] sae_message
        {
            get
            {
                return (new byte[1] { SAEMode }).Concat(SAEData).ToArray();
            }
            set
            {
                if(value.Length == 0)
                {
                    IsValid = false;
                    return;
                }
                SAEMode = value[0];

                if (SAEMode == SAEModes.GENERAL_RESPONSE)
                {
                    switch (value.Length)
                    {
                        case 1:
                            SAEResponse = SAE_responses.NONE;
                            SAEData = Array.Empty<byte>();
                            break;
                        case 2:
                            SAEData = Array.Empty<byte>();
                            SAEResponse = Enum.IsDefined(typeof(SAE_responses), (int)value.Last()) ? (SAE_responses)value.Last() : SAE_responses.MANUFACTURER_SPECIFIC;
                            break;
                        default:
                            SAEData = value.Skip(1).Take(value.Length - 2).ToArray();
                            SAEResponse = Enum.IsDefined(typeof(SAE_responses), (int)value.Last()) ? (SAE_responses)value.Last() : SAE_responses.MANUFACTURER_SPECIFIC;
                            break;
                    }
                }
                else
                {
                    SAEResponse = Enum.IsDefined(typeof(SAE_responses), value.Last()) ? (SAE_responses)value.Last() : SAE_responses.MANUFACTURER_SPECIFIC;
                    SAEData = value.Skip(1).ToArray();
                }
            }
        }

        public static implicit operator byte[](SAEMessage Message)
        {
            return Message.RawMessage;
        }

        public Predicate<J2534.J2534Message> DefaultRxComparer
        {
            get
            {
                return (_J2534Message =>
                {
                    SAEMessage RxMessage = new SAEMessage(_J2534Message.Data, J2534.J2534PROTOCOL.CAN) { Network = this.Network };
                    return (RxMessage.TargetAddress == SourceAddress &&
                            RxMessage.SAEMode.BaseMode == SAEMode &&
                            RxMessage.SAEMode.IsResponse);
                });
            }
        }
    }
}
