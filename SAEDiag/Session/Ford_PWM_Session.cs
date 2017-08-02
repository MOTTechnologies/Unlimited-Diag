using J2534;

namespace SAE.Session
{
    class FordPWMSession : J1850PWM_Session
    {
        public FordPWMSession(J2534Device Device) : base(Device)
        {
        }
    }
}
