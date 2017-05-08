using System.Collections.Generic;
using Microsoft.Win32;
using System.Configuration;
using System.IO;

namespace J2534DotNet
{
    static public class J2534Discovery
    {
        static public List<J2534PhysicalDevice> OpenEverything()
        {
            List<J2534PhysicalDevice> PhysicalDeviceList = new List<J2534PhysicalDevice>();
            foreach (string DllFile in FindLibrarys())
            {
                J2534Lib Library = new J2534Lib(DllFile);
                if(Library.IsLoaded)
                {
                    List<string> Devices = new List<string>();
                    //while(Library.GetNextCarDAQ())
                    //    Devices.Add(Library.NextDeviceName);
                    //if(Devices.Count > 0)
                    //{
                    //    foreach(string DeviceName in Devices)
                    //    {
                    //        J2534PhysicalDevice PhysicalDevice = Library.ConstructDevice(DeviceName);
                    //        if(PhysicalDevice.IsConnected)
                    //            PhysicalDeviceList.Add(PhysicalDevice);
                    //    }
                    //}
                    //else if(Library.Status == J2534APIWrapper.FUNCTION_NOT_ASSIGNED)
                    //{
                        J2534PhysicalDevice PhysicalDevice = Library.ConstructDevice();
                        if (PhysicalDevice.IsConnected)
                            PhysicalDeviceList.Add(PhysicalDevice);
                    //}
                }
            }
            return PhysicalDeviceList;
        }

        static private List<string> FindLibrarys()
        {
            List<string> Filenames = new List<string>();

            foreach(J2534RegisteryEntry Entry in GetRegisteryEntries())
                Filenames.Add(Entry.FunctionLibrary);

            Filenames.AddRange(Directory.GetFiles(".", "*.dll"));

            if(ConfigurationManager.AppSettings["UserLibrarys"] != null)
                Filenames.AddRange(ConfigurationManager.AppSettings["UserLibrarys"].Split(';'));

            return Filenames;
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
            string[] DeviceEntries = RootKey.GetSubKeyNames();
            foreach (string Entry in DeviceEntries)
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
