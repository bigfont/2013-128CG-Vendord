[module:
    System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.DocumentationRules", "*",
        Justification = "Reviewed. Suppression of all documentation rules is OK here.")]

namespace Vendord.SmartDevice.Shared
{
    using System;
    using System.Collections.Generic;    
    using System.IO;
    using System.Linq;
    using System.Text;

    public class Constants
    {
        public const string ApplicationDatabaseName = "VendordDB.sdf";
        public const string RemoteCopyFlag = "_REMOTE";
        public const string ApplicationName = "VENDORD";
        public const string ItRetailDatabaseConnectionString = "Data Source=FONTY;Initial Catalog=ITRetail;Integrated Security=True";

        private const string ErrorLogName = "error.log";
        private const string ApplicationLogName = "application.log";

        public static string ApplicationDataStoreFullPath
        {
            get
            {
                string result;
                result = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationName);
                return result;
            }
        }

        public static string ErrorLogFullPath
        {
            get
            {
                string result;
                result = Path.Combine(ApplicationDataStoreFullPath, ErrorLogName);
                return result;
            }
        }

        public static string ApplicationLogFullPath
        {
            get
            {
                string result;
                result = Path.Combine(ApplicationDataStoreFullPath, ApplicationLogName);
                return result;
            }
        }

        public static string VendordDatabaseFullPath
        {
            get
            {
                string result;
                result = Path.Combine(ApplicationDataStoreFullPath, ApplicationDatabaseName);
                return result;
            }
        }              
    }
}
