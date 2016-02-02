using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAEDiag
{
    class SAE_data
    {
        public int mode;
        public int pid { get; set; }
        public byte[] data;
        public SAE_data()
        {
            mode = 0;
            pid = 0;
            data = new byte[8];
        }
    }
}
