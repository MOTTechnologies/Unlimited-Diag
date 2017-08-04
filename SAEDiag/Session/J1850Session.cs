using System;
using System.Collections.Generic;
using J2534;

namespace SAE.Session
{
    //Base class that all J1850 sessions are built from
    abstract class J1850Session
    {
        protected J2534Device device;
        protected Channel channel;
        protected J2534PROTOCOL SessionProtocol;
        protected J2534TXFLAG SessionTxFlags;

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
            J1850Message message = new J1850Message(default_message_prototype);
            message.TargetAddress = Addr;
            message.SAEMode = Mode;

            List<J2534Message> J2534Messages = new List<J2534Message>();

            Data.ForEach(data =>
            {
                message.Data = data;
                J2534Messages.Add(new J2534Message(SessionProtocol, SessionTxFlags, message.RawMessage));
            });

            J2534ERR Status = channel.SendMessages(J2534Messages);
            if(Status != J2534ERR.STATUS_NOERROR)
            {
                throw new J2534Exception(Status, channel.GetLastError());
            }
        }

        public object CreateRxHandle(int Addr, byte Mode, byte[] Params)
        {
            J1850Message message = new J1850Message(default_message_prototype);
            message.TargetAddress = Addr;
            message.SAEMode = Mode;
            message.Data = Params;
            message.RxDataIndex = Params.Length;

            return message.DefaultRxComparer;
        }

        public void DestroyRxHandle(object Handle)
        {
            throw new NotImplementedException();
        }

        public List<byte[]> SAERx(object RxHandle, int NumOfMsgs, int Timeout, bool DestroyHandle)
        {
            throw new NotImplementedException();
        }
    }
}
