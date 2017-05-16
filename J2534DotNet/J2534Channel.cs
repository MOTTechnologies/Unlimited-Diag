using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace J2534
{
    public class Channel
    {
        private J2534PhysicalDevice Device;
        private IntPtr pChannelID;
        public J2534HeapMessageArray HeapMessages;
        public bool IsConnected { get; private set; }
        public J2534PROTOCOL ProtocolID { get; private set; }
        public int Baud { get; private set; }
        public J2534CONNECTFLAG ConnectFlags { get; internal set; }
        public List<PeriodicMsg> PeriodicMsgList;
        public List<MessageFilter> FilterList;
        public int DefaultTxTimeout { get; set; }
        public int DefaultRxTimeout { get; set; }
        public J2534TXFLAG DefaultTxFlag { get; set; }
        public byte FiveBaudKeyword1 { get; set; }
        public byte FiveBaudKeyword2 { get; set; }
        private int J2534MessageSize = Marshal.SizeOf<J2534Message>();

        public J2534ERR Status
        {
            get
            {
                return Device.Library.Status;
            }
            set
            {
                Device.Library.Status = value;
            }
        }

        internal Channel(J2534PhysicalDevice Device, J2534PROTOCOL ProtocolID, J2534BAUD Baud, J2534CONNECTFLAG ConnectFlags)
        {
            pChannelID = Marshal.AllocHGlobal(4);
            HeapMessages = new J2534HeapMessageArray(200);
            this.Device = Device;
            this.ProtocolID = ProtocolID;
            this.Baud = (int)Baud;
            this.ConnectFlags = ConnectFlags;
            Connect();
            PeriodicMsgList = new List<PeriodicMsg>();
            FilterList = new List<J2534.MessageFilter>();
            DefaultTxTimeout = 50;
            DefaultRxTimeout = 250;
            DefaultTxFlag = J2534TXFLAG.NONE;
        }

        private void Connect()
        {
            Status = (J2534ERR)Device.Library.API.Connect(Device.DeviceID, (int)ProtocolID, (int)ConnectFlags, Baud, pChannelID);
        }

        public bool Disconnect()
        {
            Status = (J2534ERR)Device.Library.API.Disconnect(pChannelID);
            if (Status == J2534ERR.STATUS_NOERROR)
            {
                IsConnected = false;
                return CONST.SUCCESS;
            }
            return CONST.FAILURE;
        }

        public bool GetMessage()
        {
            return GetMessages(1, DefaultRxTimeout);
        }

        /// <summary>
        /// Reads 'NumMsgs' messages from the input buffer and then the device.  Will block
        /// until it gets 'NumMsgs' messages, or 'DefaultRxTimeout' expires.
        /// </summary>
        /// <param name="NumMsgs"></param>
        /// <returns>Returns 'false' if successful</returns>
        public bool GetMessages(int NumMsgs)
        {
            return GetMessages(NumMsgs, DefaultRxTimeout);
        }

        /// <summary>
        /// Reads 'NumMsgs' messages from the input buffer and then the device.  Will block
        /// until it gets 'NumMsgs' messages, or 'Timeout' expires.
        /// </summary>
        /// <param name="NumMsgs"></param>
        /// <param name="Timeout"></param>
        /// <returns>Returns 'false' if successful</returns>
        public bool GetMessages(int NumMsgs, int Timeout)
        {
            HeapMessages.Length = NumMsgs;

            Status = (J2534ERR)Device.Library.API.ReadMsgs(pChannelID, HeapMessages, HeapMessages.NumMsgs, Timeout);

            if(Status == J2534ERR.STATUS_NOERROR)
                return CONST.SUCCESS;
            return CONST.FAILURE;
        }

        /// <summary>
        /// Sends a single message 'Message'
        /// </summary>
        /// <param name="Message"></param>
        /// <returns>Returns 'false' if successful</returns>
        public bool SendMessage(byte[] Message)
        {
            HeapMessages[0] = new J2534.J2534Message(ProtocolID, DefaultTxFlag, Message);
            HeapMessages.Length = 1;

            return SendMessages();
        }

        /// <summary>
        /// Sends all messages contained in 'MsgList'
        /// </summary>
        /// <returns>Returns 'false' if successful</returns>
        public bool SendMessages(List<J2534Message> Messages)
        {
            HeapMessages.Length = Messages.Count;
            for (int i = 0;i < Messages.Count; i++)
                HeapMessages[i] = Messages[i];

            return SendMessages();
        }

        public bool SendMessages()
        {
            Status = (J2534ERR)Device.Library.API.WriteMsgs(pChannelID, HeapMessages, HeapMessages.NumMsgs, DefaultTxTimeout);

            if (Status == J2534ERR.STATUS_NOERROR)
                return CONST.SUCCESS;
            return CONST.FAILURE;
        }

        public bool StartPeriodicMessage(J2534Message Message, int Interval)
        {
            PeriodicMsgList.Add(new PeriodicMsg(Message, Interval));

            //If success
            if (!StartPeriodicMessage(PeriodicMsgList.Count - 1))
                return CONST.SUCCESS;
            //Otherwise, remove it from the list, and return fail
            PeriodicMsgList.RemoveAt(PeriodicMsgList.Count - 1);
            return CONST.FAILURE;
        }

        /// <summary>
        /// Starts the periodic message in 'PeriodicMsgList' referenced by 'Index'
        /// </summary>
        /// <param name="Index"></param>
        /// <returns>Returns 'false' if successful</returns>
        public bool StartPeriodicMessage(int Index)
        {
            J2534HeapInt MessageID = new J2534HeapInt();

            J2534HeapMessage Message = new J2534HeapMessage(PeriodicMsgList[Index].Message);

            Status = (J2534ERR)Device.Library.API.StartPeriodicMsg(pChannelID, Message.Ptr, MessageID, PeriodicMsgList[Index].Interval);

            PeriodicMsgList[Index].MessageID = MessageID;

            if (Status == J2534ERR.STATUS_NOERROR)
                return CONST.SUCCESS;
            return CONST.FAILURE;
        }

        /// <summary>
        /// Stops the periodic message in 'PeriodicMsgList' referenced by 'Index'.
        /// </summary>
        /// <param name="Index"></param>
        /// <returns>Returns 'false' if successful</returns>
        public bool StopPeriodicMsg(int Index)
        {
            Status = (J2534ERR)Device.Library.API.StopPeriodicMsg(pChannelID, PeriodicMsgList[Index].MessageID);
            if (Status == J2534ERR.STATUS_NOERROR)
                return CONST.SUCCESS;
            return CONST.FAILURE;
        }

        /// <summary>
        /// Starts a single message filter and if successful, adds it to the FilterList.
        /// </summary>
        /// <param name="Filter"></param>
        /// <returns>Returns false if successful</returns>
        public bool StartMsgFilter(MessageFilter Filter)
        {
            FilterList.Add(Filter);
            if(StartMsgFilter(FilterList.Count - 1))
            {
                FilterList.RemoveAt(FilterList.Count - 1);
                return CONST.FAILURE;
            }
            return CONST.SUCCESS;
        }

        public bool StartMsgFilter(int Index)
        {
            J2534HeapInt FilterID = new J2534HeapInt();

            J2534HeapMessage Mask = new J2534HeapMessage(new J2534Message(ProtocolID, FilterList[Index].TxFlags, FilterList[Index].Mask));
            J2534HeapMessage Pattern = new J2534HeapMessage(new J2534Message(ProtocolID, FilterList[Index].TxFlags, FilterList[Index].Pattern));
            J2534HeapMessage FlowControl = new J2534HeapMessage(new J2534Message(ProtocolID, FilterList[Index].TxFlags, FilterList[Index].FlowControl));

            if (FilterList[Index].FilterType == J2534FILTER.FLOW_CONTROL_FILTER)
                Status = (J2534ERR)Device.Library.API.StartMsgFilter(pChannelID, (int)FilterList[Index].FilterType, Mask.Ptr, Pattern.Ptr, FlowControl.Ptr, FilterID);
            else
                Status = (J2534ERR)Device.Library.API.StartMsgFilter(pChannelID, (int)FilterList[Index].FilterType, Mask.Ptr, Pattern.Ptr, IntPtr.Zero, FilterID);

            FilterList[Index].FilterId = FilterID;

            if (Status == J2534ERR.STATUS_NOERROR)
                return CONST.SUCCESS;
            return CONST.FAILURE;
        }

        public bool StopMsgFilter(int Index)
        {
            Status = (J2534ERR)Device.Library.API.StopMsgFilter(pChannelID, FilterList[Index].FilterId);
            if (Status == J2534ERR.STATUS_NOERROR)
                return CONST.SUCCESS;
            return CONST.FAILURE;
        }

        public bool GetConfig(J2534PARAMETER Parameter, ref int Value)
        {
            List<SConfig> SConfig = GetConfig(new List<SConfig>() { new J2534.SConfig(Parameter, 0) });
            if (SConfig.Count > 0)
                Value = SConfig[0].Value;

            if (Status == J2534ERR.STATUS_NOERROR)
                return CONST.SUCCESS;
            return CONST.FAILURE;
        }

        public List<SConfig> GetConfig(List<SConfig> SConfig)
        {
            HeapSConfigList SConfigList = new HeapSConfigList(SConfig);

            Status = (J2534ERR)Device.Library.API.IOCtl(pChannelID, (int)J2534IOCTL.GET_CONFIG, SConfigList, IntPtr.Zero);
 
            return SConfigList;
        }

        public bool SetConfig(J2534PARAMETER Parameter, int Value)
        {
            return SetConfig(new List<SConfig>() { new SConfig(Parameter, Value) });
        }

        public bool SetConfig(List<SConfig> SConfig)
        {
            HeapSConfigList SConfigList = new HeapSConfigList(SConfig);

            Status = (J2534ERR)Device.Library.API.IOCtl(pChannelID, (int)J2534IOCTL.SET_CONFIG, SConfigList, IntPtr.Zero);

            if (Status == J2534ERR.STATUS_NOERROR)
                return CONST.SUCCESS;
            return CONST.FAILURE;
        }

        public bool ClearTxBuffer()
        {
            Status = (J2534ERR)Device.Library.API.IOCtl(pChannelID, (int)J2534IOCTL.CLEAR_TX_BUFFER, IntPtr.Zero, IntPtr.Zero);
            if (Status == J2534ERR.STATUS_NOERROR)
                return CONST.SUCCESS;
            return CONST.FAILURE;
        }

        public bool ClearRxBuffer()
        {
            Status = (J2534ERR)Device.Library.API.IOCtl(pChannelID, (int)J2534IOCTL.CLEAR_RX_BUFFER, IntPtr.Zero, IntPtr.Zero);
            if (Status == J2534ERR.STATUS_NOERROR)
                return CONST.SUCCESS;
            return CONST.FAILURE;
        }

        public bool ClearPeriodicMsgs()
        {
            Status = (J2534ERR)Device.Library.API.IOCtl(pChannelID, (int)J2534IOCTL.CLEAR_PERIODIC_MSGS, IntPtr.Zero, IntPtr.Zero);
            if (Status == J2534ERR.STATUS_NOERROR)
                return CONST.SUCCESS;
            return CONST.FAILURE;
        }

        public bool ClearMsgFilters()
        {
            Status = (J2534ERR)Device.Library.API.IOCtl(pChannelID, (int)J2534IOCTL.CLEAR_MSG_FILTERS, IntPtr.Zero, IntPtr.Zero);
            if (Status == J2534ERR.STATUS_NOERROR)
                return CONST.SUCCESS;
            return CONST.FAILURE;
        }

        public bool ClearFunctMsgLookupTable()
        {
            Status = (J2534ERR)Device.Library.API.IOCtl(pChannelID, (int)J2534IOCTL.CLEAR_FUNCT_MSG_LOOKUP_TABLE, IntPtr.Zero, IntPtr.Zero);
            if (Status == J2534ERR.STATUS_NOERROR)
                return CONST.SUCCESS;
            return CONST.FAILURE;
        }

        public bool AddToFunctMsgLookupTable(byte Addr)
        {
            return AddToFunctMsgLookupTable(new List<byte>() { Addr });
        }

        public bool AddToFunctMsgLookupTable(List<byte> FuncAddr)
        {
            HeapSByteArray SByteArray = new HeapSByteArray(FuncAddr.ToArray());

            Status = (J2534ERR)Device.Library.API.IOCtl(pChannelID, (int)J2534IOCTL.ADD_TO_FUNCT_MSG_LOOKUP_TABLE, SByteArray, IntPtr.Zero);

            if (Status == J2534ERR.STATUS_NOERROR)
                return CONST.SUCCESS;
            return CONST.FAILURE;
        }

        public bool DeleteFromFunctMsgLookupTable(byte Addr)
        {
            return DeleteFromFunctMsgLookupTable(new List<byte>() { Addr });
        }

        public bool DeleteFromFunctMsgLookupTable(List<byte> FuncAddr)
        {
            HeapSByteArray SByteArray = new HeapSByteArray(FuncAddr.ToArray());

            Status = (J2534ERR)Device.Library.API.IOCtl(pChannelID, (int)J2534IOCTL.DELETE_FROM_FUNCT_MSG_LOOKUP_TABLE, SByteArray, IntPtr.Zero);

            if (Status == J2534ERR.STATUS_NOERROR)
                return CONST.SUCCESS;
            return CONST.FAILURE;
        }

        public bool FiveBaudInit(byte TargetAddress)
        {
            HeapSByteArray Input = new HeapSByteArray(new byte[] { TargetAddress });
            HeapSByteArray Output = new HeapSByteArray(new byte[2]);

            Status = (J2534ERR)Device.Library.API.IOCtl(pChannelID, (int)J2534IOCTL.FIVE_BAUD_INIT, Input, Output);

            if (Status == J2534ERR.STATUS_NOERROR)
            {
                    FiveBaudKeyword1 = Output[0];
                    FiveBaudKeyword2 = Output[1];
                    return CONST.SUCCESS;
            }
            return CONST.FAILURE;
        }

        public bool FastInit(J2534Message TxMessage)
        {
            J2534HeapMessage input = new J2534HeapMessage(TxMessage);

            Status = (J2534ERR)Device.Library.API.IOCtl(pChannelID, (int)J2534IOCTL.FAST_INIT, input, HeapMessages);

            if (Status == J2534ERR.STATUS_NOERROR)
                return CONST.SUCCESS;
            return CONST.FAILURE;
        }

        public bool SetProgrammingVoltage(J2534PIN PinNumber, int Voltage)
        {
            return Device.SetProgrammingVoltage(PinNumber, Voltage);
        }

        public int MeasureProgrammingVoltage()
        {
            return Device.MeasureProgrammingVoltage();
        }

        public int MeasureBatteryVoltage()
        {
            return Device.MeasureBatteryVoltage();
        }

    }
}
