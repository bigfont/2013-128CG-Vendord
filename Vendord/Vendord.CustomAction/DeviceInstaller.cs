[module:
    System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.DocumentationRules", "*",
        Justification = "Reviewed. Suppression of all documentation rules is OK here.")]

namespace Vendord.CustomAction
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration.Install;
    using System.Linq;

    [RunInstaller(true)]
    public partial class DeviceInstaller : Installer
    {
        public DeviceInstaller()
        {
            this.InitializeComponent();
        }

        public override void Commit(System.Collections.IDictionary savedState)
        {
            // Call the Commit method of the base class
            base.Commit(savedState);

            // Open the registry key containing the path to the Application Manager
            Microsoft.Win32.RegistryKey key = null;
            key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\microsoft\\windows\\currentversion\\app paths\\ceappmgr.exe");

            // If the key is not null, then ActiveSync is installed on the user's desktop computer
            if (key != null)
            {
                // Get the path to the Application Manager from the registry value
                string applicationManagerPath = null;
                applicationManagerPath = key.GetValue(null).ToString();

                // Get the target directory where the .ini file is installed.
                // This is sent from the Setup application
                string iniFilePath;
                
                iniFilePath = "\"" + Context.Parameters["targetdir"] + "Vendord.SmartDevice.ini\"";
                PassIniToApplicationManagerAndStart(applicationManagerPath, iniFilePath);

                iniFilePath = "\"" + Context.Parameters["targetdir"] + "SqlServerCe_3_5_1.ini\"";
                PassIniToApplicationManagerAndStart(applicationManagerPath, iniFilePath);
            }
            else
            {
                // No Active Sync - throw a message
            }
        }

        private void PassIniToApplicationManagerAndStart(string applicationManagerPath, string iniFilePath)
        {
            if (applicationManagerPath != null && iniFilePath != null)
            {
                // Now launch the Application Manager
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = applicationManagerPath;
                process.StartInfo.Arguments = iniFilePath;
                process.Start();
            }
        }
    }
}
