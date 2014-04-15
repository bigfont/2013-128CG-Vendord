using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Synchronization;
using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Data.SqlServerCe;
using Vendord.SmartDevice.Linked;

namespace Vendord.Sync
{
    using RAPI = System.Devices;
    using System.Data.OleDb;
    using System.Xml.Linq;
    using System.ComponentModel; // Remote API Managed Code Wrapper

    public class Sync
    {
        // rapi       
        public enum SyncResult
        {
            NoRemoteDatabase,
            Disconnected,
            Complete
        }

        private string _remoteDatabaseFullPath;
        private string _remoteDatabaseLocalCopyFullPath;

        internal SyncResult PullVendorsFromItRetailXmlBackup(BackgroundWorker worker, string filePath, ref int totalRecords, ref int insertedRecords)
        {
            SyncResult result;
            try
            {
                CopyVendorsFromItRetailXmlBackupFilesToDesktopDb(worker, filePath, ref totalRecords, ref insertedRecords);
                result = SyncResult.Complete;
            }
            catch (SqlException ex)
            {
                result = SyncResult.Disconnected;
                IOHelpers.LogException(ex);
            }

            return result;
        }

        public SyncResult PullProductsFromItRetailXmlBackup(BackgroundWorker worker, string filePath, ref int totalRecords, ref int insertedRecords)
        {
            SyncResult result;
            try
            {
                CopyProductsFromItRetailXmlBackupFilesToDesktopDb(worker, filePath, ref totalRecords, ref insertedRecords);
                result = SyncResult.Complete;
            }
            catch (SqlException ex)
            {
                result = SyncResult.Disconnected;
                IOHelpers.LogException(ex);
            }

            return result;
        }

        public SyncResult SyncDesktopAndDeviceDatabases(string scopeName)
        {
            // assume the worste
            var result = SyncResult.Disconnected;

            // get the remote device
            var mgr = new RAPI.RemoteDeviceManager();
            var remoteDevice = mgr.Devices.FirstConnectedDevice;
            if (remoteDevice == null || remoteDevice.Status != RAPI.DeviceStatus.Connected) return result;

            // we assume that we have a connected device now but sometimes it disconnected
            try
            {
                // Get the remote database
                SetRemoteDeviceDatabaseNames(remoteDevice);
                if (TryCopyDatabaseFromDeviceToDesktop(remoteDevice))
                {
                    // Instantiate the connections
                    var localConn = new SqlCeConnection(Database.GenerateSqlCeConnString(Constants.VendordMainDatabaseFullPath));
                    var remoteConn = new SqlCeConnection(Database.GenerateSqlCeConnString(_remoteDatabaseLocalCopyFullPath));

                    // Provision the nodes
                    AddAllScopesToAllNodes(localConn, remoteConn);

                    // Set sync options
                    var orchestrator = SetSyncOptions(scopeName, localConn, remoteConn);

                    // Sync
                    SyncTheNodes(orchestrator);

                    // Clean up
                    remoteConn.Close();
                    localConn.Close();
                    CleanUpDatabases();
                    CopyDatabaseBackToDevice(remoteDevice);

                    // success!
                    result = SyncResult.Complete;
                }
                else
                {
                    result = SyncResult.NoRemoteDatabase;
                }
            }
            catch (InvalidOperationException e)
            {
                IOHelpers.LogException(e);
            }
            catch (RAPI.RapiException e)
            {
                IOHelpers.LogException(e);
            }
            finally
            {
                remoteDevice.Dispose();
            }

            return result;
        }

        private List<Vendor> GetVendorListFromXmlBackup(string filePath)
        {
            XElement vendorsXml = XElement.Load(filePath);
            var query =
                from v in vendorsXml.Descendants("Vendors")
                select new Vendor()
                {
                    Id = Convert.ToInt32(v.Element("vendor_id").Value.ToString().Trim()),
                    Name = (string)v.Element("name")
                };

            // for debugging
            int i = query.Count();

            List<Vendor> vendors = query.ToList<Vendor>();
            return vendors;
        }

        private List<Product> GetProductListFromXmlBackup(string filePath)
        {
            XElement productsXml = XElement.Load(filePath);

            Func<string, long> parseLong =
                value =>
                {
                    long longValue;
                    return Int64.TryParse(value, out longValue) ? longValue : -1;
                };

            Func<string, decimal> parseDecimal =
                value =>
                {
                    decimal decimalValue;
                    return Decimal.TryParse(value, out decimalValue) ? decimalValue : -1m;
                };

            Func<string, int> parseInt =
                value =>
                {
                    int intValue;
                    return Int32.TryParse(value, out intValue) ? intValue : -1;
                };

            Func<XElement, string, string> getXElementValue =
                (xElem, key) =>
                {
                    string result = string.Empty;
                    if (xElem != null && xElem.Element(key) != null && xElem.Element(key).Value != null)
                    {
                        result = xElem.Element(key).Value;
                    }
                    return result;
                };

            List<Product> products = new List<Product>();

            string normalPrice = string.Empty;

            try
            {
                var query = from p
                        in productsXml.Descendants("Products")
                        select p;

                foreach (XElement elem in query)
                {
                    Product p = new Product();
                    p.Upc = parseLong(getXElementValue(elem, "upc"));
                    p.CertCode = parseLong(getXElementValue(elem, "cert_code"));
                    p.Name = getXElementValue(elem, "description");
                    p.Price = parseDecimal(getXElementValue(elem, "normal_price"));
                    p.Department = new Department()
                    {
                        Id = parseInt(getXElementValue(elem, "department")),
                        Name = string.Empty
                    };
                    p.Vendor = new Vendor()
                    {
                        Id = parseInt(getXElementValue(elem, "vendor")),
                        Name = string.Empty
                    };
                    products.Add(p);
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }

            return products;
        }

        private void CopyVendorsFromItRetailXmlBackupFilesToDesktopDb(BackgroundWorker worker, string filePath, ref int totalRecords, ref int insertedRecords)
        {
            List<Vendor> vendors = GetVendorListFromXmlBackup(filePath);

            insertedRecords = 0;
            totalRecords = vendors.Count();
            DateTime progressReportTime = DateTime.MinValue;

            Database db = new Database();
            DbQueryExecutor queryExe = new DbQueryExecutor(db.ConnectionString);

            foreach (Vendor v in vendors)
            {
                v.QueryExecutor = queryExe;

                if (v.Name.Length == 0)
                {
                    v.Name = "Vendor #" + v.Id;
                }

                v.UpsertIntoDb();

                insertedRecords++;

                // if one seconds have passed, make the worker report progress
                double timeSinceLastReport = DateTime.Now.Subtract(progressReportTime).TotalSeconds;
                if (timeSinceLastReport >= 1.0)
                {
                    worker.ReportProgress(100 * insertedRecords / totalRecords);
                    progressReportTime = DateTime.Now;
                }
            }
        }

        private void CopyProductsFromItRetailXmlBackupFilesToDesktopDb(BackgroundWorker worker, string filePath, ref int totalRecords, ref int insertedRecords)
        {
            List<Product> products = GetProductListFromXmlBackup(filePath);

            insertedRecords = 0;
            totalRecords = products.Count();
            DateTime progressReportTime = DateTime.MinValue;

            Database db = new Database();
            DbQueryExecutor queryExe = new DbQueryExecutor(db.ConnectionString);

            foreach (Product p in products)
            {
                Vendor v = new Vendor();
                v.QueryExecutor = queryExe;
                v.Id = p.Vendor.Id;
                v.Name = p.Vendor.Name;
                v.UpsertIntoDb();

                Department d = new Department();
                d.QueryExecutor = queryExe;
                d.Id = p.Department.Id;
                d.Name = p.Department.Name;
                d.UpsertIntoDb();

                p.QueryExecutor = queryExe;
                p.UpsertIntoDb();
                insertedRecords++;

                // if five seconds have passed, make the worker report progress
                double timeSinceLastReport = DateTime.Now.Subtract(progressReportTime).TotalSeconds;
                if (timeSinceLastReport >= 1.0)
                {
                    worker.ReportProgress(100 * insertedRecords / totalRecords);
                    progressReportTime = DateTime.Now;
                }
            }
        }

        private void SetRemoteDeviceDatabaseNames(RAPI.RemoteDevice remoteDevice)
        {
            var rapiApplicationData = remoteDevice.GetFolderPath(RAPI.SpecialFolder.MyDocuments);
            var rapiApplicationDataStore = Path.Combine(rapiApplicationData, Constants.ApplicationName);
            _remoteDatabaseFullPath = Path.Combine(rapiApplicationDataStore, Constants.ApplicationDatabaseName);
            _remoteDatabaseLocalCopyFullPath = IOHelpers.AddSuffixToFilePath(Constants.VendordMainDatabaseFullPath,
                Constants.RemoteCopyFlag);
        }

        private bool TryCopyDatabaseFromDeviceToDesktop(RAPI.RemoteDevice remoteDevice)
        {
            bool result = false;

            // does the device have a database
            if (RAPI.RemoteFile.Exists(remoteDevice, _remoteDatabaseFullPath))
            {
                // yup, so copy it to the desktop
                RAPI.RemoteFile.CopyFileFromDevice(remoteDevice, _remoteDatabaseFullPath, _remoteDatabaseLocalCopyFullPath,
                    true);
                result = true;
            }

            return result;
        }

        private void CleanUpDatabases()
        {
            var db = new Database();
            db.EmptyTrash();

            var dbRemote = new Database(_remoteDatabaseLocalCopyFullPath);
            dbRemote.EmptyTrash();
        }

        private void CopyDatabaseBackToDevice(RAPI.RemoteDevice remoteDevice)
        {
            if (File.Exists(_remoteDatabaseLocalCopyFullPath))
            {
                RAPI.RemoteFile.CopyFileToDevice(remoteDevice, _remoteDatabaseLocalCopyFullPath, _remoteDatabaseFullPath, true);
            }
        }

        #region MS Sync Framework

        private static DbSyncScopeDescription DescribeTheScope(IEnumerable<string> tablesToSync, string scopeName,
            SqlCeConnection conn)
        {
            // create a scope description object
            var scopeDesc = new DbSyncScopeDescription { ScopeName = scopeName };

            // add each table to the scope without any filtering
            foreach (var tableDesc in tablesToSync.Select(tableName => SqlCeSyncDescriptionBuilder.GetDescriptionForTable(tableName, conn)))
            {
                scopeDesc.Tables.Add(tableDesc);
            }

            return scopeDesc;
        }

        private static void AddAllScopesToAllNodes(SqlCeConnection localConn, SqlCeConnection remoteConn)
        {
            var orderTables = new[] { "tblOrder", "tblOrderProduct" };
            var orderScopeName = "SyncOrders";

            ProvisionNode(orderScopeName, orderTables, localConn);
            ProvisionNode(orderScopeName, orderTables, remoteConn);

            var importTables = new[] { "tblVendor", "tblDepartment", "tblProduct" };
            var importScopeName = "SyncProductsVendorsAndDepts";

            ProvisionNode(importScopeName, importTables, localConn);
            ProvisionNode(importScopeName, importTables, remoteConn);
        }

        private static void ProvisionNode(string scopeName, string[] tablesToSync, SqlCeConnection conn)
        {
            var ceConfig = new SqlCeSyncScopeProvisioning(conn);
            if (!ceConfig.ScopeExists(scopeName))
            {
                DbSyncScopeDescription scopeDesc = DescribeTheScope(tablesToSync, scopeName, conn);
                ceConfig.SetCreateTableDefault(DbSyncCreationOption.CreateOrUseExisting);
                ceConfig.PopulateFromScopeDescription(scopeDesc);
                ceConfig.Apply();
            }
        }

        private static SyncOrchestrator SetSyncOptions(string scopeName, IDbConnection localConn,
            IDbConnection remoteConn)
        {
            var localProvider = new SqlCeSyncProvider { ScopeName = scopeName, Connection = localConn };

            var remoteProvider = new SqlCeSyncProvider { ScopeName = scopeName, Connection = remoteConn };

            var orchestrator = new SyncOrchestrator
            {
                LocalProvider = localProvider,
                RemoteProvider = remoteProvider,
                Direction = SyncDirectionOrder.DownloadAndUpload
            };

            return orchestrator;
        }

        private static void SyncTheNodes(SyncOrchestrator orchestrator)
        {
            orchestrator.Synchronize();
        }

        #endregion
    }
}