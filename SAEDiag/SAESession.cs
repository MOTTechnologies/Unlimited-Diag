using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J2534;

namespace SAE
{
    class SAESession
    {
        J2534PhysicalDevice J2534Device;

        public SAESession(J2534PhysicalDevice J2534Device)
        {
            this.J2534Device = J2534Device;
            AutoDetectNetwork();
        }

        public void AutoDetectNetwork()
        {

        }
    }
}
