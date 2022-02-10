using System;
using System.Management;

namespace VideoCompressorGUI.Utils
{
    
    public class NvidiaNvencSupport
    {
        public static bool Supported()
        {
            ManagementObjectSearcher myVideoObject = new ManagementObjectSearcher("select * from Win32_VideoController");

            string name = "";
            foreach (ManagementObject obj in myVideoObject.Get())
            {
                name = (string) obj["Name"];
                break;
            }

            name = name.ToLower();

            return name.Contains("rtx") || name.Contains("gtx");
        }
    }
}