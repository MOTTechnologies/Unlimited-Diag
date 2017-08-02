using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J2534;

namespace SAE
{
    public class SAEDiag
    {
        public Channel Channel;

        public bool Ping(Channel channel)
        {
            //SAEMessage CANSAEMessage = new SAEMessage(new List<byte>{ 0x00, 0x00, 0x07, 0xE0 }, new List<byte> { 0x00, 0x00, 0x07, 0xE8});
            SAEMessage Msg = new SAEMessage(new byte[] { 0x00, 0x00, 0x07, 0xE0, 0x01, 0x00 }, channel.ProtocolID);

            //channel.SendMessage(Msg);
            //for(int i = 0;i < 5; i++)
            //{
            //    if (!channel.GetMessage())
            //    {
            //        J2534Message mESSAGE = channel.HeapMessageArray[0];
            //    }
            //    if (CANSAEMessage.Receive(channel.RxMessages[0].Data))
                   
            //}
            //return !CANSAEMessage.Failure;
            return true;
        }
        //public string GetVIN(Channel Channel)
        //{
        //    SAEMessage CANSAEMessage = new SAEMessage(new List<byte> { 0x00, 0x00, 0x07, 0xE0 }, new List<byte> { 0x00, 0x00, 0x07, 0xE8 });

        //    Channel.SendMessage(CANSAEMessage.Send(SAEModes.REQ_VEHICLE_INFO, 0x02));
        //    for (int i = 0; i < 5; i++)
        //    {
        //        if (CANSAEMessage.Receive(Channel.GetMessage()))
        //            break;
        //    }
        //    return CANSAEMessage.rx_message.
        //}
    }
}
