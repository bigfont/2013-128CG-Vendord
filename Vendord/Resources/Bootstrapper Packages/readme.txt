
1. Copy and paste these packages into the following directory 

C:\Program Files (x86)\Microsoft SDKs\Windows\v6.0A\Bootstrapper\Packages

2. Open Vendord in Visual Studio 2008. 
3. Open the solution explorer (CTRL + ALT + L)
4. Go to Vendord.Desktop.App > Right Click > Properties > Publish > Prerequisites.
5. In the prerequisites dialog, choose the following: 

    Create setup program to install prerequisite components. (YES)

    Windows Installer 3.1
    .NET Framework 3.5
    SQL Server Compact 3.5.8
    Windows Mobile Device Center v6.1.6965.0
    SyncFX20Core v2.1 (x86)
	SyncFX20DatabaseProviders v2.1 (x86)

    Download prerequisites from the same location as my application.

6. OK
7. File > Save All. 
8. Go to Vendord.Desktop.App > Right Click > Publish
9. To be continued...