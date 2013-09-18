using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using System.Windows.Forms;
using Microsoft.Synchronization;
using Microsoft.Synchronization.Data.SqlServerCe;
using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Data.Server;


namespace SyncApplication
{
    public class SynchronizationHelper
    {
        ProgressForm progressForm;
        CESharingForm ceSharingForm;

        public SynchronizationHelper(CESharingForm ceSharingForm)
        {
            this.ceSharingForm = ceSharingForm;
        }

        /// <summary>
        /// Utility function that will create a SyncOrchestrator and synchronize the two passed in providers
        /// </summary>
        /// <param name="localProvider">Local store provider</param>
        /// <param name="remoteProvider">Remote store provider</param>
        /// <returns></returns>
        public SyncOperationStatistics SynchronizeProviders(RelationalSyncProvider localProvider, RelationalSyncProvider remoteProvider)
        {
            SyncOrchestrator orchestrator = new SyncOrchestrator();
            orchestrator.LocalProvider = localProvider;
            orchestrator.RemoteProvider = remoteProvider;
            orchestrator.Direction = SyncDirectionOrder.UploadAndDownload;

            progressForm = new ProgressForm();
            progressForm.Show();

            //Check to see if any provider is a SqlCe provider and if it needs schema
            CheckIfProviderNeedsSchema(localProvider as SqlCeSyncProvider);
            CheckIfProviderNeedsSchema(remoteProvider as SqlCeSyncProvider);

            SyncOperationStatistics stats = orchestrator.Synchronize();
            progressForm.ShowStatistics(stats);
            progressForm.EnableClose();
            return stats;
        }

        /// <summary>
        /// Check to see if the passed in CE provider needs Schema from server
        /// </summary>
        /// <param name="localProvider"></param>
        private void CheckIfProviderNeedsSchema(SqlCeSyncProvider localProvider)
        {
            if (localProvider != null)
            {
                SqlCeConnection ceConn = (SqlCeConnection)localProvider.Connection;
		SqlCeSyncScopeProvisioning ceConfig = new SqlCeSyncScopeProvisioning(ceConn );
                string scopeName = localProvider.ScopeName;
                if (!ceConfig.ScopeExists(scopeName))
                {
                    DbSyncScopeDescription scopeDesc = SqlSyncDescriptionBuilder.GetDescriptionForScope(SyncUtils.ScopeName, (System.Data.SqlClient.SqlConnection)(ceSharingForm.providersCollection["Server"]).Connection);
                    ceConfig.PopulateFromScopeDescription(scopeDesc);
                    ceConfig.Apply();
                }
            }
        }

        /// <summary>
        /// This class generates a new SqlSyncProvider for the server and then provisons that server 
        /// if it has not been provisioned.  Typically provisioning the server would occur as a design 
        /// time scenario and not be baked into the client side application as it requires the ability
        /// to modify the server schema.  
        /// </summary>
        /// <param name="hostName"></param>
        /// <returns></returns>
        public SqlSyncProvider ConfigureSqlSyncProvider(string hostName)
        {
            SqlSyncProvider provider = new SqlSyncProvider();
            provider.ScopeName = SyncUtils.ScopeName;

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = hostName;
            builder.IntegratedSecurity = true;
            builder.InitialCatalog = "peer1";
            builder.ConnectTimeout = 1;
            provider.Connection = new SqlConnection(builder.ToString());

            //create a new scope description and add the appropriate tables to this scope
            DbSyncScopeDescription scopeDesc = new DbSyncScopeDescription(SyncUtils.ScopeName);

            //class to be used to provision the scope defined above
            SqlSyncScopeProvisioning serverConfig = new SqlSyncScopeProvisioning((System.Data.SqlClient.SqlConnection)provider.Connection);

            //determine if this scope already exists on the server and if not go ahead and provision
            if (!serverConfig.ScopeExists(SyncUtils.ScopeName))
            {    
                //add the approrpiate tables to this scope
                scopeDesc.Tables.Add(SqlSyncDescriptionBuilder.GetDescriptionForTable("orders",(System.Data.SqlClient.SqlConnection)provider.Connection));
                scopeDesc.Tables.Add(SqlSyncDescriptionBuilder.GetDescriptionForTable("order_details",(System.Data.SqlClient.SqlConnection)provider.Connection));

                //note that it is important to call this after the tables have been added to the scope
                serverConfig.PopulateFromScopeDescription(scopeDesc);

                //indicate that the base table already exists and does not need to be created
                serverConfig.SetCreateTableDefault(DbSyncCreationOption.Skip);

                //provision the server
                serverConfig.Apply();
            }           
            
          
            //Register the BatchSpooled and BatchApplied events. These are fired when a provider is either enumerating or applying changes in batches.
            provider.BatchApplied += new EventHandler<DbBatchAppliedEventArgs>(provider_BatchApplied);
            provider.BatchSpooled += new EventHandler<DbBatchSpooledEventArgs>(provider_BatchSpooled);

            return provider;
        }

        /// <summary>
        /// Utility function that configures a CE provider
        /// </summary>
        /// <param name="sqlCeConnection"></param>
        /// <returns></returns>
        public SqlCeSyncProvider ConfigureCESyncProvider(SqlCeConnection sqlCeConnection)
        {
            SqlCeSyncProvider provider = new SqlCeSyncProvider();
            //Set the scope name
            provider.ScopeName = SyncUtils.ScopeName;
            //Set the connection.
            provider.Connection = sqlCeConnection;

            //Register event handlers

            //1. Register the BeginSnapshotInitialization event handler. Called when a CE peer pointing to an uninitialized
            // snapshot database is about to being initialization.
            provider.BeginSnapshotInitialization += new EventHandler<DbBeginSnapshotInitializationEventArgs>(provider_BeginSnapshotInitialization);

            //2. Register the EndSnapshotInitialization event handler. Called when a CE peer pointing to an uninitialized
            // snapshot database has been initialized for the given scope.
            provider.EndSnapshotInitialization += new EventHandler<DbEndSnapshotInitializationEventArgs>(provider_EndSnapshotInitialization);
    
            //3. Register the BatchSpooled and BatchApplied events. These are fired when a provider is either enumerating or applying changes in batches.
            provider.BatchApplied += new EventHandler<DbBatchAppliedEventArgs>(provider_BatchApplied);
            provider.BatchSpooled += new EventHandler<DbBatchSpooledEventArgs>(provider_BatchSpooled);
            
            //Thats it. We are done configuring the CE provider.
            return provider;
        }

        /// <summary>
        /// Called whenever an enumerating provider spools a batch file to the disk
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void provider_BatchSpooled(object sender, DbBatchSpooledEventArgs e)
        {
            this.progressForm.listSyncProgress.Items.Add("BatchSpooled event fired: Details");
            this.progressForm.listSyncProgress.Items.Add("\tSource Database :" + ((RelationalSyncProvider)sender).Connection.Database);
            this.progressForm.listSyncProgress.Items.Add("\tBatch Name      :" + e.BatchFileName);
            this.progressForm.listSyncProgress.Items.Add("\tBatch Size      :" + e.DataCacheSize);
            this.progressForm.listSyncProgress.Items.Add("\tBatch Number    :" + e.CurrentBatchNumber);
            this.progressForm.listSyncProgress.Items.Add("\tTotal Batches   :" + e.TotalBatchesSpooled);
            this.progressForm.listSyncProgress.Items.Add("\tBatch Watermark :" + ReadTableWatermarks(e.CurrentBatchTableWatermarks));
        }

        /// <summary>
        /// Calls when the destination provider successfully applies a batch file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void provider_BatchApplied(object sender, DbBatchAppliedEventArgs e)
        {
            this.progressForm.listSyncProgress.Items.Add("BatchApplied event fired: Details");
            this.progressForm.listSyncProgress.Items.Add("\tDestination Database   :" + ((RelationalSyncProvider)sender).Connection.Database);
            this.progressForm.listSyncProgress.Items.Add("\tBatch Number           :" + e.CurrentBatchNumber);
            this.progressForm.listSyncProgress.Items.Add("\tTotal Batches To Apply :" + e.TotalBatchesToApply);
        }

        /// <summary>
        /// Reads the watermarks for each table from the batch spooled event. Denotes the max tickcount for each table in each batch
        /// </summary>
        /// <param name="dictionary">Watermark dictionary retrieved from DbBatchSpooledEventArgs</param>
        /// <returns>String</returns>
        private string ReadTableWatermarks(Dictionary<string, ulong> dictionary)
        {
            StringBuilder builder = new StringBuilder();
            Dictionary<string, ulong> dictionaryClone = new Dictionary<string, ulong>(dictionary);
            foreach (KeyValuePair<string, ulong> kvp in dictionaryClone)
            {
                builder.Append(kvp.Key).Append(":").Append(kvp.Value).Append(",");
            }
            return builder.ToString();
        }

        /// <summary>
        /// Snapshot intialization process completed. Database is now ready for sync with other existing peers in topology
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void provider_EndSnapshotInitialization(object sender, DbEndSnapshotInitializationEventArgs e)
        {
            this.progressForm.listSyncProgress.Items.Add("EndSnapshotInitialization Event fired.");
            this.progressForm.ShowSnapshotInitializationStatistics(e.InitializationStatistics, e.TableInitializationStatistics);
            this.progressForm.listSyncProgress.Items.Add("Snapshot Initialization Process Completed.....");
        }

        /// <summary>
        /// CE provider detected that the database was imported from a snapshot from another peer. Snapshot initialziation about to begin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void provider_BeginSnapshotInitialization(object sender, DbBeginSnapshotInitializationEventArgs e)
        {
            this.progressForm.listSyncProgress.Items.Add("Snapshot Initialization Process Started.....");
            this.progressForm.listSyncProgress.Items.Add(
                string.Format("BeginSnapshotInitialization Event fired for Scope {0}", e.ScopeName)
                );
        }

        #region Static Helper Functions
        /// <summary>
        /// Static helper function to create an empty CE database
        /// </summary>
        /// <param name="client"></param>
        public static void CheckAndCreateCEDatabase(CEDatabase client)
        {
            if (!File.Exists(client.Location))
            {
                SqlCeEngine engine = new SqlCeEngine(client.Connection.ConnectionString);
                engine.CreateDatabase();
                engine.Dispose();
            }
        }

        #endregion
    }
}
