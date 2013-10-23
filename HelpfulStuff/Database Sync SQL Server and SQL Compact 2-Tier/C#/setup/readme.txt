© 2009 Microsoft Corporation.  All rights reserved.


CE End-To-End SharingAppDemo Sample Application

This application demonstrates how to use Sync Framework database synchronization providers to configure and execute peer-to-peer synchronization between a SQL Server database and one or more SQL Server Compact databases.

What is Demonstrated in This Sample?

- Synchronizing a server database scope (hosted in a SQL Server or SQL Server Express instance) with multiple instances of a Compact client database.
- The new multi-scope change-tracking model on the server.
- Two ways to configure and synchronize a Compact database: full initialization and snapshot initialization.
- Provisioning of both SQL Server and SQL Compact through the use of the Sync Framework API.
- Configuration of SqlSyncProvider and SqlCeSyncProvider objects.
- The use of batcing to throttle resource usage during synchronization.
- The use of the config file to enable tracing.
 

How Do I Install the Application?

1- Connect to an instance of SQL Server, and open and execute peer1_setup.sql.
2- Open demo.sql and execute the "Insert Sample Data In Tables" section at the top of the script.
3- In Visual Studio, open the SharingAppDemo-CEProviderEndToEnd solution.
4- Build the project.


What do the Individual CS Files Contain? 

App directory - Contains all the code files for the Windows Form app.
 App\CeCreationUtilities.cs - Contains utility classes that the app uses to handle string constants and hold client database information.
 App\CESharingForm.cs - Main entrance point for the Window Form app. Contains all GUI eventing/OnClick logic. 
 App\NewCEClientCreationWizard.cs - New wizard app that is used to gather user information to configure and provision a new Compact client database.
 App\ProgressForm.cs - Form app that shows progress information for each SyncOrchestrator.Synchronize() call.
 App\Resource.resx and App\Resources.Designer.cs - Resource files. 
 App\SharingApp.cs - Contains the Main function that launches a new instance of the CESharingForm class.
 App\SynchronizationHelper.cs - The main class that handles configuration of server side SqlSyncProvider and client side SqlCESyncProvider instances. Short instructions are included for each method in the class:
	CheckAndCreateCEDatabase() - Utility function that creates an empty Compact database.
	CheckIfProviderNeedsSchema() - Sample that demonstrates how a Compact provider would determine if the underlying database needs schema or not.
	ConfigureCESyncProvider() - Sample that demonstrates how to configure SqlCeSyncProvider and provision the underlying database.
	ConfigureSqlSyncProvider() - Sample that demonstrates how to configure SqlSyncProvider and provision the underlying database.
	provider_*() - Sample client side event registration code that demonstrates how to handle specific events raised by Sync Framework.
 App\TablesViewControl.cs - Custom user control that displays values from the two sample tables (orders and order_details), based on the client and server connections that are passed in. 
 
Setup directory - Contains the server provisioning .sql files.


How Do I Use the Sample?

1.  Install the application as described in the "How to install" section.
2.  Run the sample app. By default it assumes that SQL Server is installed as localhost. If it's not, then change the server from Environment.MachineName to the desired server in the CESharingForm_Shown() method.
3.  If the sample is correctly installed, values from the orders and order_details should display in the datagrid on the "Server" tab.
4.  The Synchronize button is disabled until at least one Compact client is added. Add a new Compact client database by clicking "Add CE Client". Options for creating a new client in the New CE Creation Wizard:
	* Full initialization - Create an empty Compact database, get the schema from the server, create the schema on the client, and get all data from the server on the first Synchronize() call.
	* Snapshot initialization - Export an existing Compact database, initialize that snapshot and receive only incremental changes from the server on the first Synchronize() call.
5a. For full initialization, select the location and file name. 
5b. For snapshot initialization, select the .sdf file of an existing client (in the same scope) and pick the location and name for the exported database.
6.  On clicking OK, a new tab with the name "Client#" should be added to the main form. After the client is synchronized, clicking that tab displays values for the tables orders and order_details in that Compact database.
7.  Batching can be enabled by setting a non zero value in the Batch size text box. By default batching is turned off.
7a. Batching location can be modified by clicking on the Change button.
8.  To synchronize, select source and destination providers, and click Synchronize.
9.  Make changes to the server or client database tables and then try to synchronize the peers to confirm that changes are synchronized.
