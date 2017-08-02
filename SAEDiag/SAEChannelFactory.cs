using J2534;


namespace SAE
{
    class SAEChannelFactory
    {
        private int enumerator_max = 4;
        private int channel_enumerator = -1;
        private J2534Device device;

        public SAEChannelFactory(J2534Device Device)
        {
            device = Device;
        }

        public Channel NextChannel()
        {
            Channel next_channel;
            //non standard usage of for loop construct.  It works....
            for(channel_enumerator++; channel_enumerator < enumerator_max; channel_enumerator++)
            {
                switch (channel_enumerator)
                {
                    case 0:
                        next_channel = device.ConstructChannel(J2534PROTOCOL.ISO15765, J2534BAUD.ISO15765, J2534CONNECTFLAG.NONE);
                        if (next_channel.IsOpen)
                            return next_channel;
                        break;
                    case 1:
                        next_channel = device.ConstructChannel(J2534PROTOCOL.J1850PWM, J2534BAUD.J1850PWM, J2534CONNECTFLAG.NONE);
                        if (next_channel.IsOpen)
                            return next_channel;
                        break;
                    case 2:
                        next_channel = device.ConstructChannel(J2534PROTOCOL.J1850VPW, J2534BAUD.J1850VPW, J2534CONNECTFLAG.NONE);
                        if (next_channel.IsOpen)
                            return next_channel;
                        break;
                    case 3:
                    default:
                        //Should I include the 5 baud init here?
                        next_channel = device.ConstructChannel(J2534PROTOCOL.ISO9141, J2534BAUD.ISO9141, J2534CONNECTFLAG.NONE);
                        if (next_channel.IsOpen)
                            return next_channel;
                        break;
                }
            }
            return null;
        }
    }
}
