using System;
using System.Collections.Generic;
using System.Linq;
using J2534;

namespace SAE.Session
{
    class ISO15765_Session : ISAESession
    {
        private Channel channel;
        private J2534Device device;
        private object resource_lock;
        private int this_physical_address;

        public ISO15765_Session(J2534Device Device)
        {
            this.device = Device;
            channel = device.ConstructChannel(J2534PROTOCOL.ISO15765, J2534BAUD.ISO15765, J2534CONNECTFLAG.NONE);
            if (channel.IsOpen)    //If channel is constructed successfully and is live
            {
                InitializeDefaultConfigs();
            }
        }

        private void InitializeDefaultConfigs()
        {
            channel.ClearMsgFilters();
            for (int i = 0; i < 8; i++)
                channel.StartMsgFilter(new MessageFilter(COMMONFILTER.STANDARDISO15765,
                                                         new byte[4] { 0x00, 0x00, 0x07, (byte)(0xE0 + i) }));
            channel.SetConfig(J2534PARAMETER.LOOP_BACK, 0);
        }

        public SAEMessage SAETxRx(int Addr, byte Mode, byte[] Params, byte[] Data)
        {
            GetMessageResults Results;
            object _handle = CreateRxHandle(Addr, Mode, Params);

            TxMessage.SAEMode = Mode;
            TxMessage.PID = PID_record_num;
            TxMessage.TargetAddress = Module.Address;
            Results = channel.MessageTransaction(TxMessage, 1, TxMessage.DefaultRxComparer);
            if (Results.Status == J2534ERR.STATUS_NOERROR)
                Module.Parse_PID_Validation_Bytes(PID_record_num, Results.Messages[0].Data);

        }
        public void SAETx(int Addr, byte Mode, List<byte[]> Data)
        {

        }
        public List<byte[]> SAERx(int Addr, byte Mode, int NumOfMsgs)
        {

        }
        public object CreateRxHandle(int Addr, byte Mode, byte[] Params)
        {

        }
        public List<byte[]> SAERx(object RxHandle, int NumOfMsgs, int Timeout, bool DestroyHandle)
        {

        }

    }
}
