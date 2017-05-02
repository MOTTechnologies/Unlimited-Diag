#region Copyright (c) 2010, Michael Kelly
/* 
 * Copyright (c) 2010, Michael Kelly
 * michael.e.kelly@gmail.com
 * http://michael-kelly.com/
 * 
 * All rights reserved.
 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
 * Neither the name of the organization nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * 
 */
#endregion License
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
namespace J2534DotNet
{
    public class PassThruMsg
    {
        public PassThruMsg()
        {
            Data = new List<byte>();
        }
        public PassThruMsg(Protocols ProtocolID, TxFlag TxFlags, List<byte> Data)
        {
            this.ProtocolID = ProtocolID;
            this.TxFlags = TxFlags;
            this.Data = Data;
        }
		public Protocols ProtocolID {get; set;}
        public RxStatus RxStatus { get; set; }
        public TxFlag TxFlags { get; set; }
        public uint Timestamp { get; set; }
        public uint ExtraDataIndex { get; set; }
        public List<byte> Data { get; set; }
    }

    public class PeriodicMsg_Type
    {
        public PassThruMsg Msg { get; set; }
        public int Interval { get; set; }
        internal int pMsgID { get;  set; }
    }


        /// <summary>
        /// enum used to create predefined filters in the MessageFilter constructor.
        /// </summary>
    public enum CommonFilter
    {
        NONE,
        PASS,
        PASSALL,
        BLOCK,
        STANDARDISO15765
    }

    public class MessageFilter
    {
        public FilterEnum FilterType;
        public List<byte> Mask;
        public List<byte> Pattern;
        public List<byte> FlowControl;
        public TxFlag TxFlags;
        public int FilterId;

        public MessageFilter()
        {
            Mask = new List<byte>();
            Pattern = new List<byte>();
            FlowControl = new List<byte>();
            TxFlags = TxFlag.NONE;
        }

        public MessageFilter(CommonFilter FilterType, List<byte> Match)
        {
            Mask = new List<byte>();
            Pattern = new List<byte>();
            FlowControl = new List<byte>();
            TxFlags = TxFlag.NONE;

            switch (FilterType)
            {
                case CommonFilter.PASSALL:
                    PassAll();
                    break;
                case CommonFilter.PASS:
                    Pass(Match);
                    break;
                case CommonFilter.BLOCK:
                    Block(Match);
                    break;
                case CommonFilter.STANDARDISO15765:
                    StandardISO15765(Match);
                    break;
                case CommonFilter.NONE:
                    break;
            }

        }

        public void Clear()
        {
            Mask.Clear();
            Pattern.Clear();
            FlowControl.Clear();
        }

        public void PassAll()
        {
            Clear();
            Mask.Add(0x00);
            Pattern.Add(0x00);
            FilterType = FilterEnum.PASS_FILTER;
        }

        public void Pass(List<byte> Match)
        {
            ExactMatch(Match);
            FilterType = FilterEnum.PASS_FILTER;
        }

        public void Block(List<byte> Match)
        {
            ExactMatch(Match);
            FilterType = FilterEnum.BLOCK_FILTER;
        }

        private void ExactMatch(List<byte> Match)
        {
            Clear();
            Mask = Enumerable.Repeat((byte)0xFF, Match.Count).ToList();
            Pattern = Match;
        }
        public void StandardISO15765(List<byte> SourceAddress)
        {
            //Should throw exception??
            if (SourceAddress.Count != 4)
                return;
            Clear();
            Mask.Add(0xFF);
            Mask.Add(0xFF);
            Mask.Add(0xFF);
            Mask.Add(0xFF);

            Pattern.AddRange(SourceAddress);
            Pattern[3] += 0x08;

            FlowControl.AddRange(SourceAddress);

            TxFlags = TxFlag.ISO15765_FRAME_PAD;
            FilterType = FilterEnum.FLOW_CONTROL_FILTER;
        }
    }

    [Flags]
    public enum RxStatus
    {
        NONE = 0x00000000,
        TX_MSG_TYPE = 0x00000001,
        START_OF_MESSAGE = 0x00000002,
        RX_BREAK = 0x00000004,
        TX_INDICATION = 0x00000008,
        ISO15765_PADDING_ERROR = 0x00000010,
        ISO15765_ADDR_TYPE = 0x00000080,
        CAN_29BIT_ID = 0x00000100
    }

    [Flags]
    public enum ConnectFlag
    {
        NONE = 0x0000,
        SNIFF_MODE = 0x10000000,    //Drewtech only
        ISO9141_K_LINE_ONLY = 0x1000,
        CAN_ID_BOTH = 0x0800,
        ISO9141_NO_CHECKSUM = 0x0200,
        CAN_29BIT_ID = 0x0100
    }

    [Flags]
    public enum TxFlag
    {
        NONE = 0x00000000,
        SCI_TX_VOLTAGE = 0x00800000,
        SCI_MODE = 0x00400000,
        WAIT_P3_MIN_ONLY = 0x00000200,
        CAN_29BIT_ID = 0x00000100,
        ISO15765_ADDR_TYPE = 0x00000080,
        ISO15765_FRAME_PAD = 0x00000040
    }

    public enum Protocols
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

    public enum BaudRate
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

    public enum Pin
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

    public enum FilterEnum
    {
        PASS_FILTER = 0x01,
        BLOCK_FILTER = 0x02,
        FLOW_CONTROL_FILTER = 0x03
    }

    enum Ioctl
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
        READ_PROG_VOLTAGE = 0x0E
    }

    public enum J2534Err
    {
        STATUS_NOERROR = 0x00,
        ERR_NOT_SUPPORTED = 0x01,
        ERR_INVALID_CHANNEL_ID = 0x02,
        ERR_INVALID_PROTOCOL_ID = 0x03,
        ERR_NULL_PARAMETER = 0x04,
        ERR_INVALID_IOCTL_VALUE = 0x05,
        ERR_INVALID_FLAGS = 0x06,
        ERR_FAILED = 0x07,
        ERR_DEVICE_NOT_CONNECTED = 0x08,
        ERR_TIMEOUT = 0x09,
        ERR_INVALID_MSG = 0x0A,
        ERR_INVALID_TIME_INTERVAL = 0x0B,
        ERR_EXCEEDED_LIMIT = 0x0C,
        ERR_INVALID_MSG_ID = 0x0D,
        ERR_DEVICE_IN_USE = 0x0E,
        ERR_INVALID_IOCTL_ID = 0x0F,
        ERR_BUFFER_EMPTY = 0x10,
        ERR_BUFFER_FULL = 0x11,
        ERR_BUFFER_OVERFLOW = 0x12,
        ERR_PIN_INVALID = 0x13,
        ERR_CHANNEL_IN_USE = 0x14,
        ERR_MSG_PROTOCOL_ID = 0x15,
        ERR_INVALID_FILTER_ID = 0x16,
        ERR_NO_FLOW_CONTROL = 0x17,
        ERR_NOT_UNIQUE = 0x18,
        ERR_INVALID_BAUDRATE = 0x19,
        ERR_INVALID_DEVICE_ID = 0x1A
    }

    public enum cfg_prm_id  //Parameter selection values used in PassThruIoCtl->Set/Get Config
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
        //T3_MAX = 0x24,
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

    [StructLayout(LayoutKind.Sequential)]
    public struct SConfig
    {
        public int Parameter;
        public int Value;
    }
}
