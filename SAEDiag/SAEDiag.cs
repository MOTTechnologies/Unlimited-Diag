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
            CANSAEMessage.tx_header = new List<byte> { 0x00, 0x00, 0x07, 0xE0 };
            CANSAEMessage.rx_header = new List<byte> { 0x00, 0x00, 0x07, 0xE8 };

            channel.SendMessage(CANSAEMessage.Send(SAEModes.REQ_DIAG_DATA, 0x00));
            for(int i = 0;i < 5; i++)
            {
                if (CANSAEMessage.Receive(channel.GetMessage()))
                    break;
            }
            return !CANSAEMessage.Failure;
        }

        //public bool SendSAEMsg(SAEMessage Msg)
        //{
        //    Channel.SendMessage(Msg);

        //    return true;    //failure
        //}
        //private SAE_data get_pid(byte mode, byte pid)
        //{            

        //    byte[] rec_msg = new byte[8];
        //    SAE_data pid_msg = new SAE_data();
        //    J2534Err status;
        //    PassThruMsg tx_msg, rx_msg = new PassThruMsg();
        //    int num_msgs;
 
        //    switch (protocol)
        //    {
        //        case ProtocolID.CAN:
        //        case ProtocolID.ISO15765:

        //            break;
        //        case ProtocolID.J1850PWM:
        //            break;
        //        case ProtocolID.ISO9141:
        //            break;
        //        default:
        //            break;

        //    status = j2534_interface.WriteMsgs(channel_id, ref tx_msg, 500);

        //    if (J2534Err.STATUS_NOERROR != status)
        //    {
        //        //TODO: Throw exception or something...
        //        return pid_msg;
        //    }

        //    //**********************************************************************************
        //    //The following read block should be moved into a generic read function that caches
        //    //old messages that dont match a case by case filter.
        //    //This will pave the way for multiple threads interacting simultaniously with the vehicle.
        //    //Application for this would be simultainous datalogging of both PCM and TCM
        //    //by independant threads.  Same goes for reading and writing.  Reflash both modules
        //    //at the same time!
        //    //This method should probably reside in the J2534 class
        //    //***********************************************************************************

        //    status = j2534_interface.ReadMsgs(channel_id, ref rx_msg, 500);

        //    if (status != J2534Err.STATUS_NOERROR)
        //    {
        //        //TODO: Throw exception or something...
        //        return pid_msg;
        //    }


        //    if (J2534Err.ERR_BUFFER_EMPTY == status || J2534Err.ERR_TIMEOUT == m_status)
        //    {
        //        if (rxMsgs.Count > 1)
        //        {
        //            // Select the last value
        //            value = rxMsgs[rxMsgs.Count - 1].Data.ToList();
        //            value.RemoveRange(0, txMsg.Data.Length);
        //            return true;
        //        }
        //        return false;
        //    }
        //    return false;
        //    }
        //    pid_msg.pid = (int)rec_msg[0];
        //    Array.Copy(rec_msg, 1, pid_msg.data, 0, rec_msg.Length - 1);

        //    return pid_msg;
        //}
        //public List<SAE_data> GetAllPids(int mode)      //Request all supported PIDs of a SAE mode
        //{
        //    List<SAE_data> results = new List<SAE_data>;
        //    int mask;

        //    results.Add(get_pid((byte) mode, 0x00));      //Issue a base MODE request

        //    int ii = 1;
        //    int max_count = 32;
        //    do
        //    {
        //        mask = BitConverter.ToInt32(results.Last().data,0);        //Should check for endianess here...
        //        if (mask > 0)
        //        {
        //            if(ii > 224)    //Pid numbers are 8bits so this prevents overflow.  Should never get here, added for robustness
        //                max_count = 31;

        //            for (byte i = 0; i < max_count; i++)
        //            {
        //                if (IsBitSet(mask, i))
        //                {
        //                    results.Add(get_pid((byte) mode, (byte) (i + ii)));
        //                }
        //            }
        //            ii += 32;
        //        }
        //    } while (IsBitSet(mask, 31) & (ii < 255));

        //    return results;
        //}

        //bool IsBitSet(byte b, byte pos)
        //{
        //    return (b & (1 << pos)) != 0;
        //}
        //bool IsBitSet(Int16 b, byte pos)
        //{
        //    return (b & (1 << pos)) != 0;
        //}
        //bool IsBitSet(Int32 b, byte pos)
        //{
        //    return (b & (1 << pos)) != 0;
        //}
    }
}
