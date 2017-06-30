namespace J2534
{
    internal class API_SIGNATURE
    {
        public API_SIGNATURE()
        {
            this.SAE_API = J2534.SAE_API.NONE;
            this.DREWTECH_API = J2534.DREWTECH_API.NONE;
        }
        public SAE_API SAE_API { get; set; }
        public DREWTECH_API DREWTECH_API { get; set; }
    }
}
