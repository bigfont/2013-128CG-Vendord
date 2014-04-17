using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlServerCe;
using Microsoft.Synchronization.Data;
using System.Data;
using Microsoft.Synchronization;
using Microsoft.Synchronization.Data.SqlServerCe;

namespace Vendord.Sync
{
    class SyncFrameworkWrapper
    {
        public static void AddAllScopesToAllNodes(SqlCeConnection localConn, SqlCeConnection remoteConn)
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

        public static SyncOrchestrator SetSyncOptions(
            ScopeName scopeName, 
            IDbConnection localConn,
            IDbConnection remoteConn, 
            EventHandler<SyncStagedProgressEventArgs> progressChanged)
        {
            var localProvider = new SqlCeSyncProvider { ScopeName = scopeName.ToString(), Connection = localConn };

            var remoteProvider = new SqlCeSyncProvider { ScopeName = scopeName.ToString(), Connection = remoteConn };

            var orchestrator = new SyncOrchestrator
            {
                LocalProvider = localProvider,
                RemoteProvider = remoteProvider,
                Direction = SyncDirectionOrder.DownloadAndUpload
            };

            orchestrator.SessionProgress += progressChanged;

            return orchestrator;
        }

        public static void SyncTheNodes(SyncOrchestrator orchestrator)
        {
            orchestrator.Synchronize();
        }

        private static DbSyncScopeDescription DescribeTheScope(
            IEnumerable<string> tablesToSync, 
            string scopeName,
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
    }
}
