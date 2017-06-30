using System.Collections.Generic;
using Microsoft.Win32;
using System.Configuration;
using System.IO;

namespace J2534
{
    static public class J2534Discovery
    {
        static internal List<J2534Library> Librarys;
        static internal List<J2534Device> PhysicalDevices;

        static J2534Discovery()
        {
            Librarys = new List<J2534Library>();
            PhysicalDevices = new List<J2534Device>();
        }

        static public List<J2534Device> OpenEverything()
        {            
            foreach (J2534Library DLL in FindLibrarys())
            {
                ConnectAllDevices(DLL);
            }
            return PhysicalDevices;
        }

        static private void ConnectAllDevices(J2534Library DLL)
        {
            //If the DLL successfully executes GetNextCarDAQ_RESET()
            if (DLL.GetNextCarDAQ_RESET() == J2534ERR.STATUS_NOERROR)
            {
                GetNextCarDAQResults TargetDrewtechDevice = DLL.GetNextCarDAQ();
                while (TargetDrewtechDevice.Exists)
                {
                    J2534Device ThisDevice = DLL.ConstructDevice(TargetDrewtechDevice);
                    if (ThisDevice.IsConnected) //This should always succeed.
                    {   //To avoid populating the list with duplicate devices due to disconnection
                        //Remove any unconnected devices that are attached to this library.
                        PhysicalDevices.RemoveAll(Listed => Listed.DeviceName == ThisDevice.DeviceName);
                        PhysicalDevices.Add(ThisDevice);
                    }
                    TargetDrewtechDevice = DLL.GetNextCarDAQ();
                }
            }
            //If its not a drewtech library, then attempt to connect a device
            else
            {
                J2534Device ThisDevice = DLL.ConstructDevice();
                if (ThisDevice.IsConnected)
                {   //To avoid populating the list with duplicate devices due to disconnection
                    //Remove any unconnected devices that are attached to this library.
                    PhysicalDevices.RemoveAll(Listed => (Listed.Library == ThisDevice.Library) && !Listed.IsConnected);
                    PhysicalDevices.Add(ThisDevice);
                }
            }
        }

        static private List<J2534Library> FindLibrarys()
        {
            List<string> DllFiles = new List<string>();

            foreach(J2534RegisteryEntry Entry in GetRegisteryEntries())
                DllFiles.Add(Entry.FunctionLibrary);

            DllFiles.AddRange(Directory.GetFiles(".", "*.dll"));

            if(ConfigurationManager.AppSettings["UserLibrarys"] != null)
                DllFiles.AddRange(ConfigurationManager.AppSettings["UserLibrarys"].Split(';'));

            foreach (string DllFile in DllFiles)
            {
                J2534Library ThisLibrary = new J2534Library(DllFile);
                if (ThisLibrary.IsLoaded && !Librarys.Exists(Listed => Listed.FileName == ThisLibrary.FileName))
                    Librarys.Add(ThisLibrary);
            }
            return Librarys;
        }

        static internal List<J2534RegisteryEntry> GetRegisteryEntries()
        {
            const string PASSTHRU_REGISTRY_PATH = @"Software\PassThruSupport.04.04";
            const string PASSTHRU_REGISTRY_PATH_6432 = @"Software\Wow6432Node\PassThruSupport.04.04";

            List<J2534RegisteryEntry> Entries = new List<J2534RegisteryEntry>();
            RegistryKey RootKey = Registry.LocalMachine.OpenSubKey(PASSTHRU_REGISTRY_PATH, false);
            if (RootKey == null)
            {
                RootKey = Registry.LocalMachine.OpenSubKey(PASSTHRU_REGISTRY_PATH_6432, false);
                if (RootKey == null)
                    return Entries;
            }
            string[] RegistryEntries = RootKey.GetSubKeyNames();
            foreach (string Entry in RegistryEntries)
            {
                RegistryKey deviceKey = RootKey.OpenSubKey(Entry);
                if (deviceKey == null)
                    continue;
                Entries.Add(new J2534RegisteryEntry()
                {
                    Vendor = (string)deviceKey.GetValue("Vendor", ""),
                    Name = (string)deviceKey.GetValue("Name", ""),
                    ConfigApplication = (string)deviceKey.GetValue("ConfigApplication", ""),
                    FunctionLibrary = (string)deviceKey.GetValue("FunctionLibrary", ""),
                    CANChannels = (int)deviceKey.GetValue("CAN", 0),
                    ISO15765Channels = (int)deviceKey.GetValue("ISO15765", 0),
                    J1850PWMChannels = (int)deviceKey.GetValue("J1850PWM", 0),
                    J1850VPWChannels = (int)deviceKey.GetValue("J1850VPW", 0),
                    ISO9141Channels = (int)deviceKey.GetValue("ISO9141", 0),
                    ISO14230Channels = (int)deviceKey.GetValue("ISO14230", 0),
                    SCI_A_ENGINEChannels = (int)deviceKey.GetValue("SCI_A_ENGINE", 0),
                    SCI_A_TRANSChannels = (int)deviceKey.GetValue("SCI_A_TRANS", 0),
                    SCI_B_ENGINEChannels = (int)deviceKey.GetValue("SCI_B_ENGINE", 0),
                    SCI_B_TRANSChannels = (int)deviceKey.GetValue("SCI_B_TRANS", 0)
                });
            }

            return Entries;
        }
    }
}
