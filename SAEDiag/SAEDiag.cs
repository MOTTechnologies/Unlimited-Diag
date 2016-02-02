using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J2534DotNet;

namespace SAEDiag
{
    public class SAEDiag : SAE_data
    {
        private IJ2534 j2534_interface;

        public ProtocolID protocol {get; set;};
        public int channel_id {get; set;};

        public SAEDiag(IJ2534 j2534Interface)
        {
            j2534_interface = j2534Interface;
            protocol = ProtocolID.ISO15765;
            status = J2534Err.STATUS_NOERROR;
        }

        private SAE_data get_pid(byte mode, byte pid)
        {            

            byte[] rec_msg = new byte[8];
            SAE_data pid_msg = new SAE_data();

            switch (protocol)
            {
                case ProtocolID.CAN:
                case ProtocolID.ISO15765:

                    break;
                case ProtocolID.J1850PWM:
                    break;
                case ProtocolID.ISO9141:
                    break;
                default:
                    break;

                    m_status = m_j2534Interface.WriteMsgs(m_channelId, ref txMsg, ref numMsgs, timeout);
            if (J2534Err.STATUS_NOERROR != m_status)
            {
                return false;
            }

            numMsgs = 1;
            while (J2534Err.STATUS_NOERROR == m_status)
	        {
                m_status = m_j2534Interface.ReadMsgs(m_channelId, ref rxMsgs, ref numMsgs, timeout * 4);
	        }

            if (J2534Err.ERR_BUFFER_EMPTY == m_status || J2534Err.ERR_TIMEOUT == m_status)
            {
                if (rxMsgs.Count > 1)
                {
                    // Select the last value
                    value = rxMsgs[rxMsgs.Count - 1].Data.ToList();
                    value.RemoveRange(0, txMsg.Data.Length);
                    return true;
                }
                return false;
            }
            return false;
            }
            pid_msg.pid = (int)rec_msg[0];
            Array.Copy(rec_msg, 1, pid_msg.data, 0, rec_msg.Length - 1);

            return pid_msg;
        }
        public List<SAE_data> GetAllPids(int mode)      //Request all supported PIDs of a SAE mode
        {
            List<SAE_data> results = new List<SAE_data>;
            int mask;

            results.Add(get_pid((byte) mode, 0x00));      //Issue a base MODE request

            int ii = 1;
            int max_count = 32;
            do
            {
                mask = BitConverter.ToInt32(results.Last().data,0);        //Should check for endianess here...
                if (mask > 0)
                {
                    if(ii > 224)    //Pid numbers are 8bits so this prevents overflow.  Should never get here, added for robustness
                        max_count = 31;

                    for (byte i = 0; i < max_count; i++)
                    {
                        if (IsBitSet(mask, i))
                        {
                            results.Add(get_pid((byte) mode, (byte) (i + ii)));
                        }
                    }
                    ii += 32;
                }
            } while (IsBitSet(mask, 31) & (ii < 255));

            return results;
        }

        bool IsBitSet(byte b, byte pos)
        {
            return (b & (1 << pos)) != 0;
        }
        bool IsBitSet(Int16 b, byte pos)
        {
            return (b & (1 << pos)) != 0;
        }
        bool IsBitSet(Int32 b, byte pos)
        {
            return (b & (1 << pos)) != 0;
        }
    }
}
