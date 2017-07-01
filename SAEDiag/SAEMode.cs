using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAE
{
    public class SAEModeData
    {
        private int mode_int;

        public SAEModeData() { }

        public SAEModeData(SAEModes ModeEnum)
        {
            Mode = ModeEnum;
        }

        public SAEModes Mode
        {
            get
            {
                return (SAEModes)mode_int;
            }
            set
            {
                //SAEMode = Enum.IsDefined(typeof(SAEModeDefinitions), value[0]) ? (SAEModeDefinitions)value[0] : SAEModeDefinitions.UNKNOWN_MODE;
                mode_int = (int)value;
            }
        }

        public SAEModes BaseMode
        {
            get
            {
                return (SAEModes)(mode_int & 0x3F);
            }
            set
            {
                mode_int = (mode_int & 0x40) | ((int)value & 0x3F);
            }
        }

        public bool IsResponse
        {
            get
            {
                return (mode_int & 0x40) == 0x40 ? true : false;
            }
            set
            {
                if (value)
                    mode_int |= 0x40;
                else
                    mode_int &= 0xBF;
            }
        }

        public bool IsJ1979
        {
            get
            {
                return ((int)BaseMode < 0x10) ? true : false;
            }
        }

        public static implicit operator SAEModeData(byte ModeByte)
        {
            return new SAEModeData() { mode_int = (int)ModeByte };
        }

        public static implicit operator byte(SAEModeData Mode)
        {
            return (byte)Mode.Mode;
        }

        public static implicit operator SAEModes(SAEModeData Mode)
        {
            return Mode.Mode;
        }

        public static implicit operator SAEModeData(SAEModes ModeEnum)
        {
            return new SAEModeData(ModeEnum);
        }
    }

    //public enum J1979Modes
    //{
    //    REQ_DIAG_DATA = 0x01,
    //    REQ_FREEZE_FRAME_DATA = 0x02,
    //    REQ_EMISSION_DIAG_DATA = 0x03,
    //    CLEAR_EMISSION_DIAG_DATA = 0x04,
    //    REQ_O2_MON_RESULTS = 0x05,
    //    REQ_SYSTEM_MON_RESULTS = 0x06,
    //    REQ_CURRENT_DTC = 0x07,
    //    REQ_SYSTEM_CTL = 0x08,
    //    REQ_VEHICLE_INFO = 0x09,
    //    REQ_PERMANENT_EMISSION_DTC = 0x0A,
    //    REQ_DIAG_DATA_RESPONSE = 0x41,
    //    REQ_FREEZE_FRAME_DATA_RESPONSE = 0x42,
    //    REQ_EMISSION_DIAG_DATA_RESPONSE = 0x43,
    //    CLEAR_EMISSION_DIAG_DATA_RESPONSE = 0x44,
    //    REQ_O2_MON_RESULTS_RESPONSE = 0x45,
    //    REQ_SYSTEM_MON_RESULTS_RESPONSE = 0x46,
    //    REQ_CURRENT_DTC_RESPONSE = 0x47,
    //    REQ_SYSTEM_CTL_RESPONSE = 0x48,
    //    REQ_VEHICLE_INFO_RESPONSE = 0x49,
    //    REQ_PERMANENT_EMISSION_DTC_RESPONSE = 0x4A
    //}

    public enum SAEModes:byte
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
        REQ_PERMANENT_EMISSION_DTC = 0x0A,
        INITIATE_DIAG_OP = 0x10,
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
        REQ_SECURITY_ACCESS = 0x27,
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

        REQ_DIAG_DATA_RESPONSE = 0x41,
        REQ_FREEZE_FRAME_DATA_RESPONSE = 0x42,
        REQ_EMISSION_DIAG_DATA_RESPONSE = 0x43,
        CLEAR_EMISSION_DIAG_DATA_RESPONSE = 0x44,
        REQ_O2_MON_RESULTS_RESPONSE = 0x45,
        REQ_SYSTEM_MON_RESULTS_RESPONSE = 0x46,
        REQ_CURRENT_DTC_RESPONSE = 0x47,
        REQ_SYSTEM_CTL_RESPONSE = 0x48,
        REQ_VEHICLE_INFO_RESPONSE = 0x49,
        REQ_PERMANENT_EMISSION_DTC_RESPONSE = 0x4A,
        INITIATE_DIAG_OP_RESPONSE = 0x50,
        MODULE_RESET_RESPONSE = 0x51,
        REQ_FREEZE_FRAME_RESPONSE = 0x52,
        REQ_DTC_RESPONSE = 0x53,
        CLEAR_DIAG_INFO_RESPONSE = 0x54,
        DTC_STATUS_RESPONSE = 0x57,
        REQ_DTC_BY_STATUS_RESPONSE = 0x58,
        RESUME_NORMAL_OPERATION_RESPONSE = 0x60,
        DATA_BY_OFFSET_RESPONSE = 0x61,
        DATA_BY_PID_RESPONSE = 0x62,
        DATA_BY_ADDRESS_RESPONSE = 0x63,
        REQ_PID_SCALING_RESPONSE = 0x64,
        STOP_DATA_TX_RESPONSE = 0x65,
        SET_DATA_RATE_RESPONSE = 0x66,
        REQ_SECURITY_ACCESS_RESPONSE = 0x67,
        STOP_NORMAL_TRAFFIC_RESPONSE = 0x68,
        RESUME_NORMAL_TRAFFIC_RESPONSE = 0x69,
        REQ_DATA_PACKET_RESPONSE = 0x6A,
        DEFINE_PACKET_BY_OFFSET_RESPONSE = 0x6B,
        DEFINE_PACKET_RESPONSE = 0x6C,
        IOCTL_BY_PID_RESPONSE = 0x6F,
        IOCTL_BY_VALUE_RESPONSE = 0x70,
        START_DIAG_ROUTINE_BY_NUMBER_RESPONSE = 0x71,
        STOP_DIAG_ROUTINE_BY_NUMBER_RESPONSE = 0x72,
        DIAG_ROUTINE_RESULTS_BY_NUMBER_RESPONSE = 0x73,
        REQ_DOWNLOAD_RESPONSE = 0x74,    //tool to module
        REQ_UPLOAD_RESPONSE = 0x75,  //module to tool
        DATA_TRANSFER_RESPONSE = 0x76,
        TRANSFER_ROUTINE_EXIT_RESPONSE = 0x77,
        START_DIAG_ROUTINE_BY_ADDRESS_RESPONSE = 0x78,
        STOP_DIAG_ROUTINE_BY_ADDRESS_RESPONSE = 0x79,
        DIAG_ROUTINE_RESULTS_BY_ADDRESS_RESPONSE = 0x7A,
        WRITE_DATA_BLOCK_RESPONSE = 0x7B,
        READ_DATA_BLOCK_RESPONSE = 0x7C,

        GENERAL_RESPONSE = 0x7F,
        UNKNOWN_MODE
    }
}
