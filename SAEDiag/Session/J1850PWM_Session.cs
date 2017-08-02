using System;
using System.Collections.Generic;
using J2534;

namespace SAE.Session
{
    class J1850PWM_Session : J1850Session , ISAESession
    {
        private J2534Device device;
        private object resource_lock;
        private int this_physical_address;

        public J1850PWM_Session(J2534Device Device)
        {
            this.device = Device;
            channel = device.ConstructChannel(J2534PROTOCOL.J1850PWM, J2534BAUD.J1850PWM, J2534CONNECTFLAG.NONE);
            if (channel.IsOpen)    //If channel is constructed successfully and is open 
            {
                InitializeDefaultConfigs();
            }
        }

        private void InitializeDefaultConfigs()
        {
            J1850Message default_message_builder = new J1850Message();
            default_message_builder.J1850_byte0 = 0xC4;
            default_message_prototype = default_message_builder.RawMessage;
            channel.ClearMsgFilters();
            channel.ClearFunctMsgLookupTable();
            channel.AddToFunctMsgLookupTable(0x6B);
            channel.StartMsgFilter(new MessageFilter()
            {
                Mask = new byte[] { 0x00, 0xFF, 0x00 },
                Pattern = new byte[] { 0x00, 0xF1, 0x00 },
                FilterType = J2534FILTER.PASS_FILTER
            });
            ToolAddress = 0xF1;
        }

        private int ToolAddress
        {
            get
            {
                return this_physical_address;
            }
            set
            {
                this_physical_address = value;
                channel.SetConfig(J2534PARAMETER.NODE_ADDRESS, (byte)this_physical_address);
                J1850Message default_message = new J1850Message(default_message_prototype);
                default_message.SourceAddress = this_physical_address;
                default_message_prototype = default_message.RawMessage;
            }
        }
    }
}
