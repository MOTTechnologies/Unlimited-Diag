using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace J2534
{
    public class Channel
    {
        private J2534Device Device;
        private int ChannelID;
        private J2534HeapMessageArray HeapMessageArray;
        private Sieve MessageSieve = new Sieve();
        public bool IsConnected { get; private set; }
        public J2534PROTOCOL ProtocolID { get; private set; }
        public int Baud { get; private set; }
        public J2534CONNECTFLAG ConnectFlags { get; internal set; }
        public List<PeriodicMsg> PeriodicMsgList = new List<PeriodicMsg>();
        public List<MessageFilter> FilterList = new List<MessageFilter>();
        public int DefaultTxTimeout { get; set; }
        public int DefaultRxTimeout { get; set; }
        public J2534TXFLAG DefaultTxFlag { get; set; }

        //Channel Constructor
        internal Channel(J2534Device Device, J2534PROTOCOL ProtocolID, J2534BAUD Baud, J2534CONNECTFLAG ConnectFlags)
        {
            HeapMessageArray = new J2534HeapMessageArray(CONST.HEAPMESSAGEBUFFERSIZE);
            this.Device = Device;
            this.ProtocolID = ProtocolID;
            this.Baud = (int)Baud;
            this.ConnectFlags = ConnectFlags;
            DefaultTxTimeout = 50;
            DefaultRxTimeout = 250;
            DefaultTxFlag = J2534TXFLAG.NONE;
            Connect();
        }

        private void Connect()
        {
            J2534ERR Status;
            
            J2534HeapInt ChannelID = new J2534HeapInt();

            lock (Device.Library.API_LOCK)
            {
                Status = (J2534ERR)Device.Library.API.Connect(Device.DeviceID, (int)ProtocolID, (int)ConnectFlags, Baud, ChannelID);
                if(Status == J2534ERR.STATUS_NOERROR)
                {
                    IsConnected = true;
                    this.ChannelID = ChannelID;
                }
                else
                    throw new J2534Exception(Status, Device.Library.GetLastError());
            }
        }

        public void Disconnect()
        {
            J2534ERR Status;

            lock (Device.Library.API_LOCK)
            {
                IsConnected = false;
                Status = (J2534ERR)Device.Library.API.Disconnect(ChannelID);
                if (Status != J2534ERR.STATUS_NOERROR)                    
                    throw new J2534Exception(Status, Device.Library.GetLastError());
            }
        }

        public GetMessageResults MessageTransaction(byte[] TxMessage, int NumOfRxMsgs, Predicate<J2534Message> Comparer)
        {
            MessageSieve.AddScreen(10, Comparer);
            J2534ERR Status = SendMessage(TxMessage);
            if (Status == J2534ERR.STATUS_NOERROR)
                return GetMessages(NumOfRxMsgs, DefaultRxTimeout, Comparer, true);
            throw new J2534Exception(Status, Device.Library.GetLastError());
        }

        public GetMessageResults MessageTransaction(List<J2534Message> TxMessages, int NumOfRxMsgs, Predicate<J2534Message> Comparer)
        {
            MessageSieve.AddScreen(10, Comparer);
            J2534ERR Status = SendMessages(TxMessages);
            if (Status == J2534ERR.STATUS_NOERROR)
                return GetMessages(NumOfRxMsgs, DefaultRxTimeout, Comparer, true);
            throw new J2534Exception(Status, Device.Library.GetLastError());
        }

        public GetMessageResults GetMessage()
        {
            return GetMessages(1, DefaultRxTimeout);
        }

        /// <summary>
        /// Reads 'NumMsgs' messages from the input buffer and then the device.  Will block
        /// until it gets 'NumMsgs' messages, or 'DefaultRxTimeout' expires.
        /// </summary>
        /// <param name="NumMsgs"></param>
        /// <returns>Returns 'false' if successful</returns>
        public GetMessageResults GetMessages(int NumMsgs)
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
        public GetMessageResults GetMessages(int NumMsgs, int Timeout)
        {
            GetMessageResults Results = new GetMessageResults();

            lock (Device.Library.API_LOCK)
            {
                HeapMessageArray.Length = NumMsgs;
                Results.Status = (J2534ERR)Device.Library.API.ReadMsgs(ChannelID, HeapMessageArray, HeapMessageArray.Length, Timeout);
                Results.Messages = HeapMessageArray.ToList();
            }
            return Results;
        }

        //Thread safety in this method assumes that each thread will have unique comparers
        public GetMessageResults GetMessages(int NumMsgs, int Timeout, Predicate<J2534Message> ComparerAsKey, bool Remove)
        {
            bool GetMoreMessages;
            Stopwatch SW = new Stopwatch();
            SW.Start();

            do
            {
                GetMessageResults RxMessages = GetMessages(CONST.HEAPMESSAGEBUFFERSIZE, 0);
                if (RxMessages.Status == J2534ERR.STATUS_NOERROR ||
                    RxMessages.Status == J2534ERR.BUFFER_EMPTY)
                    MessageSieve.ExtractFrom(RxMessages.Messages);
                else
                    throw new J2534Exception(RxMessages.Status, Device.Library.GetLastError());
                GetMoreMessages = (MessageSieve.ScreenMessageCount(ComparerAsKey) < NumMsgs);

            } while (GetMoreMessages && (SW.ElapsedMilliseconds < Timeout));

            if(GetMoreMessages)
                return new GetMessageResults(MessageSieve.EmptyScreen(ComparerAsKey, Remove), J2534ERR.TIMEOUT);
            else
                return new GetMessageResults(MessageSieve.EmptyScreen(ComparerAsKey, Remove), J2534ERR.STATUS_NOERROR);
        }

        /// <summary>
        /// Sends a single message 'Message'
        /// </summary>
        /// <param name="Message"></param>
        /// <returns>Returns 'false' if successful</returns>
        public J2534ERR SendMessage(byte[] Message)
        {
            lock (Device.Library.API_LOCK)
            {
                HeapMessageArray.PopulateWith(ProtocolID, DefaultTxFlag, Message);
                return (J2534ERR)Device.Library.API.WriteMsgs(ChannelID, HeapMessageArray, HeapMessageArray.Length, DefaultTxTimeout);
            }
        }

        /// <summary>
        /// Sends all messages contained in 'MsgList'
        /// </summary>
        /// <returns>Returns 'false' if successful</returns>
        public J2534ERR SendMessages(List<J2534Message> Messages)
        {
            lock (Device.Library.API_LOCK)
            {
                HeapMessageArray.PopulateWith(Messages);
                return (J2534ERR)Device.Library.API.WriteMsgs(ChannelID, HeapMessageArray, HeapMessageArray.Length, DefaultTxTimeout);
            }
        }

        public void StartPeriodicMessage(J2534Message Message, int Interval)
        {
            J2534ERR Status;

            lock (Device.Library.API_LOCK)
            {
                PeriodicMsgList.Add(new PeriodicMsg(Message, Interval));
                if((Status = StartPeriodicMessage(PeriodicMsgList.Count - 1)) != J2534ERR.STATUS_NOERROR)
                {
                    PeriodicMsgList.RemoveAt(PeriodicMsgList.Count - 1);
                    throw new J2534Exception(Status, Device.Library.GetLastError());
                }
            }
        }

        private J2534ERR StartPeriodicMessage(int Index)
        {
            J2534ERR Status;

            J2534HeapInt MessageID = new J2534HeapInt();

            J2534HeapMessage Message = new J2534HeapMessage(PeriodicMsgList[Index].Message);

            Status = (J2534ERR)Device.Library.API.StartPeriodicMsg(ChannelID, Message, MessageID, PeriodicMsgList[Index].Interval);

            PeriodicMsgList[Index].MessageID = MessageID;
            return Status;
        }

        /// <summary>
        /// Stops the periodic message in 'PeriodicMsgList' referenced by 'Index'.
        /// </summary>
        /// <param name="Index"></param>
        /// <returns>Returns 'false' if successful</returns>
        public void StopPeriodicMsg(int Index)
        {
            J2534ERR Status;
            lock (Device.Library.API_LOCK)
                Status = (J2534ERR)Device.Library.API.StopPeriodicMsg(ChannelID, PeriodicMsgList[Index].MessageID);
            if(Status != J2534ERR.STATUS_NOERROR)
                throw new J2534Exception(Status, Device.Library.GetLastError());
        }

        /// <summary>
        /// Starts a single message filter and if successful, adds it to the FilterList.
        /// </summary>
        /// <param name="Filter"></param>
        /// <returns>Returns false if successful</returns>
        public void StartMsgFilter(MessageFilter Filter)
        {
            J2534ERR Status;

            lock (Device.Library.API_LOCK)
            {
                FilterList.Add(Filter);
                if((Status = StartMsgFilter(FilterList.Count - 1)) != J2534ERR.STATUS_NOERROR)
                {
                    FilterList.RemoveAt(FilterList.Count - 1);
                    throw new J2534Exception(Status, Device.Library.GetLastError());
                }
            }
        }

        private J2534ERR StartMsgFilter(int Index)
        {
            J2534ERR Status;
            J2534HeapInt FilterID = new J2534HeapInt();

            J2534HeapMessage Mask = new J2534HeapMessage(new J2534Message(ProtocolID, FilterList[Index].TxFlags, FilterList[Index].Mask));
            J2534HeapMessage Pattern = new J2534HeapMessage(new J2534Message(ProtocolID, FilterList[Index].TxFlags, FilterList[Index].Pattern));
            J2534HeapMessage FlowControl = new J2534HeapMessage(new J2534Message(ProtocolID, FilterList[Index].TxFlags, FilterList[Index].FlowControl));
            //The lock is performed in the calling method to protect the 'FilterList' coherency.
            if (FilterList[Index].FilterType == J2534FILTER.FLOW_CONTROL_FILTER)
                Status = (J2534ERR)Device.Library.API.StartMsgFilter(ChannelID, (int)FilterList[Index].FilterType, Mask, Pattern, FlowControl, FilterID);
            else
                Status = (J2534ERR)Device.Library.API.StartMsgFilter(ChannelID, (int)FilterList[Index].FilterType, Mask, Pattern, IntPtr.Zero, FilterID);

            FilterList[Index].FilterId = FilterID;
            return Status;
        }

        public void StopMsgFilter(int Index)
        {
            J2534ERR Status;
            lock(Device.Library.API_LOCK)
                Status =  (J2534ERR)Device.Library.API.StopMsgFilter(ChannelID, FilterList[Index].FilterId);
            if(Status != J2534ERR.STATUS_NOERROR)
                throw new J2534Exception(Status, Device.Library.GetLastError());
        }

        public int GetConfig(J2534PARAMETER Parameter)
        {
            J2534ERR Status;
            HeapSConfigArray SConfigArray = new HeapSConfigArray(new J2534.SConfig(Parameter, 0));
            lock (Device.Library.API_LOCK)
            {
                Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.GET_CONFIG, SConfigArray, IntPtr.Zero);
                if (Status != J2534ERR.STATUS_NOERROR)
                    throw new J2534Exception(Status, Device.Library.GetLastError());
                return SConfigArray[0].Value;
            }
        }

        public List<SConfig> GetConfig(List<SConfig> SConfig)
        {
            J2534ERR Status;
            HeapSConfigArray SConfigArray = new HeapSConfigArray(SConfig);

            lock (Device.Library.API_LOCK)
            {
                Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.GET_CONFIG, SConfigArray, IntPtr.Zero);
                if (Status != J2534ERR.STATUS_NOERROR)
                    throw new J2534Exception(Status, Device.Library.GetLastError());
                return SConfigArray.ToList();  //Implicit conversion to list ;)
            }
        }

        public void SetConfig(J2534PARAMETER Parameter, int Value)
        {
            J2534ERR Status;
            HeapSConfigArray SConfigList = new HeapSConfigArray(new SConfig(Parameter, Value));
            lock (Device.Library.API_LOCK)
                Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.SET_CONFIG, SConfigList, IntPtr.Zero);
            if(Status != J2534ERR.STATUS_NOERROR)
                throw new J2534Exception(Status, Device.Library.GetLastError());
        }

        public void SetConfig(List<SConfig> SConfig)
        {
            J2534ERR Status;
            HeapSConfigArray SConfigList = new HeapSConfigArray(SConfig);
            lock (Device.Library.API_LOCK)
                Status = Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.SET_CONFIG, SConfigList, IntPtr.Zero);
            if (Status != J2534ERR.STATUS_NOERROR)
                throw new J2534Exception(Status, Device.Library.GetLastError());
        }

        public void ClearTxBuffer()
        {
            J2534ERR Status;
            lock (Device.Library.API_LOCK)
                Status = Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.CLEAR_TX_BUFFER, IntPtr.Zero, IntPtr.Zero);
            if (Status != J2534ERR.STATUS_NOERROR)
                throw new J2534Exception(Status, Device.Library.GetLastError());
        }

        public void ClearRxBuffer()
        {
            J2534ERR Status;
            lock (Device.Library.API_LOCK)
                Status = Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.CLEAR_RX_BUFFER, IntPtr.Zero, IntPtr.Zero);
            if (Status != J2534ERR.STATUS_NOERROR)
                throw new J2534Exception(Status, Device.Library.GetLastError());
        }

        public void ClearPeriodicMsgs()
        {
            J2534ERR Status;
            lock (Device.Library.API_LOCK)
                Status = Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.CLEAR_PERIODIC_MSGS, IntPtr.Zero, IntPtr.Zero);
            if (Status != J2534ERR.STATUS_NOERROR)
                throw new J2534Exception(Status, Device.Library.GetLastError());
        }

        public void ClearMsgFilters()
        {
            J2534ERR Status;
            lock (Device.Library.API_LOCK)
                Status = Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.CLEAR_MSG_FILTERS, IntPtr.Zero, IntPtr.Zero);
            if (Status != J2534ERR.STATUS_NOERROR)
                throw new J2534Exception(Status, Device.Library.GetLastError());
        }

        public void ClearFunctMsgLookupTable()
        {
            J2534ERR Status;
            lock (Device.Library.API_LOCK)
                Status = Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.CLEAR_FUNCT_MSG_LOOKUP_TABLE, IntPtr.Zero, IntPtr.Zero);
            if (Status != J2534ERR.STATUS_NOERROR)
                throw new J2534Exception(Status, Device.Library.GetLastError());
        }

        public void AddToFunctMsgLookupTable(byte Addr)
        {
            J2534ERR Status;
            HeapSByteArray SByteArray = new HeapSByteArray(Addr);
            lock (Device.Library.API_LOCK)
                Status = Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.ADD_TO_FUNCT_MSG_LOOKUP_TABLE, SByteArray, IntPtr.Zero);
            if (Status != J2534ERR.STATUS_NOERROR)
                throw new J2534Exception(Status, Device.Library.GetLastError());
        }

        public void AddToFunctMsgLookupTable(List<byte> AddressList)
        {
            J2534ERR Status;
            HeapSByteArray SByteArray = new HeapSByteArray(AddressList.ToArray());
            lock(Device.Library.API_LOCK)
                Status = Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.ADD_TO_FUNCT_MSG_LOOKUP_TABLE, SByteArray, IntPtr.Zero);
            if (Status != J2534ERR.STATUS_NOERROR)
                throw new J2534Exception(Status, Device.Library.GetLastError());
        }

        public void DeleteFromFunctMsgLookupTable(byte Addr)
        {
            J2534ERR Status;
            HeapSByteArray SByteArray = new HeapSByteArray(Addr);
            lock (Device.Library.API_LOCK)
                Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.DELETE_FROM_FUNCT_MSG_LOOKUP_TABLE, SByteArray, IntPtr.Zero);
            if (Status != J2534ERR.STATUS_NOERROR)
                throw new J2534Exception(Status, Device.Library.GetLastError());
        }

        public void DeleteFromFunctMsgLookupTable(List<byte> AddressList)
        {
            J2534ERR Status;
            HeapSByteArray SByteArray = new HeapSByteArray(AddressList.ToArray());
            lock(Device.Library.API_LOCK)
                Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.DELETE_FROM_FUNCT_MSG_LOOKUP_TABLE, SByteArray, IntPtr.Zero);
            if (Status != J2534ERR.STATUS_NOERROR)
                throw new J2534Exception(Status, Device.Library.GetLastError());
        }

        public byte[] FiveBaudInit(byte TargetAddress)
        {
            J2534ERR Status;
            HeapSByteArray Input = new HeapSByteArray(new byte[] { TargetAddress });
            HeapSByteArray Output = new HeapSByteArray(new byte[2]);
            lock (Device.Library.API_LOCK)
            {
                Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.FIVE_BAUD_INIT, Input, Output);
                if (Status != J2534ERR.STATUS_NOERROR)
                    throw new J2534Exception(Status, Device.Library.GetLastError());
                return Output;
            }
        }

        public J2534Message FastInit(J2534Message TxMessage)
        {
            J2534ERR Status;
            J2534HeapMessage Input = new J2534HeapMessage(TxMessage);
            J2534HeapMessage Output = new J2534HeapMessage();
            lock (Device.Library.API_LOCK)
            {
                Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.FAST_INIT, Input, Output);
                if (Status != J2534ERR.STATUS_NOERROR)
                    throw new J2534Exception(Status, Device.Library.GetLastError());
                return Output;
            }
        }

        public void SetProgrammingVoltage(J2534PIN PinNumber, int Voltage)
        {
            Device.SetProgrammingVoltage(PinNumber, Voltage);
        }

        public int MeasureProgrammingVoltage()
        {
            if (Device.Library.API_Signature.SAE_API == SAE_API.V202_SIGNATURE)
            {
                J2534ERR Status;
                J2534HeapInt Voltage = new J2534HeapInt();
                lock(Device.Library.API_LOCK)
                {
                    Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.READ_PROG_VOLTAGE, IntPtr.Zero, Voltage);
                    if (Status != J2534ERR.STATUS_NOERROR)
                        throw new J2534Exception(Status, Device.Library.GetLastError());
                    return Voltage;
                }
            }
            return Device.MeasureProgrammingVoltage();
        }

        public int MeasureBatteryVoltage()
        {
            if(Device.Library.API_Signature.SAE_API == SAE_API.V202_SIGNATURE)
            {
                J2534ERR Status;
                J2534HeapInt Voltage = new J2534HeapInt();
                lock (Device.Library.API_LOCK)
                {
                    Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.READ_VBATT, IntPtr.Zero, Voltage);
                    if (Status != J2534ERR.STATUS_NOERROR)
                        throw new J2534Exception(Status, Device.Library.GetLastError());
                    return Voltage;
                }
            }
            return Device.MeasureBatteryVoltage();
        }
    }
}
