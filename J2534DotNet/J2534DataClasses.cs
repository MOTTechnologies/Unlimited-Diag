using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

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

    internal class Seive
    {
        private object LOCK = new object();
        private List<SeiveScreen> Screens = new List<SeiveScreen>();

        public void Add(int Priority, Predicate<J2534Message> Predicate)
        {
            lock (LOCK)
            {
                Screens.Add(new SeiveScreen(Priority, Predicate));
                Screens.Sort((S1, S2) => { return S1.Priority - S2.Priority; });
            }
        }

        public void Remove(Predicate<J2534Message> ComparerAsKey)
        {
            lock (LOCK)
                Screens.Remove(Screens.Find(screen => screen.Comparer == ComparerAsKey));
        }

        public void Extract(List<J2534Message> Messages)
        {
            Messages.ForEach(Message =>
            {
                lock(LOCK)
                    foreach (SeiveScreen Screen in Screens)
                    {
                        if (Screen.Comparer(Message))
                        {
                            Screen.Messages.Add(Message);
                            break;
                        }
                    }
            });
        }

        public int Count(Predicate<J2534Message> ComparerAsKey)
        {
            lock (LOCK) //This will throw an exception if predicate is not found.  That is probably best.
                return Screens.Find(Screen => Screen.Comparer == ComparerAsKey).Messages.Count;
        }

        public List<J2534Message> Withdraw(Predicate<J2534Message> ComparerAsKey, bool Remove)
        {
            lock (LOCK)
            {
                SeiveScreen Screen = Screens.Find(screen => screen.Comparer == ComparerAsKey);
                if (Remove)
                    Screens.Remove(Screen);
                else
                    Screens.Find(screen => screen.Comparer == ComparerAsKey).Messages = new List<J2534Message>();
                return Screen.Messages;
            }
        }
    }

    internal class SeiveScreen
    {
        public int Priority { get; set; }
        public List<J2534Message> Messages = new List<J2534Message>();
        public Predicate<J2534Message> Comparer;
        public SeiveScreen(int Priority, Predicate<J2534Message> Comparer)
        {
            this.Priority = Priority;
            this.Comparer = Comparer;
        }
    }

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

    public class J2534Message
    {
        public J2534PROTOCOL ProtocolID;
        public J2534RXFLAG RxStatus;
        public J2534TXFLAG TxFlags;
        public uint Timestamp;
        public uint ExtraDataIndex;
        public byte[] Data;

        public J2534Message()
        {
            Data = Array.Empty<byte>();
        }

        public J2534Message(J2534PROTOCOL ProtocolID, J2534TXFLAG TxFlags, byte[] Data)
        {
            this.ProtocolID = ProtocolID;
            this.TxFlags = TxFlags;
            if (Data == null)
                this.Data = Array.Empty<byte>();
            else
                this.Data = Data;
        }
    }

    public class J2534HeapMessageArray:IDisposable
    {
        private int array_max_length;
        private IntPtr pMessages;
        private IntPtr pNumMsgs;
        private bool disposed;

        public J2534HeapMessageArray(int Length)
        {
            array_max_length = Length;
            pNumMsgs = Marshal.AllocHGlobal(4);
            pMessages = Marshal.AllocHGlobal(CONST.J2534MESSAGESIZE * Length);
        }

        public int Length
        {
            get
            {
                return Marshal.ReadInt32(pNumMsgs);
            }
            set
            {
                if(value > array_max_length)
                {
                    throw new IndexOutOfRangeException("Length is greater than array bound");
                }
                else
                    Marshal.WriteInt32(pNumMsgs, value);
            }
        }

        public J2534Message this[int index]
        {
            get
            {
                if(index > Marshal.ReadInt32(pNumMsgs))
                {
                    throw new IndexOutOfRangeException("Index is greater than array bound");
                }
                IntPtr pMessage = IntPtr.Add(pMessages, index * CONST.J2534MESSAGESIZE);
                return new J2534Message()
                {
                    ProtocolID = (J2534PROTOCOL)Marshal.ReadInt32(pMessage),
                    RxStatus = (J2534RXFLAG)Marshal.ReadInt32(pMessage, 4),
                    TxFlags = (J2534TXFLAG)Marshal.ReadInt32(pMessage, 8),
                    Timestamp = (uint)Marshal.ReadInt32(pMessage, 12),
                    ExtraDataIndex = (uint)Marshal.ReadInt32(pMessage, 20),
                    Data = MarshalDataArray(pMessage),
                };
            }
            set
            {
                if (index > Marshal.ReadInt32(pNumMsgs))
                {
                    throw new IndexOutOfRangeException("Index is greater than array bound");
                }
                IntPtr pMessage = IntPtr.Add(pMessages, index * CONST.J2534MESSAGESIZE);
                Marshal.WriteInt32(pMessage, (int)value.ProtocolID);
                Marshal.WriteInt32(pMessage, 4, (int)value.RxStatus);
                Marshal.WriteInt32(pMessage, 8, (int)value.TxFlags);
                Marshal.WriteInt32(pMessage, 12, (int)value.Timestamp);
                Marshal.WriteInt32(pMessage, 16, value.Data.Length);
                Marshal.WriteInt32(pMessage, 20, (int)value.ExtraDataIndex);
                Marshal.Copy(value.Data, 0, IntPtr.Add(pMessage, 24), value.Data.Length);
            }
        }

        public IntPtr NumMsgs
        {
            get
            {
                return pNumMsgs;
            }
        }

        public List<J2534Message> ToList()
        {
            List<J2534Message> return_list = new List<J2534Message>();
            for (int i = 0; i < Length; i++)
                return_list.Add(this[i]);
            return return_list;
        }

        public static implicit operator IntPtr(J2534HeapMessageArray HeapMessageArray)
        {
            return HeapMessageArray.pMessages;
        }

        private byte[] MarshalDataArray(IntPtr pData)
        {
            int Length = Marshal.ReadInt32(pData, 16);
            byte[] data = new byte[Length];
            Marshal.Copy(IntPtr.Add(pData, 24), data, 0, Length);
            return data;
        }
        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //
            Marshal.FreeHGlobal(pMessages);
            Marshal.FreeHGlobal(pNumMsgs);
            disposed = true;
        }
    }

    //Class for creating a single message on the heap.  Used for Periodic messages, filters, etc.
    public class J2534HeapMessage:IDisposable
    {
        private IntPtr pMessage;
        private bool disposed;
        public J2534HeapMessage()
        {
            pMessage = Marshal.AllocHGlobal(CONST.J2534MESSAGESIZE);
        }

        public J2534HeapMessage(J2534Message Message)
        {
            pMessage = Marshal.AllocHGlobal(CONST.J2534MESSAGESIZE);
            this.Message = Message;
        }

        public static implicit operator J2534Message(J2534HeapMessage HeapMessage)
        {
            return new J2534Message()
            {
                ProtocolID = (J2534PROTOCOL)Marshal.ReadInt32(HeapMessage.pMessage),
                RxStatus = (J2534RXFLAG)Marshal.ReadInt32(HeapMessage.pMessage, 4),
                TxFlags = (J2534TXFLAG)Marshal.ReadInt32(HeapMessage.pMessage, 8),
                Timestamp = (uint)Marshal.ReadInt32(HeapMessage.pMessage, -12),
                ExtraDataIndex = (uint)Marshal.ReadInt32(HeapMessage.pMessage, 20),
                Data = MarshalDataArray(HeapMessage.pMessage),
            };
        }
        
        public J2534Message Message
        {
            get
            {
                return new J2534Message()
                {
                    ProtocolID = (J2534PROTOCOL)Marshal.ReadInt32(pMessage),
                    RxStatus = (J2534RXFLAG)Marshal.ReadInt32(pMessage, 4),
                    TxFlags = (J2534TXFLAG)Marshal.ReadInt32(pMessage, 8),
                    Timestamp = (uint)Marshal.ReadInt32(pMessage, -12),
                    ExtraDataIndex = (uint)Marshal.ReadInt32(pMessage, 20),
                    Data = MarshalDataArray(pMessage),
                };
            }
            set
            {
                Marshal.WriteInt32(pMessage, (int)value.ProtocolID);
                Marshal.WriteInt32(pMessage, 4, (int)value.RxStatus);
                Marshal.WriteInt32(pMessage, 8, (int)value.TxFlags);
                Marshal.WriteInt32(pMessage, 12, (int)value.Timestamp);
                Marshal.WriteInt32(pMessage, 16, value.Data.Length);
                Marshal.WriteInt32(pMessage, 20, (int)value.ExtraDataIndex);
                Marshal.Copy(value.Data, 0, IntPtr.Add(pMessage, 24), value.Data.Length);
            }
        }

        public static implicit operator IntPtr(J2534HeapMessage HeapMessage)
        {
            return HeapMessage.pMessage;
        }

        private static byte[] MarshalDataArray(IntPtr pData)
        {
            int Length = Marshal.ReadInt32(pData, 16);
            byte[] data = new byte[Length];
            Marshal.Copy(IntPtr.Add(pData, 24), data, 0, Length);
            return data;
        }

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //
            Marshal.FreeHGlobal(pMessage);
            disposed = true;
        }

    }

    public class J2534HeapInt:IDisposable
    {
        private bool disposed;
        private IntPtr pInt;
        public J2534HeapInt()
        {
            pInt = Marshal.AllocHGlobal(4);
        }

        public J2534HeapInt(int i)
        {
            pInt = Marshal.AllocHGlobal(4);
            Marshal.WriteInt32(pInt, i);
        }

        public static implicit operator IntPtr(J2534HeapInt HeapInt)
        {
            return HeapInt.pInt;
        }

        public static implicit operator int(J2534HeapInt HeapInt)
        {
            return Marshal.ReadInt32(HeapInt.pInt);
        }

        public static implicit operator J2534HeapInt(int i)
        {
            return new J2534HeapInt(i);
        }

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //
            Marshal.FreeHGlobal(pInt);
            disposed = true;
        }
    }


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

    public class MessageFilter
    {
        public J2534FILTER FilterType;
        public byte[] Mask;
        public byte[] Pattern;
        public byte[] FlowControl;
        public J2534TXFLAG TxFlags;
        public int FilterId;

        public MessageFilter()
        {
            TxFlags = J2534TXFLAG.NONE;
        }

        public MessageFilter(COMMONFILTER FilterType, byte[] Match)
        {
            TxFlags = J2534TXFLAG.NONE;

            switch (FilterType)
            {
                case COMMONFILTER.PASSALL:
                    PassAll();
                    break;
                case COMMONFILTER.PASS:
                    Pass(Match);
                    break;
                case COMMONFILTER.BLOCK:
                    Block(Match);
                    break;
                case COMMONFILTER.STANDARDISO15765:
                    StandardISO15765(Match);
                    break;
                case COMMONFILTER.NONE:
                    break;
            }
        }

        public void Reset(int Length)
        {
            Mask = new byte[Length];
            Pattern = new byte[Length];
            FlowControl = new byte[Length];
        }

        public void PassAll()
        {
            Reset(1);
            Mask[0] = 0x00;
            Pattern[0] = 0x00;
            FilterType = J2534FILTER.PASS_FILTER;
        }

        public void Pass(byte[] Match)
        {
            ExactMatch(Match);
            FilterType = J2534FILTER.PASS_FILTER;
        }

        public void Block(byte[] Match)
        {
            ExactMatch(Match);
            FilterType = J2534FILTER.BLOCK_FILTER;
        }

        private void ExactMatch(byte[] Match)
        {
            Reset(Match.Length);
            Mask = Enumerable.Repeat((byte)0xFF, Match.Length).ToArray();
            Pattern = Match;
        }
        public void StandardISO15765(byte[] SourceAddress)
        {
            //Should throw exception??
            if (SourceAddress.Length != 4)
                return;
            Reset(4);
            Mask[0] = 0xFF;
            Mask[1] = 0xFF;
            Mask[2] = 0xFF;
            Mask[3] = 0xFF;

            Pattern = SourceAddress;
            Pattern[3] += 0x08;

            FlowControl = SourceAddress;

            TxFlags = J2534TXFLAG.ISO15765_FRAME_PAD;
            FilterType = J2534FILTER.FLOW_CONTROL_FILTER;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public class SConfig
    {
        [FieldOffset(0), MarshalAs(UnmanagedType.U4)]
        public J2534PARAMETER Parameter;
        [FieldOffset(4), MarshalAs(UnmanagedType.U4)]
        public int Value;

        public SConfig(J2534PARAMETER Parameter, int Value)
        {
            this.Parameter = Parameter;
            this.Value = Value;
        }
    }

    public class HeapSConfigList:IDisposable
    {
        private IntPtr pSConfigArrayHeap;
        private bool disposed;
        private int count;

        public HeapSConfigList(SConfig ConfigItem)
        {
            //Create a blob big enough for 'ConfigItems' and two longs (NumOfItems and pItems)
            pSConfigArrayHeap = Marshal.AllocHGlobal(16);
            Count = 1;  //Set count

            //Write pItems.  To save complexity, the array immediately follows SConfigList.
            Marshal.WriteIntPtr(pSConfigArrayHeap, 4, IntPtr.Add(pSConfigArrayHeap, 8));

            //Write ConfigItem to the blob
            Marshal.StructureToPtr<SConfig>(ConfigItem, IntPtr.Add(pSConfigArrayHeap, 8), false);
        }

        public HeapSConfigList(List<SConfig> ConfigItems)
        {
            //Create a blob big enough for 'ConfigItems' and two longs (NumOfItems and pItems)
            pSConfigArrayHeap = Marshal.AllocHGlobal(ConfigItems.Count * 8 + 8);
            Count = ConfigItems.Count;

            //Write pItems.  To save complexity, the array immediately follows SConfigList.
            Marshal.WriteIntPtr(pSConfigArrayHeap, 4, IntPtr.Add(pSConfigArrayHeap, 8));

            //Write the array to the blob
            for(int i = 0, Offset = 8;i < ConfigItems.Count;i++, Offset += 8)
                Marshal.StructureToPtr<SConfig>(ConfigItems[i], IntPtr.Add(pSConfigArrayHeap, Offset), false);
        }

        public int Count
        {
            get
            {
                return Marshal.ReadInt32(pSConfigArrayHeap);
            }
            private set //Count should only be set by the constructor after the Marshal Alloc
            {
                Marshal.WriteInt32(pSConfigArrayHeap, value);
            }
        }
        public SConfig this[int Index]
        {
            get
            {
                    return Marshal.PtrToStructure<SConfig>(IntPtr.Add(pSConfigArrayHeap, (Index * 8 + 8)));
            }
        }

        public static implicit operator IntPtr(HeapSConfigList SConfigList)
        {
            return SConfigList.pSConfigArrayHeap;
        }

        public static implicit operator List<SConfig>(HeapSConfigList SConfigList)
        {
            int Count = Marshal.ReadInt32(SConfigList.pSConfigArrayHeap);
            List<SConfig> List = new List<SConfig>(Count);
            for (int i = 0, Offset = 8; i < Count; i++, Offset += 8)
                List.Add(Marshal.PtrToStructure<SConfig>(IntPtr.Add(SConfigList.pSConfigArrayHeap, Offset)));
            return List;
        }

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //
            Marshal.FreeHGlobal(pSConfigArrayHeap);
            disposed = true;
        }
    }

    public class HeapSByteArray:IDisposable
    {
        private IntPtr pSByteArray;
        private bool disposed;

        public HeapSByteArray(byte SingleByte)
        {
            pSByteArray = Marshal.AllocHGlobal(9);
            Length = 1;
            Marshal.WriteIntPtr(pSByteArray, 4, IntPtr.Add(pSByteArray, 8));
            Marshal.WriteByte(IntPtr.Add(pSByteArray, 8), SingleByte);
        }

        public HeapSByteArray(byte[] SByteArray)
        {
            pSByteArray = Marshal.AllocHGlobal(SByteArray.Length + 8);
            Length = SByteArray.Length;
            Marshal.WriteIntPtr(pSByteArray, 4, IntPtr.Add(pSByteArray, 8));
            Marshal.Copy(SByteArray, 0, IntPtr.Add(pSByteArray, 8), SByteArray.Length);
        }

        public int Length
        {
            get
            {
                return Marshal.ReadInt32(pSByteArray);
            }
            private set
            {
                Marshal.WriteInt32(pSByteArray, value);
            }
        }
        public byte this[int Index]
        {
            get
            {
                if(Index < Length)
                    return Marshal.ReadByte(IntPtr.Add(pSByteArray, Index + 8));
                throw new IndexOutOfRangeException("Index is greater than array bound");
            }
        }

        public static implicit operator IntPtr(HeapSByteArray HeapSByteArray)
        {
            return HeapSByteArray.pSByteArray;
        }

        public static implicit operator byte[](HeapSByteArray HeapSByteArray)
        {
            int Length = Marshal.ReadInt32(HeapSByteArray.pSByteArray);
            byte[] Array = new byte[Length];
            Marshal.Copy(IntPtr.Add(HeapSByteArray.pSByteArray, 8), Array, 0, Length);
            return Array;
        }
        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //
            Marshal.FreeHGlobal(pSByteArray);
            disposed = true;
        }
    }

    //class to hold data reported from the Windows Registry about what J2534 Devices are installed
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
