namespace Vendord.SmartDevice.Shared
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;

    public class Constants
    {
        public const string DATABASE_NAME = "VendordDB.sdf";
        public const string APPLICATION_NAME = "VENDORD";
        public const string TEMPORARY_COPY_OF_REMOTE_FILE_FLAG = "_REMOTE";

        private const string ERROR_LOG_NAME = "error.log";
        private const string APPLICATION_LOG_NAME = "application.log";

        public static string ApplicationDataStoreFullPath
        {
            get
            {
                string result;
                result = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), APPLICATION_NAME);
                return result;
            }
        }

        public static string ErrorLogFullPath
        {
            get
            {
                string result;
                result = Path.Combine(ApplicationDataStoreFullPath, ERROR_LOG_NAME);
                return result;
            }
        }

        public static string ApplicationLogFullPath
        {
            get
            {
                string result;
                result = Path.Combine(ApplicationDataStoreFullPath, APPLICATION_LOG_NAME);
                return result;
            }
        }

        public static string DatabaseFullPath
        {
            get
            {
                string result;
                result = Path.Combine(ApplicationDataStoreFullPath, DATABASE_NAME);
                return result;
            }
        }
    }
}
