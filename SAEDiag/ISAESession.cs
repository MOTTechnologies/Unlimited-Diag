using J2534;
using System.Collections.Generic;

namespace SAE
{
    interface ISAESession
    {
        SAEMessage SAETxRx(int Addr, byte Mode, int RxDataIndex, byte[] Data);
        void SAETx(int Addr, byte Mode, List<byte[]> Data);
        object CreateRxHandle(int Addr, byte Mode, byte[] RxSignature);
        void DestroyRxHandle(object Handle);
        List<byte[]> SAERx(object RxHandle, int NumOfMsgs, int Timeout, bool DestroyHandle = false);
    }
}
