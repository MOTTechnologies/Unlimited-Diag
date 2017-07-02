namespace J2534
{
    public class PeriodicMsg
    {
        public J2534Message Message { get; set; }
        public int Interval { get; set; }
        internal int MessageID;
        public PeriodicMsg(J2534Message Message, int Interval)
        {
            this.Message = Message;
            this.Interval = Interval;
        }
    }
}
