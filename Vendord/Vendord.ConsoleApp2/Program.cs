using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Devices;
using System.IO;
using System.Data.SqlServerCe;

namespace Vendord.ConsoleApp2
{
    class Program
    {
        private static RemoteDeviceManager mgr;
        static void Main(string[] args)
        {
            mgr = new RemoteDeviceManager();
            RemoteDevice dev = mgr.Devices.FirstConnectedDevice;

            // copy SDF file to local machine.
            string EXE_DIRECTORY_NAME = @"\Vendord.SmartDeviceApp";
            string DATABASE_NAME = @"\VendordDB.sdf";

            // remote path
            string sdfPath_remote =
                dev.GetFolderPath(SpecialFolder.ProgramFiles) +
                EXE_DIRECTORY_NAME +
                DATABASE_NAME;

            // local path            
            string sdfPath_local = Directory.GetCurrentDirectory() + DATABASE_NAME.Insert(DATABASE_NAME.LastIndexOf(".sdf"), "_PC");

            // confirm
            Console.WriteLine(sdfPath_remote);
            Console.WriteLine(sdfPath_local);            

            RemoteFile.CopyFileFromDevice(dev, sdfPath_remote, sdfPath_local, true);            

            string connString = @"Data Source=" + sdfPath_local;
            SqlCeConnection conn =
                new SqlCeConnection(connString);

            conn.Open();

            

            conn.Close();

            Console.ReadLine();
        }
    }
}
