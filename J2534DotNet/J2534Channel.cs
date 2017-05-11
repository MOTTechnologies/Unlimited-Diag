using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace J2534
{
    public class Channel
    {
        private J2534PhysicalDevice Device;
        private int ChannelID;
        public J2534ERR Status { get; private set; }
        public bool IsConnected { get; private set; }
        public J2534PROTOCOL ProtocolID { get; private set; }
        public int Baud { get; private set; }
        public J2534CONNECTFLAG ConnectFlags { get; internal set; }
        public List<J2534Message> RxMessages;
        public List<PeriodicMsg> PeriodicMsgList;
        public List<MessageFilter> FilterList;
        public int DefaultTxTimeout { get; set; }
        public int DefaultRxTimeout { get; set; }
        public J2534TXFLAG DefaultTxFlag { get; set; }
        public byte FiveBaudKeyword1 { get; set; }
        public byte FiveBaudKeyword2 { get; set; }
        private int J2534MessageSize = Marshal.SizeOf<J2534Message>();

        internal Channel(J2534PhysicalDevice Device, J2534PROTOCOL ProtocolID, J2534BAUD Baud, J2534CONNECTFLAG ConnectFlags)
        {
            this.Device = Device;
            this.ProtocolID = ProtocolID;
            this.Baud = (int)Baud;
            this.ConnectFlags = ConnectFlags;
            Connect();
            Status = Device.Status;
            RxMessages = new List<J2534.J2534Message>();
            PeriodicMsgList = new List<PeriodicMsg>();
            FilterList = new List<J2534.MessageFilter>();
            DefaultTxTimeout = 50;
            DefaultRxTimeout = 250;
            DefaultTxFlag = J2534TXFLAG.NONE;
        }

        private void Connect()
        {
            Status = (J2534ERR)Device.Library.API.Connect(Device.DeviceID, (int)ProtocolID, (int)ConnectFlags, Baud, ref ChannelID);
        }

        public bool Disconnect()
        {
            Status = (J2534ERR)Device.Library.API.Disconnect(ChannelID);
            if (Status == J2534ERR.STATUS_NOERROR)
            {
                IsConnected = false;
                return false;
            }
            return true;
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
            int HeapBlobSize = J2534MessageSize * NumMsgs;
            IntPtr pHeapBlob = Marshal.AllocHGlobal(HeapBlobSize);

            Status = (J2534ERR)Device.Library.API.ReadMsgs(ChannelID, pHeapBlob, ref NumMsgs, Timeout);

            if (Status == J2534ERR.STATUS_NOERROR)
            {
                HeapBlobSize = J2534MessageSize * NumMsgs;
                RxMessages.Clear();
                for (uint p = (uint)pHeapBlob; p < (uint)pHeapBlob + HeapBlobSize; p += (uint)J2534MessageSize)
                    RxMessages.Add(Marshal.PtrToStructure<J2534Message>((IntPtr)p));
            }

            Marshal.FreeHGlobal(pHeapBlob);

            if(Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        /// <summary>
        /// Sends a single message 'Message'
        /// </summary>
        /// <param name="Message"></param>
        /// <returns>Returns 'false' if successful</returns>
        public bool SendMessage(List<byte> Message)
        {
            List<J2534Message> MessageList = new List<J2534.J2534Message>();
            MessageList.Add(new J2534.J2534Message(ProtocolID, DefaultTxFlag, Message));

            return SendMessages(MessageList);
        }

        /// <summary>
        /// Sends all messages contained in 'MsgList'
        /// </summary>
        /// <returns>Returns 'false' if successful</returns>
        public bool SendMessages(List<J2534Message> Messages)
        {
            int NumMsgs = Messages.Count;

            int HeapBlobSize = J2534MessageSize * NumMsgs;
            IntPtr pHeapBlob = Marshal.AllocHGlobal(HeapBlobSize);

            for (uint p = (uint)pHeapBlob, i = 0; p < (uint)pHeapBlob + HeapBlobSize; p += (uint)J2534MessageSize, i++)
                Marshal.StructureToPtr<J2534Message>(Messages[(int)i], (IntPtr)p, false);

            Status = (J2534ERR)Device.Library.API.WriteMsgs(ChannelID, pHeapBlob, ref NumMsgs, DefaultTxTimeout);

            Marshal.FreeHGlobal(pHeapBlob);

            //If some messages didnt get sent, put them into the RxMessages list
            //Not sure what else to do in this case.
            if(NumMsgs < Messages.Count)
            {
                Messages.RemoveRange(0, NumMsgs);
                RxMessages = Messages;
            }

            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool StartPeriodicMessage(J2534Message Message, int Interval)
        {
            PeriodicMsgList.Add(new PeriodicMsg(Message, Interval));

            //If success
            if (!StartPeriodicMsg(PeriodicMsgList.Count - 1))
                return false;
            //Otherwise, remove it from the list, and return fail
            PeriodicMsgList.RemoveAt(PeriodicMsgList.Count - 1);
            return true;
        }

        /// <summary>
        /// Starts the periodic message in 'PeriodicMsgList' referenced by 'Index'
        /// </summary>
        /// <param name="Index"></param>
        /// <returns>Returns 'false' if successful</returns>
        public bool StartPeriodicMsg(int Index)
        {
            IntPtr pHeapBlob = Marshal.AllocHGlobal(J2534MessageSize);
            Marshal.StructureToPtr<J2534Message>(PeriodicMsgList[Index].Message, pHeapBlob, false);

            Status = (J2534ERR)Device.Library.API.StartPeriodicMsg(ChannelID, pHeapBlob, ref PeriodicMsgList[Index].MessageID, PeriodicMsgList[Index].Interval);

            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        /// <summary>
        /// Stops the periodic message in 'PeriodicMsgList' referenced by 'Index'.
        /// </summary>
        /// <param name="Index"></param>
        /// <returns>Returns 'false' if successful</returns>
        public bool StopPeriodicMsg(int Index)
        {
            int MsgID = PeriodicMsgList[Index].MessageID;
            Status = (J2534ERR)Device.Library.API.StopPeriodicMsg(ChannelID, MsgID);
            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
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
                return true;
            }
            return false;
        }

        public bool StartMsgFilter(int Index)
        {
            IntPtr pMask = Marshal.AllocHGlobal(J2534MessageSize);
            IntPtr pPattern = Marshal.AllocHGlobal(J2534MessageSize);
            IntPtr pFlowControl = Marshal.AllocHGlobal(J2534MessageSize);

            Marshal.StructureToPtr<J2534Message>(new J2534Message(ProtocolID, FilterList[Index].TxFlags, FilterList[Index].Mask), pMask, false);
            Marshal.StructureToPtr<J2534Message>(new J2534Message(ProtocolID, FilterList[Index].TxFlags, FilterList[Index].Pattern), pPattern, false);
            Marshal.StructureToPtr<J2534Message>(new J2534Message(ProtocolID, FilterList[Index].TxFlags, FilterList[Index].FlowControl), pFlowControl, false);

            if (FilterList[Index].FilterType != J2534FILTER.FLOW_CONTROL_FILTER)
            {
                Marshal.FreeHGlobal(pFlowControl);
                pFlowControl = IntPtr.Zero;
            }

            Status = (J2534ERR)Device.Library.API.StartMsgFilter(ChannelID, (int)FilterList[Index].FilterType, pMask, pPattern, pFlowControl, ref FilterList[Index].FilterId);

            Marshal.FreeHGlobal(pMask);
            Marshal.FreeHGlobal(pPattern);
            Marshal.FreeHGlobal(pFlowControl);

            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool StopMsgFilter(int Index)
        {
            Status = (J2534ERR)Device.Library.API.StopMsgFilter(ChannelID, FilterList[Index].FilterId);
            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool GetConfig(J2534PARAMETER Parameter, ref int Value)
        {
            List<SConfig> SConfig = GetConfig(new List<SConfig>() { new J2534.SConfig(Parameter, 0) });
            if (SConfig.Count > 0)
                Value = SConfig[0].Value;

            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }
        public List<SConfig> GetConfig(List<SConfig> SConfig)
        {
            int SConfigSize = Marshal.SizeOf<SConfig>();
            int SConfigBlobSize = SConfigSize * SConfig.Count;

            IntPtr pSConfigList = Marshal.AllocHGlobal(Marshal.SizeOf<SConfigList>());
            IntPtr pSConfigBlob = Marshal.AllocHGlobal(SConfigBlobSize);

            Marshal.StructureToPtr<SConfigList>(new SConfigList(SConfig.Count(), pSConfigBlob), pSConfigList, false);
            for (uint p = (uint)pSConfigBlob, i = 0; p < (uint)pSConfigBlob + SConfigBlobSize; p += (uint)SConfigSize, i++)
                Marshal.StructureToPtr<SConfig>(SConfig[(int)i], (IntPtr)p, false);

            Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.GET_CONFIG, pSConfigList, IntPtr.Zero);

            SConfigList SConfigList = Marshal.PtrToStructure<SConfigList>(pSConfigList);
            List<SConfig> ReturnList = new List<SConfig>();

            SConfigBlobSize = SConfigSize * SConfigList.NumOfParams;
            for (uint p = (uint)SConfigList.pSConfig, i = 0; p < (uint)SConfigList.pSConfig + SConfigBlobSize; p += (uint)SConfigSize, i++)
                ReturnList.Add(Marshal.PtrToStructure<SConfig>((IntPtr)p));

            Marshal.FreeHGlobal(pSConfigBlob);
            Marshal.FreeHGlobal(pSConfigList);

            return ReturnList;
        }

        public bool SetConfig(J2534PARAMETER Parameter, int Value)
        {
            return SetConfig(new List<SConfig>() { new SConfig(Parameter, Value) });
        }

        public bool SetConfig(List<SConfig> SConfig)
        {
            int SConfigSize = Marshal.SizeOf<SConfig>();
            int SConfigBlobSize = SConfigSize * SConfig.Count;

            IntPtr pSConfigList = Marshal.AllocHGlobal(Marshal.SizeOf<SConfigList>());
            IntPtr pSConfigBlob = Marshal.AllocHGlobal(SConfigBlobSize);

            Marshal.StructureToPtr<SConfigList>(new SConfigList(SConfig.Count(), pSConfigBlob), pSConfigList, false);
            for (uint p = (uint)pSConfigBlob, i = 0; p < (uint)pSConfigBlob + SConfigBlobSize; p += (uint)SConfigSize, i++)
                Marshal.StructureToPtr<SConfig>(SConfig[(int)i], (IntPtr)p, false);

            Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.SET_CONFIG, pSConfigList, IntPtr.Zero);

            Marshal.FreeHGlobal(pSConfigList);
            Marshal.FreeHGlobal(pSConfigBlob);

            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool ClearTxBuffer()
        {
            Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.CLEAR_TX_BUFFER, IntPtr.Zero, IntPtr.Zero);
            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool ClearRxBuffer()
        {
            Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.CLEAR_RX_BUFFER, IntPtr.Zero, IntPtr.Zero);
            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool ClearPeriodicMsgs()
        {
            Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.CLEAR_PERIODIC_MSGS, IntPtr.Zero, IntPtr.Zero);
            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool ClearMsgFilters()
        {
            Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.CLEAR_MSG_FILTERS, IntPtr.Zero, IntPtr.Zero);
            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool ClearFunctMsgLookupTable()
        {
            Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.CLEAR_FUNCT_MSG_LOOKUP_TABLE, IntPtr.Zero, IntPtr.Zero);
            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool AddToFunctMsgLookupTable(byte Addr)
        {
            return AddToFunctMsgLookupTable(new List<byte>() { Addr });
        }

        public bool AddToFunctMsgLookupTable(List<byte> FuncAddr)
        {
            IntPtr input = Marshal.AllocHGlobal(Marshal.SizeOf<SByteArray>());
            IntPtr pFuncAddr = Marshal.AllocHGlobal(FuncAddr.Count);

            Marshal.StructureToPtr<SByteArray>(new SByteArray(FuncAddr.Count, pFuncAddr), input, false);
            Marshal.Copy(FuncAddr.ToArray(), 0, pFuncAddr, FuncAddr.Count);

            Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.ADD_TO_FUNCT_MSG_LOOKUP_TABLE, input, IntPtr.Zero);

            Marshal.FreeHGlobal(input);
            Marshal.FreeHGlobal(pFuncAddr);

            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool DeleteFromFunctMsgLookupTable(byte Addr)
        {
            return DeleteFromFunctMsgLookupTable(new List<byte>() { Addr });
        }

        public bool DeleteFromFunctMsgLookupTable(List<byte> FuncAddr)
        {
            IntPtr input = Marshal.AllocHGlobal(Marshal.SizeOf<SByteArray>());
            IntPtr pFuncAddr = Marshal.AllocHGlobal(FuncAddr.Count);

            Marshal.StructureToPtr<SByteArray>(new SByteArray(FuncAddr.Count, pFuncAddr), input, false);
            Marshal.Copy(FuncAddr.ToArray(), 0, pFuncAddr, FuncAddr.Count);

            Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.DELETE_FROM_FUNCT_MSG_LOOKUP_TABLE, input, IntPtr.Zero);

            Marshal.FreeHGlobal(input);
            Marshal.FreeHGlobal(pFuncAddr);

            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool FiveBaudInit(byte TargetAddress)
        {
            IntPtr input = Marshal.AllocHGlobal(Marshal.SizeOf<SByteArray>());
            IntPtr output = Marshal.AllocHGlobal(Marshal.SizeOf<SByteArray>());
            IntPtr pAddress = Marshal.AllocHGlobal(Marshal.SizeOf<byte>());
            IntPtr pKeywords = Marshal.AllocHGlobal(Marshal.SizeOf<byte>() * 2);

            Marshal.StructureToPtr<SByteArray>(new SByteArray(1, pAddress), input, false);
            Marshal.StructureToPtr<SByteArray>(new SByteArray(2, pKeywords), output, false);
            Marshal.WriteByte(pAddress, TargetAddress);

            Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.FIVE_BAUD_INIT, input, output);

            if (Status == J2534ERR.STATUS_NOERROR)
            {
                SByteArray SByteArray = Marshal.PtrToStructure<SByteArray>(output);    
                if(SByteArray.NumOfBytes == 2)
                {
                    FiveBaudKeyword1 = Marshal.ReadByte(SByteArray.Pointer, 0);
                    FiveBaudKeyword2 = Marshal.ReadByte(SByteArray.Pointer, 1);
                }
            }

            Marshal.FreeHGlobal(input);
            Marshal.FreeHGlobal(output);
            Marshal.FreeHGlobal(pAddress);
            Marshal.FreeHGlobal(pKeywords);

            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
        }

        public bool FastInit(J2534Message TxMessage)
        {

            IntPtr input = Marshal.AllocHGlobal(Marshal.SizeOf<J2534Message>());
            IntPtr output = Marshal.AllocHGlobal(Marshal.SizeOf<J2534Message>());

            Marshal.StructureToPtr<J2534Message>(TxMessage, input, false);

            Status = (J2534ERR)Device.Library.API.IOCtl(ChannelID, (int)J2534IOCTL.FAST_INIT, input, output);

            if (Status == J2534ERR.STATUS_NOERROR)
            {                
                RxMessages.Clear();
                RxMessages.Add(Marshal.PtrToStructure<J2534Message>(output));
            }

            Marshal.FreeHGlobal(input);
            Marshal.FreeHGlobal(output);

            if (Status == J2534ERR.STATUS_NOERROR)
                return false;
            return true;
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
