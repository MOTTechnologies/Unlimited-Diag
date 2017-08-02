using System.Collections.Generic;
using J2534;
namespace SAE.Session
{
    abstract class SAE_Session
    {
        private Channel Channel;
        private object resource_lock;
        private int this_physical_address;

        public SAEMessage SAETxRx(int Addr, byte Mode, byte[] Params, byte[] Data)
        {
            TxMessage.SAEMode = Mode;
            TxMessage.PID = PID_record_num;
            TxMessage.TargetAddress = Module.Address;
            Results = session_channel.MessageTransaction(TxMessage, 1, TxMessage.DefaultRxComparer);
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
