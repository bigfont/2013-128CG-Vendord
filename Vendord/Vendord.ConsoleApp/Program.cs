using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using AS = OpenNETCF.Desktop.Communication;

namespace Vendord.ConsoleApp
{

    class Program
    {
        [DllImport("User32.dll")]
        static extern Boolean MessageBeep(UInt32 beepType);

        [DllImport("Ceutil.dll")]
        static extern int CeGetDeviceId();  

        private static AS.RAPI rapi;

        private static void On_RapiConnected()
        {
            Console.WriteLine("Connected");
        }

        // see also http://community.psion.com/knowledge/w/knowledgebase/1330.rapi-remote-api-to-talk-with-windows-ce-device-via-active-sync.aspx
        static void Main(string[] args)
        {
            rapi = new AS.RAPI();

            if (rapi.DevicePresent)
            {
                Console.WriteLine("DevicePresent");

                if (!rapi.Connected)
                {
                    rapi.RAPIConnected += On_RapiConnected;
                    rapi.Connect(false);
                }

                AS.SYSTEM_INFO info;
                rapi.GetDeviceSystemInfo(out info);

                Console.WriteLine(Enum.GetName(typeof(AS.ProcessorArchitecture), info.wProcessorArchitecture));
                Console.WriteLine(Enum.GetName(typeof(AS.ProcessorType), info.dwProcessorType));

                IEnumerable<AS.FileInformation> e = rapi.EnumerateFiles("*");
                Console.WriteLine(e.Count());
                foreach (AS.FileInformation fileinfo in e)
                {
                    Console.WriteLine(fileinfo.FileName);
                }

                if (rapi.DeviceFileExists("Vendord_Testing"))
                {
                    Console.WriteLine("Exists");
                    rapi.RemoveDeviceDirectory("Vendord_Testing");
                }                
            }

            //int i = CeGetDeviceId();
            //MessageBeep(0);

            Console.ReadLine();
        }
    }
}
