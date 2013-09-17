using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using ActiveSync = OpenNETCF.Desktop.Communication;
using System.IO;

namespace Vendord.ConsoleApp
{

    class Program
    {
        [DllImport("User32.dll")]
        private static extern Boolean MessageBeep(UInt32 beepType);

        [DllImport("Ceutil.dll")]
        private static extern int CeGetDeviceId();

        private static ActiveSync.RAPI rapi;

        #region RAPI Wrappers

        // See http://crosswire.org/svn/swordreader/trunk/src/Installer_BC/include/rapi.h
        private const short FAD_NAME = 0x04;
        private const int ALL_DB_TYPES = 0;

        [DllImport("rapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CeFindAllDatabases(
            int dwDbaseType,
            int wFlags,
            out IntPtr cFindData,
            out IntPtr ppFindData);

        #endregion

        private static void InvokeProcedureUsingRAPI()
        {
            string dllPath = "";
            string functionName = "";
            byte[] inputData = new byte[10];
            byte[] outputData;

            rapi.Invoke(dllPath, functionName, inputData, out outputData);
        }

        private static void WriteFileToDeviceUsingRAPI()
        {
            // create the file locally
            string localPath = @"c:\vendord_test.txt";
            string remotePath = @"\vendord_test.txt";
            if (!File.Exists(localPath))
            {
                using (StreamWriter sw = File.CreateText(localPath))
                {
                    sw.WriteLine("Hello World.");
                }
            }

            // copy the file to the device
            if (!rapi.DeviceFileExists(remotePath))
            {
                rapi.CopyFileToDevice(localPath, remotePath);
            }

        }

        private static void EnumerateFilesUsingRAPI()
        {
            Console.WriteLine("\nFiles\n");

            IEnumerable<ActiveSync.FileInformation> e = rapi.EnumerateFiles("*");
            foreach (ActiveSync.FileInformation fileinfo in e)
            {
                Console.Write(fileinfo.FileName);
                Console.Write(" - ");
                Console.WriteLine(fileinfo.FileAttributes);
            }
        }

        private static void On_RapiConnected()
        {
            Console.WriteLine("\nConnected\n");

            // do stuff with the connection
            // this is the entry point for RAPI
            EnumerateFilesUsingRAPI();
            WriteFileToDeviceUsingRAPI();
            //InvokeProcedureUsingRAPI();

            IntPtr cFindData, ppFindData;
            CeFindAllDatabases(ALL_DB_TYPES, FAD_NAME, out cFindData, out ppFindData);
        }

        private static void UseRAPI()
        {
            rapi = new ActiveSync.RAPI();
            if (rapi.DevicePresent)
            {                
                Console.WriteLine("\nDevicePresent\n");
                if (!rapi.Connected)
                {
                    // setup event handler
                    rapi.RAPIConnected += On_RapiConnected;

                    // run Connect async
                    rapi.Connect(false);
                }
            }
        }

        // see also 
        // http://community.psion.com/knowledge/w/knowledgebase/1330.rapi-remote-api-to-talk-with-windows-ce-device-via-active-sync.aspx
        static void Main(string[] args)
        {

            UseRAPI();

            Console.ReadLine();
        }
    }
}
