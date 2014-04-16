using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

[module:
    SuppressMessage(
        "StyleCop.CSharp.DocumentationRules", "*",
        Justification = "Reviewed. Suppression of all documentation rules is OK here.")]

namespace Vendord.SmartDevice.Linked
{
    public class Constants
    {
        public const string ApplicationDatabaseName = "VendordDB.sdf";
        public const string ItRetailMsAccessBackupFileName = "PosBack.mdb";
        public const string RemoteCopyFlag = "_REMOTE";
        public const string ApplicationName = "VENDORD";

        private const string ErrorLogName = "error.log";
        private const string ApplicationLogName = "application.log";

        public const int DefaultCasesToOrder = 1;

        // e.g. "C:\Users\Shaun\Documents\VENDORD"
        public static string ApplicationDataStoreFullPath
        {
            get
            {
                string result =
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                    ApplicationName);
                return result;
            }
        }

        // e.g. "C:\Users\Shaun\Documents\VENDORD\PosBack.mdb"
        public static string ItRetailMsAccessBackupFileFullPath
        {
            get
            {
                string result = Path.Combine(ApplicationDataStoreFullPath, ItRetailMsAccessBackupFileName);
                return result;
            }
        }

        // e.g. "C:\Users\Shaun\Documents\VENDORD\error.log"
        public static string ErrorLogFullPath
        {
            get
            {
                string result = 
                    Path.Combine(ApplicationDataStoreFullPath, ErrorLogName);
                return result;
            }
        }

        // e.g. "C:\Users\Shaun\Documents\VENDORD\application.log"
        public static string ApplicationLogFullPath
        {
            get
            {
                string result =
                    Path.Combine(ApplicationDataStoreFullPath, ApplicationLogName);
                return result;
            }
        }

        // e.g. "C:\Users\Shaun\Documents\VENDORD\error.log"
        public static string VendordMainDatabaseFullPath
        {
            get
            {
                string result =
                    Path.Combine(ApplicationDataStoreFullPath, ApplicationDatabaseName);
                return result;
            }
        }
    }
}