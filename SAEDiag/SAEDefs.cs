using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

namespace SAEDiag
{
    /*
    public class SAE_data
    {
        public int mode;
        public int pid { get; set; }
        public byte[] data;
        public SAE_data()
        {
            mode = 0;
            pid = 0;
            data = new byte[8];
        }
    }
    */
	internal class SAE_message
	{
		static public int size = 16;
		private byte[] message_bytes = new byte[size];
		
        //implicit conversion to byte[]
		public static implicit operator byte[](SAE_message msg)
		{
			return msg.message_bytes;
		}
        //implicit conversion from byte[]
		public static implicit operator SAE_message(byte[] byte_array)
		{
            return new SAE_message(byte_array);
		}

        public byte this[int index]
        {
            get
            {
                if (0 <= index && index <= size)
                    return message_bytes[index];
                else
                    return 0;   //TODO: Throw exception for out of bounds index
            }
            set
            {
                if (0 <= index && index <= size)
                    message_bytes[index] = value;

                    //TODO: Throw exception for out of bounds index
            }
        }
		public SAE_message(int length)
		{
			size = length;
			message_bytes = new byte[size];
		}
		
		public SAE_message(byte[] msg)
		{
			size = msg.Length;
			message_bytes = msg;
		}
		
        public byte Modebyte
        {
            get
            {
                return message_bytes[0];
            }
            set
            {
                message_bytes[0] = value;
            }
        }
        public SAE_modes Mode
        {
            get
            {
                return (System.Enum.IsDefined(typeof(SAE_modes), message_bytes[0])) ? (SAE_modes)message_bytes[0] : SAE_modes.UNKNOWN_MODE;
            }
            set
            {
                message_bytes[0] = (byte)value;
            }
        }
        public byte PID
        {
            get
            {
                return message_bytes[1];
            }
            set
            {
                message_bytes[1] = value;
            }
        }
		public SAE_responses Response
		{
			get
			{
                return (System.Enum.IsDefined(typeof(SAE_responses), message_bytes[size - 1])) ? (SAE_responses)message_bytes[size - 1] : SAE_responses.MANUFACTURER_SPECIFIC;
			}
			set
			{
				message_bytes[size - 1] = (byte)value;
			}
		}		
	}
    internal enum SAE_modes
    {
        REQ_DIAG_DATA = 0x01,
        REQ_FREEZE_FRAME_DATA = 0x02,
        REQ_EMISSION_DIAG_DATA = 0x03,
        CLEAR_EMISSION_DIAG_DATA = 0x04,
        REQ_O2_MON_RESULTS = 0x05,
        REQ_SYSTEM_MON_RESULTS = 0x06,
        REQ_CURRENT_DTC = 0x07,
        REQ_SYSTEM_CTL = 0x08,
        REQ_VEHICLE_INFO = 0x09,
        INITIATE_DIAG_OP = 0x10,
        REQ_PERMANENT_EMISSION_DTC = 0x0A,
        MODULE_RESET = 0x11,
        REQ_FREEZE_FRAME = 0x12,
        REQ_DTC = 0x13,
        CLEAR_DIAG_INFO = 0x14,
        DTC_STATUS = 0x17,
        REQ_DTC_BY_STATUS = 0x18,
        NORMAL_OPERATION = 0x20,
        DATA_BY_OFFSET = 0x21,
        DATA_BY_PID = 0x22,
        DATA_BY_ADDRESS = 0x23,
        REQ_PID_SCALING = 0x24,
        STOP_DATA_TX = 0x25,
        SET_DATA_RATE = 0x26,
        SECURITY_ACCESS = 0x27,
        STOP_NORMAL_TRAFFIC = 0x28,
        RESUME_NORMAL_TRAFFIC = 0x29,
        REQ_DATA_PACKET = 0x2A,
        DEFINE_PACKET_BY_OFFSET = 0x2B,
        DEFINE_PACKET = 0x2C,
        IOCTL_BY_PID = 0x2F,
        IOCTL_BY_VALUE = 0x30,
        START_DIAG_ROUTINE_BY_NUMBER = 0x31,
        STOP_DIAG_ROUTINE_BY_NUMBER = 0x32,
        DIAG_ROUTINE_RESULTS_BY_NUMBER = 0x33,
        REQ_DOWNLOAD = 0x34,    //tool to module
        REQ_UPLOAD = 0x35,  //module to tool
        DATA_TRANSFER = 0x36,
        TRANSFER_ROUTINE_EXIT = 0x37,
        START_DIAG_ROUTINE_BY_ADDRESS = 0x38,
        STOP_DIAG_ROUTINE_BY_ADDRESS = 0x39,
        DIAG_ROUTINE_RESULTS_BY_ADDRESS = 0x3A,
        WRITE_DATA_BLOCK = 0x3B,
        READ_DATA_BLOCK = 0x3C,
        DIAG_HEARTBEAT = 0x3F,
        GENERAL_RESPONSE = 0x7F,
        UNKNOWN_MODE
    }
    [Flags]
    internal enum SAE_DTC_status
    {
        IMMATURE = 0x01,    //0 Maturing/intermittent code - insufficient data to consider as a malfunction
        OCCURING_NOW = 0x02,    //Current code - present at time of request
        OEM_FLAG1 = 0x04,   //Manufacturer specific status
        OEM_FLAG2 = 0x08,   //Manufacturer specific status
        STORED = 0x10,  //Stored trouble code
        PASSING = 0x20, //Warning lamp was previously illuminated for this code, malfunction not currently detected, code not yet erased
        PENDING = 0x40, //Warning lamp pending for this code, not illuminate but malfunction was detected
        MIL_ON = 0x80   //Warning lamp illuminated for this code
    }
	internal enum SAE_responses
	{
		AFFIRMITIVE_RESPONSE = 0x00,
		GENERAL_REJECT = 0x10,
		MODE_NOT_SUPPORTED = 0x11,
		INVALID_SUBFUNCTION = 0x12,
		REPEAT_REQUEST = 0x21,
		CONDITIONS_NOT_CORRECT = 0x22,
		ROUTINE_NOT_COMPLETE = 0x23,
		REQUEST_OUT_OF_RANGE = 0x31,
		SECURITY_ACCESS_DENIED = 0x33,
		SECURITY_ACCESS_ALLOWED = 0x34,
		INVALID_KEY = 0x35,
		EXCEED_NUMBER_OF_ATTEMPTS = 0x36,
		TIME_DELAY_NOT_EXPIRED = 0x37,
		DOWNLOAD_NOT_ACCEPTED = 0x40,
		IMPROPER_DOWNLOAD_TYPE = 0x41,
		DOWNLOAD_ADDRESS_UNAVAILABLE = 0x42,
		IMPROPER_DOWNLOAD_LENGTH = 0x43,
		READY_FOR_DOWNLOAD = 0x44,
		UPLOAD_NOT_ACCEPTED = 0x50,
		IMPROPER_UPLOAD_TYPE = 0x51,
		UPLOAD_ADDRESS_UNAVAILABLE = 0x52,
		IMPROPER_UPLOAD_LENGTH = 0x53,
		READY_FOR_UPLOAD = 0x54,
		PASS_WITH_RESULTS = 0x61,
		PASS_WITHOUT_RESULTS = 0x62,
		FAIL_WITH_RESULTS = 0x63,
		FAILT_WITHOUT_RESULTS = 0x64,
		TRANSFER_SUSPENDED = 0x71,
		TRANSFER_ABORTED = 0x72,
		TRANSFER_COMPLETE = 0x73,
		TRANSFER_ADDRESS_UNAVAILABLE = 0x74,
		IMPROPER_TRANSFER_LENGTH = 0x75,
		IMPROPER_TRANSFER_TYPE = 0x76,
		TRANSFER_CHECKSUM_FAIL = 0x77,
		TRANSFER_RECEIVED = 0x78,
		TRANSFER_BYTE_COUNT_MISMATCH = 0x79,
        MANUFACTURER_SPECIFIC
    }
	
}
