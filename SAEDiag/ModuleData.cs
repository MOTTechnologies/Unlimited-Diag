using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAE
{
    public class ModuleData
    {
        public int Address { get; set; }
        public string Name { get; set; }
        public List<PID> ValidatedPIDS;

        public ModuleData(int Address)
        {
            this.Address = Address;
            ValidatedPIDS = new List<PID>();
        }

        public void PID_Validation_Bytes(byte Range, byte[] bytes)
        {
            int validation_bits = bytes[0] << 24;
            validation_bits += bytes[1] << 16;
            validation_bits += bytes[2] << 8;
            validation_bits += bytes[3];

            for (int i = 31; i > -1; i--)
            {
                if(((validation_bits >> i) & 0x01) == 0x01)
                {
                    if(!ValidatedPIDS.Where(p => p.Number == ((32 - i) + Range)).Any())    //Verify no duplicate insertions
                    {
                        ValidatedPIDS.Add(new PID((32 - i) + Range));
                    }
                }
            }

        }
    }

    public class PID
    {
        public string Name { get; set; }
        public int Number { get; set; }
        public bool Signed { get; set; }
        public int  Size { get; set; }
        public int BP { get; set; }
        public float Multiplier { get; set; }
        public float Offset { get; set; }

        public PID(int PID_Number)
        {
            Number = PID_Number;
        }
    }
}
