using System;
using System.ComponentModel;

namespace J2534
{
    internal static class CONST
    {
        internal const bool SUCCESS = false;
        internal const bool FAILURE = true;
        internal const int J2534MESSAGESIZE = 4152; //Max message length in bytes
        internal const int HEAPMESSAGEBUFFERSIZE = 200; //Max number of messages that can be passed to/from the API in a single call
    }
    /// <summary>
    /// enum used to create predefined filters in the MessageFilter constructor.
    /// </summary>
    public enum COMMONFILTER
    {
        NONE,
        PASS,
        PASSALL,
        BLOCK,
        STANDARDISO15765
    }

    [Flags]
    public enum J2534RXFLAG
    {
        NONE = 0x00000000,
        TX_MSG_TYPE = 0x00000001,
        START_OF_MESSAGE = 0x00000002,
        RX_BREAK = 0x00000004,
        TX_INDICATION = 0x00000008,
        ISO15765_PADDING_ERROR = 0x00000010,
        ISO15765_EXT_ADDR = 0x00000080,
        CAN_29BIT_ID = 0x00000100
    }

    [Flags]
    public enum J2534CONNECTFLAG
    {
        NONE = 0x0000,
        CAN_29BIT_ID = 0x0100,
        ISO9141_NO_CHECKSUM = 0x0200,
        CAN_ID_BOTH = 0x0800,
        ISO9141_K_LINE_ONLY = 0x1000,
        DT_ISO9141_LISTEN_L_LINE = 0x08000000,
        SNIFF_MODE = 0x10000000,                    //Drewtech only
        ISO9141_FORD_HEADER = 0x20000000,           //Drewtech only
        ISO9141_NO_CHECKSUM_DT = 0x40000000         //Drewtech only
    }

    [Flags]
    public enum J2534TXFLAG
    {
        NONE = 0x00000000,
        SCI_TX_VOLTAGE = 0x00800000,
        SCI_MODE = 0x00400000,
        WAIT_P3_MIN_ONLY = 0x00000200,
        CAN_29BIT_ID = 0x00000100,
        ISO15765_ADDR_TYPE = 0x00000080,
        ISO15765_FRAME_PAD = 0x00000040
    }

    public enum J2534PROTOCOL
    {
        J1850VPW = 0x01,
        J1850PWM = 0x02,
        ISO9141 = 0x03,
        ISO14230 = 0x04,
        CAN = 0x05,
        ISO15765 = 0x06,
        SCI_A_ENGINE = 0x07,
        SCI_A_TRANS = 0x08,
        SCI_B_ENGINE = 0x09,
        SCI_B_TRANS = 0x0A
    }

    public enum J2534BAUD
    {
        ISO9141 = 10400,
        ISO9141_10400 = 10400,
        ISO9141_10000 = 10000,

        ISO14230 = 10400,
        ISO14230_10400 = 10400,
        ISO14230_10000 = 10000,

        J1850PWM = 41600,
        J1850PWM_41600 = 41600,
        J1850PWM_83200 = 83200,

        J1850VPW = 10400,
        J1850VPW_10400 = 10400,
        J1850VPW_41600 = 41600,

        CAN = 500000,
        CAN_125000 = 125000,
        CAN_250000 = 250000,
        CAN_500000 = 500000,

        ISO15765 = 500000,
        ISO15765_125000 = 125000,
        ISO15765_250000 = 250000,
        ISO15765_500000 = 500000,

        SCI_7812 = 7812,
        SCI_62500 = 62500
    }

    public enum J2534PIN
    {
        AUX = 0,
        PIN_6 = 6,
        PIN_9 = 9,
        PIN_11 = 11,
        PIN_12 = 12,
        PIN_13 = 13,
        PIN_14 = 14,
        PIN_15 = 15
    }

    public enum J2534FILTER
    {
        PASS_FILTER = 0x01,
        BLOCK_FILTER = 0x02,
        FLOW_CONTROL_FILTER = 0x03
    }

    enum J2534IOCTL
    {
        GET_CONFIG = 0x01,
        SET_CONFIG = 0x02,
        READ_VBATT = 0x03,
        FIVE_BAUD_INIT = 0x04,
        FAST_INIT = 0x05,
        CLEAR_TX_BUFFER = 0x07,
        CLEAR_RX_BUFFER = 0x08,
        CLEAR_PERIODIC_MSGS = 0x09,
        CLEAR_MSG_FILTERS = 0x0A,
        CLEAR_FUNCT_MSG_LOOKUP_TABLE = 0x0B,
        ADD_TO_FUNCT_MSG_LOOKUP_TABLE = 0x0C,
        DELETE_FROM_FUNCT_MSG_LOOKUP_TABLE = 0x0D,
        READ_PROG_VOLTAGE = 0x0E,
        READ_CH1_VOLTAGE = 0x10000,
        READ_CH2_VOLTAGE = 0x10001,
        READ_CH3_VOLTAGE = 0x10002,
        READ_CH4_VOLTAGE = 0x10003,
        READ_CH5_VOLTAGE = 0x10004,
        READ_CH6_VOLTAGE = 0x10005,
        READ_ANALOG_CH1 = 0x10010,
        READ_ANALOG_CH2 = 0x10011,
        READ_ANALOG_CH3 = 0x10012,
        READ_ANALOG_CH4 = 0x10013,
        READ_ANALOG_CH5 = 0x10014,
        READ_ANALOG_CH6 = 0x10015,
        READ_TIMESTAMP =	0x10100,
        DT_IOCTL_VVSTATS = 0x20000000
    }

    public enum J2534ERR
    {
        [Description("Function completed successfully.")]
        STATUS_NOERROR = 0x00,
        [Description("Function option is not supported.")]
        NOT_SUPPORTED = 0x01,
        [Description("Channel Identifier or handle was not recognized.")]
        INVALID_CHANNEL_ID = 0x02,
        [Description("Protocol identifier was not recognized.")]
        INVALID_PROTOCOL_ID = 0x03,
        [Description("NULL pointer presented as a function parameter address.")]
        NULL_PARAMETER = 0x04,
        [Description("IOCTL GET_CONFIG/SET_CONFIG parameter value is not recognized.")]
        INVALID_IOCTL_VALUE = 0x05,
        [Description("Flags bit field(s) contaln(s) an invalid value.")]
        INVALID_FLAGS = 0x06,
        [Description("Unspecified error, use PassThruGetLastError for obtaining error text string.")]
        FAILED = 0x07,
        [Description("The PassThru device is not connected to the PC.")]
        DEVICE_NOT_CONNECTED = 0x08,
        [Description("The PassThru device was unable to read the specified number of messages from the vehicle network within the specified time.")]
        TIMEOUT = 0x09,
        [Description("Message contains a min/max Length, ExtraData support or J1850PWM specific source address conflict violation.")]
        INVALID_MSG = 0x0A,
        [Description("The time interval value is outside the allowed range.")]
        INVALID_TIME_INTERVAL = 0x0B,
        [Description("The limit (ten) of filter/periodic messages has been exceed. for the protocol associated with the communications channel.")]
        EXCEEDED_LIMIT = 0x0C,
        [Description("The message identifier or handle was not recognized.")]
        INVALID_MSG_ID = 0x0D,
        [Description("The specified PassThru device is already in use.")]
        DEVICE_IN_USE = 0x0E,
        [Description("IOCTL identifier is not recognized.")]
        INVALID_IOCTL_ID = 0x0F,
        [Description("The PassThru device could not read any messages from the vehicie network.")]
        BUFFER_EMPTY = 0x10,
        [Description("The PassThru device could not queue any more transmit messages destined for the vehicle network.")]
        BUFFER_FULL = 0x11,
        [Description("The PassThru device experienced a buffer overflow and receive messages were lost.")]
        BUFFER_OVERFLOW = 0x12,
        [Description("An unknown pin number specified for the J1962 connector, or the resource is already in use.")]
        PIN_INVALID = 0x13,
        [Description("An existing communications channel is currently using the specified network protocol.")]
        CHANNEL_IN_USE = 0x14,
        [Description("The specified protocol type within the message structure is different from the protocol associated with the communications channel when it was opened.")]
        MSG_PROTOCOL_ID = 0x15,
        [Description("Filter identifier is not recognized.")]
        INVALID_FILTER_ID = 0x16,
        [Description("No ISO15765 flow control filter matches the header of the outgoing message.")]
        NO_FLOW_CONTROL = 0x17,
        [Description("An existing filter already matches this header or node identifier.")]
        NOT_UNIQUE = 0x18,
        [Description("Unable to honor requested baudrate within required tolerances.")]
        INVALID_BAUDRATE = 0x19,
        [Description("PassThru device identifier was not recognized.")]
        INVALID_DEVICE_ID = 0x1A,
        [Description("The API call was not mapped to a function in the PassThru DLL.")]
        FUNCTION_NOT_ASSIGNED = 0x7EADBEEF  //non-standard flag used by the wrapper to indicate no function assigned
    }

    public enum J2534PARAMETER  //Parameter selection values used in PassThruIoCtl->Set/Get Config
    {
        DATA_RATE = 0x01,
        LOOP_BACK = 0x03,
        NODE_ADDRESS = 0x04,
        NETWORK_LINE = 0x05,
        P1_MIN = 0x06,
        P1_MAX = 0x07,
        P2_MIN = 0x08,
        P2_MAX = 0x09,
        P3_MIN = 0x0A,
        P3_MAX = 0x0B,
        P4_MIN = 0x0C,
        P4_MAX = 0x0D,
        W0 = 0x19,
        W1 = 0x0E,
        W2 = 0x0F,
        W3 = 0x10,
        W4 = 0x11,
        W5 = 0x12,
        TIDLE = 0x13,
        TINIL = 0x14,
        TWUP = 0x15,
        PARITY = 0x16,
        BIT_SAMPLE_POINT = 0x17,
        SYNC_JUMP_WIDTH = 0x18,
        T1_MAX = 0x1A,
        T2_MAX = 0x1B,
        T3_MAX = 0x24,
        T4_MAX = 0x1C,
        T5_MAX = 0x1D,
        ISO15765_BS = 0x1E,
        ISO15765_STMIN = 0x1F,
        DATA_BITS = 0x20,
        FIVE_BAUD_MOD = 0x21,
        BS_TX = 0x22,
        STMIN_TX = 0x23,
        ISO15765_WFT_MAX = 0x25,
        ISO15765_SIMULTANEOUS = 0x10000000, /*DT*/ 
        DT_ISO15765_PAD_BYTE = 0x10000001,  /*DT*/
        CAN_MIXED_FORMAT = 0x8000,          //-2
        J1962_PINS = 0x8001,                //-2
        SW_CAN_HS_DATA_RATE = 0x8010,       //-2
        SW_CAN_SPEEDCHANGE_ENABLE = 0x8011, //-2 
        SW_CAN_RES_SWITCH = 0x8012,         //-2
        ACTIVE_CHANNELS = 0x8020,           //-2
        SAMPLE_RATE = 0x8021,               //-2
        SAMPLES_PER_READING = 0x8022,       //-2
        READINGS_PER_MSG = 0x8023,          //-2
        AVERAGING_METHOD = 0x8024,          //-2
        SAMPLE_RESOLUTION = 0x8025,         //-2
        INPUT_RANGE_LOW = 0x8026,           //-2
        INPUT_RANGE_HIGH = 0x8027,          //-2
        ADC_READINGS_PER_SECOND = 0x10000,  //Drewtech
        ADC_READINGS_PER_SAMPLE = 0x20000  //Drewtech
    }
    [Flags]
    internal enum SAE_API
    {
        //2.02 calls
        NONE = 0x00000000,
        CONNECT = 0x00000001,
        DISCONNECT = 0x00000002,
        READMSGS = 0x00000004,
        WRITEMSGS = 0x00000008,
        STARTPERIODICMSG = 0x00000010,
        STOPPERIODICMSG = 0x00000020,
        STARTMSGFILTER = 0x00000040,
        STOPMSGFILTER = 0x00000080,
        SETPROGRAMMINGVOLTAGE = 0x00000100,
        READVERSION = 0x00000200,
        GETLASTERROR = 0x00000400,
        IOCTL = 0x00000800,
        //4.04 calls
        OPEN = 0x00001000,
        CLOSE = 0x00002000,
        //5.00 calls
        SCANFORDEVICES = 0x00004000,
        GETNEXTDEVICE = 0x00008000,
        LOGICALCONNECT = 0x00010000,
        LOGICALDISCONNECT = 0x00020000,
        SELECT = 0x00040000,
        QUEUEMESSAGES = 0x00080000,
        //Signature matches
        V202_SIGNATURE = 0x0FFF,
        V404_SIGNATURE = 0x3FFF,
        V500_SIGNATURE = 0xFFFFF
    }
    public enum DREWTECH_API
    {
        NONE = 0x00,
        GETNEXTCARDAQ = 0x01,
        GETPOINTER = 0x02,
        READPCSETUP = 0x04,
        WRITEIPSETUP = 0x08,
        READIPSETUP = 0x10,
        RECOVERFIRMWARE = 0x20,
        LOADFIRMWARE = 0x40
    }
}
