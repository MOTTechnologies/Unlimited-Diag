using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAEDiag
{
    class pid_data_type
    {
        public int pid { get; set; }
        public byte[] data;
        public pid_data_type()
        {
            pid = 0;
            data = new byte[8];
        }
    }
    class SAEData
    {
        public List<pid_data_type> pid_data;        
        public int mode { get; set; }

        public SAEData()
        {
            mode = 0;
            pid_data = new List<pid_data_type>();
        }

    }
}
