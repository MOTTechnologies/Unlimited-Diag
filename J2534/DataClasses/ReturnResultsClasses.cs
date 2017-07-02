using System.Collections.Generic;

namespace J2534
{
    public class GetMessageResults
    {
        public J2534ERR Status { get; set; }
        public List<J2534Message> Messages;

        public GetMessageResults()
        {
            Messages = new List<J2534Message>();
        }

        public GetMessageResults(J2534ERR Status)
        {
            Messages = new List<J2534Message>();
            this.Status = Status;
        }
        public GetMessageResults(List<J2534Message> Messages, J2534ERR Status)
        {
            this.Status = Status;
            this.Messages = Messages;
        }
    }

    public class GetConfigResults
    {
        public J2534ERR Status { get; set; }
        public int Value { get; set; }
        public int Parameter { get; set; }
    }

    internal class GetNextCarDAQResults
    {
        public J2534ERR Status { get; set; }
        public bool Exists { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Address { get; set; }
    }
    public class J2534RegisteryEntry
    {
        public string Vendor { get; set; }
        public string Name { get; set; }
        public string FunctionLibrary { get; set; }
        public string ConfigApplication { get; set; }
        public int CANChannels { get; set; }
        public int ISO15765Channels { get; set; }
        public int J1850PWMChannels { get; set; }
        public int J1850VPWChannels { get; set; }
        public int ISO9141Channels { get; set; }
        public int ISO14230Channels { get; set; }
        public int SCI_A_ENGINEChannels { get; set; }
        public int SCI_A_TRANSChannels { get; set; }
        public int SCI_B_ENGINEChannels { get; set; }
        public int SCI_B_TRANSChannels { get; set; }

        public bool IsCANSupported
        {
            get { return (CANChannels > 0 ? true : false); }
        }

        public bool IsISO15765Supported
        {
            get { return (ISO15765Channels > 0 ? true : false); }
        }

        public bool IsJ1850PWMSupported
        {
            get { return (J1850PWMChannels > 0 ? true : false); }
        }

        public bool IsJ1850VPWSupported
        {
            get { return (J1850VPWChannels > 0 ? true : false); }
        }

        public bool IsISO9141Supported
        {
            get { return (ISO9141Channels > 0 ? true : false); }
        }

        public bool IsISO14230Supported
        {
            get { return (ISO14230Channels > 0 ? true : false); }
        }

        public bool IsSCI_A_ENGINESupported
        {
            get { return (SCI_A_ENGINEChannels > 0 ? true : false); }
        }

        public bool IsSCI_A_TRANSSupported
        {
            get { return (SCI_A_TRANSChannels > 0 ? true : false); }
        }

        public bool IsSCI_B_ENGINESupported
        {
            get { return (SCI_B_ENGINEChannels > 0 ? true : false); }
        }

        public bool IsSCI_B_TRANSSupported
        {
            get { return (SCI_B_TRANSChannels > 0 ? true : false); }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
