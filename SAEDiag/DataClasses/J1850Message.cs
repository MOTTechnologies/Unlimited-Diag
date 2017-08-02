using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAE
{
    class J1850Message
    {
        public enum OBD_PARSE_ERROR
        {
            NO_ERROR,
            J1850_MESSAGE_INSUFFICIENT_BYTES,
            J1979_MESSAGE_INSUFFICIENT_BYTES
        }

        private byte[] raw_message;
        private int source_address;
        private int target_address;
        private bool functional_addressing;
        private bool physical_addressing;
        public int J1850_priority { get; set; }
        public bool J1850_h_bit { get; set; }   //1 = single byte header
        public bool J1850_k_bit { get; set; }   //1 = IFR not allowed
        public bool J1850_y_bit { get; set; }   //1 = Physical addressing
        public int J1850_zz { get; set; }  //message type/single byte header address
        public SAEModeData SAEMode;
        public int RxDataIndex { get; set; }
        public byte[] Data { get; set; }
        public SAE_responses ResponseByte { get; private set; }
        public OBD_PARSE_ERROR Error_Code { get; private set; }
        public bool IsValid { get; private set; }

        public J1850Message()
        {
            Data = Array.Empty<byte>();
            SAEMode = new SAEModeData();
            raw_message = RawMessage;
            IsValid = true;
        }

        public J1850Message(byte[] RawMessage)
        {
            Data = Array.Empty<byte>();
            SAEMode = new SAEModeData();
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
                byte0 |= (byte)(J1850_zz & 0x03);
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
            }
        }

        public int TargetAddress
        {
            get { return target_address; }
            set
            {
                target_address = value;
            }
        }

        public bool FunctionalAddressing
        {
            get { return functional_addressing; }
            set
            {
                J1850_y_bit = physical_addressing = !(functional_addressing = value);
            }
        }

        public bool PhysicalAddressing
        {
            get { return physical_addressing; }
            set
            {
                functional_addressing = !(J1850_y_bit = physical_addressing = value);
            }
        }

        private void parse_raw_message()
        {
            if (raw_message.Length < 2)
            {
                IsValid = false;
                Error_Code = OBD_PARSE_ERROR.J1850_MESSAGE_INSUFFICIENT_BYTES;
            }
            else
            {
                J1850_byte0 = raw_message[0];

                if (!J1850_h_bit && (raw_message.Length < 4))
                {
                    IsValid = false;
                    Error_Code = OBD_PARSE_ERROR.J1850_MESSAGE_INSUFFICIENT_BYTES;
                }
                else if (J1850_h_bit)    //Single byte header
                {
                    target_address = J1850_zz;  //Need to review J2178-1 to verify this
                    sae_message = raw_message.Skip(1).ToArray();
                }
                else
                {
                    target_address = raw_message[1];
                    source_address = raw_message[2];
                    sae_message = raw_message.Skip(3).ToArray();
                }
            }
        }

        private byte[] header
        {
            get
            {
                byte[] header_array = Array.Empty<byte>();
                if (J1850_h_bit)    //Single byte header_array
                {
                    header_array = new byte[1] { J1850_byte0 };
                }
                else//3 byte header_array
                {
                    header_array = new byte[3] { J1850_byte0, (byte)target_address, (byte)source_address };
                }
                return header_array;
            }
        }

        private byte[] sae_message
        {
            get
            {
                return (new byte[1] { SAEMode }).Concat(Data).ToArray();
            }
            set
            {
                if (value.Length == 0)
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
                            ResponseByte = SAE_responses.NONE;
                            Data = Array.Empty<byte>();
                            break;
                        case 2:
                            Data = Array.Empty<byte>();
                            ResponseByte = Enum.IsDefined(typeof(SAE_responses), (int)value.Last()) ? (SAE_responses)value.Last() : SAE_responses.MANUFACTURER_SPECIFIC;
                            break;
                        default:
                            Data = value.Skip(1).Take(value.Length - 2).ToArray();
                            ResponseByte = Enum.IsDefined(typeof(SAE_responses), (int)value.Last()) ? (SAE_responses)value.Last() : SAE_responses.MANUFACTURER_SPECIFIC;
                            break;
                    }
                }
                else
                {
                    ResponseByte = Enum.IsDefined(typeof(SAE_responses), value.Last()) ? (SAE_responses)value.Last() : SAE_responses.MANUFACTURER_SPECIFIC;
                    Data = value.Skip(1).ToArray();
                }
            }
        }

        public J1850Message Clone()
        {
            return new J1850Message(RawMessage);
        }
        public static implicit operator byte[] (J1850Message Message)
        {
            return Message.RawMessage;
        }

        public Predicate<J2534.J2534Message> DefaultRxComparer
        {
            get
            {
                if (J1850_h_bit)    //single byte header
                {
                    return (TestMessage =>
                                           {
                                               //TODO: Build the single byte comparer function
                                               return false;
                                           });
                }
                else    //three byte header
                {
                    return (TestMessage =>
                                           {
                                               if (TestMessage.Data.Length > 3 &&
                                                   TestMessage.Data[1] == SourceAddress &&
                                                   TestMessage.Data[2] == TargetAddress &&
                                                   TestMessage.RxStatus == J2534.J2534RXFLAG.NONE)
                                               {
                                                   if (TestMessage.Data[3] == (byte)SAEMode.ResponseMode)
                                                       return true;
                                                   else if (TestMessage.Data[3] == (byte)SAEModes.GENERAL_RESPONSE &&
                                                           TestMessage.Data.Length > 4 &&
                                                           TestMessage.Data[4] == (byte)SAEMode)
                                                       return true;
                                                   else
                                                       return false;
                                               }
                                               else
                                                   return false;
                                           });
                }
            }
        }
    }
}
