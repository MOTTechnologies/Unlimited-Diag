using System;
using System.Collections.Generic;
using J2534;

namespace SAE.Session
{
    //Base class that all J1850 sessions are built from
    abstract class J1850Session
    {
        protected Channel channel;
        protected byte[] default_message_prototype;

        public SAEMessage SAETxRx(SAEMessage Message, int RxDataIndex)
        {
            J1850Message message = new J1850Message(default_message_prototype);
            GetMessageResults Results;
            SAEMessage ReturnMessage = new SAEMessage();

            message.TargetAddress = Message.Addr;
            message.SAEMode = Message.Mode;
            message.Data = Message.Data;
            message.RxDataIndex = RxDataIndex;

            Results = channel.MessageTransaction(message.RawMessage, 1, message.DefaultRxComparer);
            if (Results.Status == J2534ERR.STATUS_NOERROR)
            {
                message.RawMessage = Results.Messages[0].Data;
                ReturnMessage.Mode = message.SAEMode;
                ReturnMessage.Data = message.Data;
                ReturnMessage.IsValid = true;
            }
            return ReturnMessage;
        }

        public void SAETx(int Addr, byte Mode, List<byte[]> Data)
        {
            throw new NotImplementedException();
        }

        public List<byte[]> SAERx(int Addr, byte Mode, int NumOfMsgs)
        {
            throw new NotImplementedException();
        }

        public object CreateRxHandle(int Addr, byte Mode, byte[] Params)
        {
            throw new NotImplementedException();
        }

        public void DestroyRxHandle(object Handle)
        {
            throw new NotImplementedException();
        }

        public List<byte[]> SAERx(object RxHandle, int NumOfMsgs, int Timeout, bool DestroyHandle)
        {

        }
    }
}
