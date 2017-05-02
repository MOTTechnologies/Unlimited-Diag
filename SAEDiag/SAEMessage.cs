using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAE
{
    public class SAEMessage
    {
        public  List<byte> tx_header;   //outbound header
        public  List<byte> rx_header;   //inbound header
        private List<byte> tx_message;  //outbound raw message including header
        public List<byte> rx_message;  //inbound raw message including header
        private List<byte> tx_data;     //inner message data to be sent
        private List<byte> rx_data;     //inner message data received
        private byte mode_byte;
        private byte pid_byte;
        private byte response_byte;
        private bool UsePIDByte;
        public bool IsValidResponse;
        //public bool ReceiveComplete;
        public bool Failure;
        public string Response;

        //implicit conversion to byte[]
        //public static implicit operator byte[] (SAEMessage sae_msg)
        //{
        //    return sae_msg.RawMessage;
        //}
        //implicit conversion from byte[]
        //public static implicit operator SAEMessage(byte[] byte_array)
        //{
        //    SAEMessage msg = new SAEMessage();
        //    msg.RawMessage = byte_array;
        //    return msg;
        //}
        public SAEMessage(List<byte> TxHeader, List<byte> RxHeader)
        {
            tx_header = TxHeader;
            rx_header = RxHeader;
            tx_data = new List<byte>();
            rx_data = new List<byte>();
        }
        public bool Receive(List<byte> Receive_Message)
        {
            Failure = true;
            IsValidResponse = false;
            if (Receive_Message == null)
                return IsValidResponse;

            //if message is possibly long enough
            if (Receive_Message.Count > rx_header.Count)
            {
                //do a byte comparison of the header in reverse because its likely to find non-matches quicker
                for(int i = rx_header.Count - 1;i > -1;i--)
                {
                    if(Receive_Message[i] != rx_header[i])
                        return IsValidResponse; //Not valid
                }
                //check for a success message matching the sent mode byte
                if(Receive_Message[rx_header.Count] == mode_byte + 0x40)
                {
                    IsValidResponse = true;
                    Failure = false;
                    rx_message = Receive_Message;
                    return IsValidResponse; //Is valid
                }

                if (Receive_Message[rx_header.Count] != (byte)SAEModes.GENERAL_RESPONSE)
                    return IsValidResponse; //Not valid

                //This is the most likely path at this point
                if(Receive_Message.Count > rx_header.Count + 2)
                {
                    //if its a general response that is not for us
                    if (Receive_Message[rx_header.Count + 1] != mode_byte)
                        return IsValidResponse;
                    ResponseByte = Receive_Message.Last();
                    IsValidResponse = true;
                    rx_message = Receive_Message;
                    return IsValidResponse;
                }

                //Unusual case that likely will never happen, debateable wether its valid
                else if (Receive_Message.Count == rx_header.Count + 2)
                {
                    //This response is not specifically for the sent message, but is valid
                    ResponseByte = Receive_Message.Last();
                    IsValidResponse = true;
                    rx_message = Receive_Message;
                    return IsValidResponse;
                }
            }
            return IsValidResponse;
        }

        //private void StripResponseByteFromData(byte[] )
        public List<byte> Send(SAEModes _Mode)
        {
            return Send(_Mode, 0xFF, null);
        }

        public List<byte> Send(SAEModes _Mode, byte _PID)
        {
            return Send(_Mode, _PID, null);
        }

        public List<byte> Send(SAEModes _Mode, byte _PID, List<byte> Data)
        {
            Mode = _Mode;
            PID = _PID;
            if (Data == null)
                tx_data.Clear();
            else
                tx_data = Data;

            List<byte> send_msg = new List<byte>();
            send_msg.AddRange(tx_header);
            send_msg.Add(Modebyte);
            if (UsePIDByte)
                send_msg.Add(PID);
            if(tx_data.Count > 0)
                send_msg.AddRange(Data);
            return send_msg;
        }
        //public SAEMessage(SAEModes mode, byte PID)
        //{
        //    Failure = true;
        //}

        //public SAEMessage(SAEModes mode) : this(mode, 0xFF) { }
        //public SAEMessage() : this(SAEModes.UNKNOWN_MODE, 0xFF) { }

        //public byte this[int index]
        //{
        //    get
        //    {
        //        return RawMessage[index];
        //    }
        //    set
        //    {
        //        //raw_message[index] = value;
        //    }
        //}

        //public byte[] RawMessage
        //{
        //    get
        //    {
        //        List<byte> raw = new List<byte>();
        //        raw.AddRange(tx_header);
        //        raw.Add(mode_byte);
        //        if (UsePIDByte)
        //            raw.Add(PID_byte);
        //        raw.AddRange(tx_message);
        //        return raw.ToArray();
        //    }
        //    set
        //    {

        //        //if(value.Length > rx_header.Count)
        //        //{

        //        //    if((value[0] == (int)SAEModes.GENERAL_RESPONSE) ||
        //        //       (value[0] == (mode_byte + 0x40)))
        //        //    {
        //        //        IsValidResponse = true;
        //        //    }
        //        //    else
        //        //    {
        //        //        IsValidResponse = false;
        //        //    }
        //        //}
        //    }   //set
        //}   //property

        //public byte[] MessageData
        //{
        //    get
        //    {
        //        return;
        //    }
        //    set
        //    {
        //        MessageData.
        //        message_data = new byte[value.Length];
        //        message_data = (byte[])value.Clone();
        //    }
        //}
        public byte Modebyte
        {
            get
            {
                return mode_byte; ;
            }
            set
            {
                mode_byte = value;
            }
        }

        public SAEModes Mode
        {
            get
            {
                return (System.Enum.IsDefined(typeof(SAEModes), mode_byte)) ? (SAEModes)mode_byte : SAEModes.UNKNOWN_MODE;
            }
            set
            {
                mode_byte = (byte)value;
            }
        }

        public byte PID
        {
            get
            {
                return pid_byte;
            }
            set
            {
                pid_byte = value;
                if (pid_byte == 0xFF)   //I *think* this is a safe flag to indicate the PIDbyte is not needed.
                    UsePIDByte = false;
                else
                    UsePIDByte = true;
            }
        }

        public byte ResponseByte
        {
            get
            {
                return response_byte;
            }
            set
            {
                response_byte = value;
                if (System.Enum.IsDefined(typeof(SuccessResponse), response_byte))
                    Failure = false;
                else
                    Failure = true;
            }
        }

        //public SAE_responses Response
        //{
        //    get
        //    {
        //        return (System.Enum.IsDefined(typeof(SAE_responses), response_byte)) ? (SAE_responses)response_byte : SAE_responses.MANUFACTURER_SPECIFIC;
        //    }
        //    set
        //    {
        //        response_byte = (byte)value;
        //    }
        //}
    }
}
