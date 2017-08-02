using System;

namespace SAE
{
    class SAEMessage
    {
        public int Addr { get; set; }
        public SAEModes Mode { get; set; }
        public byte[] Data { get; set; }
        public bool IsValid { get; set; }

        public SAEMessage()
        {
            Mode = new SAEModes();
            Data = Array.Empty<byte>();
            IsValid = false;
        }
    }
}
