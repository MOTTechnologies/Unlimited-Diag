using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J2534DotNet;

namespace SAEDiag
{
    public class SAEDiag : SAEData
    {

        private pid_data_type get_pid(byte mode, byte pid)
        {
            ProtocolID Protocol;

            byte[] rec_msg = new byte[8];
            pid_data_type pid_msg = new pid_data_type();

            switch (Protocol)
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
            }
            pid_msg.pid = (int)rec_msg[0];
            Array.Copy(rec_msg, 1, pid_msg.data, 0, rec_msg.Length - 1);

            return pid_msg;
        }
        public SAEData GetAllPids(int channel_id, int mode)
        {
            SAEData results = new SAEData();
            pid_data_type pid;
            int mask;

            pid = get_pid((byte) mode, 0x00);      //Issue a base MODE 9 request
            results.mode = mode;            //Store the mode of the results
            results.pid_data.Add(pid);      //store the result of the base request

            int ii = 1;
            do
            {
                mask = BitConverter.ToInt32(pid.data,0);        //Should check for endianess here...
                if (mask > 0)
                {
                    for (byte i = 0; i < 32; i++)
                    {
                        if (IsBitSet(mask, i))
                        {
                            pid = get_pid((byte) mode, (byte) (i + ii));
                            results.pid_data.Add(pid);
                        }
                    }
                    ii += 32;
                }
            } while (IsBitSet(mask, 31));

            return results;
        }

        bool IsBitSet(byte b, byte pos)
        {
            return (b & (1 << pos)) != 0;
        }
        bool IsBitSet(int16 b, byte pos)
        {
            return (b & (1 << pos)) != 0;
        }
        bool IsBitSet(Int32 b, byte pos)
        {
            return (b & (1 << pos)) != 0;
        }
    }
}
