using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;

namespace J2534
{
    public class J2534Exception : Exception
    {
        public bool device_error { get; private set; }
        public bool parameter_error { get; private set; }
        public bool communication_error { get; private set; }
        public bool general_error { get; private set; }
        public bool error_is_fatal { get; private set; }
        public bool error_is_hardware { get; private set; }
        public string Status_Description { get; private set; }
        private J2534ERR status;

        public J2534Exception()
        {
            Status = 0;
        }

        public J2534Exception(J2534ERR Status)
        {
            this.Status = Status;
        }

        public J2534Exception(J2534ERR Status, string Message) : base(Message)
        {
            this.Status = Status;

        }

        public J2534Exception(J2534ERR Status, string Message, Exception Inner) : base(Message, Inner)
        {
            this.Status = Status;
        }

        public J2534ERR Status
        {
            get
            {
                return Status;
            }
            private set
            {
                status = value;
                ParseError(status);
            }
        }

        private void ParseError(J2534ERR Status)
        {
            switch (Status)
            {
                case J2534ERR.NOT_SUPPORTED:        //0x01
                case J2534ERR.INVALID_CHANNEL_ID:   //0x02
                case J2534ERR.INVALID_PROTOCOL_ID:  //0x03
                case J2534ERR.NULL_PARAMETER:       //0x04
                case J2534ERR.INVALID_IOCTL_VALUE:  //0x05
                case J2534ERR.INVALID_FLAGS:        //0x06
                case J2534ERR.INVALID_MSG:          //0x0A
                case J2534ERR.INVALID_TIME_INTERVAL://0x0B
                case J2534ERR.EXCEEDED_LIMIT:       //0x0C
                case J2534ERR.INVALID_MSG_ID:       //0x0D
                case J2534ERR.INVALID_IOCTL_ID:     //0x0F
                case J2534ERR.PIN_INVALID:          //0x13
                case J2534ERR.CHANNEL_IN_USE:       //0x14
                case J2534ERR.MSG_PROTOCOL_ID:      //0x15
                case J2534ERR.INVALID_FILTER_ID:    //0x16
                case J2534ERR.NO_FLOW_CONTROL:      //0x17
                case J2534ERR.NOT_UNIQUE:           //0x18
                case J2534ERR.INVALID_BAUDRATE:     //0x19
                case J2534ERR.INVALID_DEVICE_ID:    //0x1A
                    {
                        error_is_fatal = parameter_error = true;
                        error_is_hardware = false;
                        break;
                    }
                case J2534ERR.DEVICE_NOT_CONNECTED: //0x08
                case J2534ERR.DEVICE_IN_USE:        //0x0E
                    {
                        error_is_hardware = error_is_fatal = device_error = true;
                        break;
                    }
                case J2534ERR.TIMEOUT:              //0x09
                case J2534ERR.BUFFER_EMPTY:         //0x10
                case J2534ERR.BUFFER_FULL:          //0x11
                case J2534ERR.BUFFER_OVERFLOW:      //0x12
                    {
                        communication_error = true;
                        error_is_hardware = error_is_fatal = false; //These are the only recoverable errors
                        break;
                    }
                case J2534ERR.FUNCTION_NOT_ASSIGNED://non-standard flag used by the wrapper to indicate no function assigned
                case J2534ERR.FAILED:               //0x07
                    {
                        error_is_fatal = general_error = true;
                        error_is_hardware = false;
                        break;
                    }
                default:
                    {
                        error_is_fatal = general_error = true;
                        error_is_hardware = false;
                        break;
                    }
            }

            //Use reflection to retrieve the Description attribute from the enum member contained in 'Status'
            MemberInfo[] memberInfo = typeof(J2534ERR).GetMember(Status.ToString());
            if (memberInfo != null && memberInfo.Length > 0)
            {
                object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attrs != null && attrs.Length > 0)
                    Status_Description = ((DescriptionAttribute)attrs[0]).Description;
                else
                    Status_Description = Status.ToString();
            }
        }
    }
}
