using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J2534DotNet;

namespace SAE
{
    public class SAEDiag
    {
        public Channel Channel;

        public bool Ping(Channel channel)
        {
            SAEMessage CANSAEMessage = new SAEMessage(new List<byte>{ 0x00, 0x00, 0x07, 0xE0 }, new List<byte> { 0x00, 0x00, 0x07, 0xE8});

            channel.SendMessage(CANSAEMessage.Send(SAEModes.REQ_DIAG_DATA, 0x00));
            for(int i = 0;i < 5; i++)
            {
                if (CANSAEMessage.Receive(channel.GetMessage()))
                    break;
            }
            return !CANSAEMessage.Failure;
        }
        public string GetVIN(Channel Channel)
        {
            SAEMessage CANSAEMessage = new SAEMessage(new List<byte> { 0x00, 0x00, 0x07, 0xE0 }, new List<byte> { 0x00, 0x00, 0x07, 0xE8 });

            Channel.SendMessage(CANSAEMessage.Send(SAEModes.REQ_VEHICLE_INFO, 0x02));
            for (int i = 0; i < 5; i++)
            {
                if (CANSAEMessage.Receive(Channel.GetMessage()))
                    break;
            }
            return CANSAEMessage.rx_message.d
        }
    }
}
