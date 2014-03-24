# 2013-128CG-Vendord

ClickOnce Deployment of Vendord.Desktop.App
--

- Publish Vendord.Desktop.App
	- Open Vendord.sln in Visual Studio 2008.
	- Right Click Vendord.Desktop.App > Publish
	- Where do you want to publish the application? 
	- C:\Users\Shaun\Documents\GitHub\BigFont\BigFont.MVC\Software\Vendord\Win7\
	- How will users install the application? 
	- From a website: 
	- http://bigfont.ca/software/vendord/win7/
	- Yes. This app is available online and offline.
	- Next > Finish
	- Close Visual Studio 2008

- Include the new files in BigFont.MVC
	- Open BigFont.MVC in Visual Studio 2013
	- In the solution explorer, expand to \Software\Vendord\Win7\ApplicationFiles
	- Show all files in the solution explorer.
	- Right click on the newly published version of Vendord (e.g. Vendord.Desktop.App_1_0_0_64) 
	- Choose Include in Project
	- (Also include any other files that are new in this version of Vendord.Desktop.App)
	- Save the solution via Ctrl + Shift + S
	- Close Visual Studio 2013

- Push the new publish files to the Internet
	- Open the BigFont website in Git
	- Run the following commands
	
	```
	git add -A
	git commit -m "Publish version __ of Vendord.Desktop.App."
	git push
	```
	
- Test the installation package.
	- Go to http://manage.windowsazure.com	
	- Websites > bigfont > deployments
	- Check that the deployment succeeded.
	- If it did, go to http://bigfont.ca/software/vendord/win7/publish.htm > Install
	- Choose "Save"
	- Accept the myriad security prompts
	- Once it is downloaded, choose to Run setup.exe
	- Accept the myriad security prompts
	- Installation will eventually complete.

# Uninstall Vendord.Desktop.App

## Uninstall from the PC


- Use AppWiz.cpl to uninstall Vendord
- Uninstall dependencies
	- Use Microsoft Fixit etc to uninstall Windows Mobile Device Center 
	- Warning - can be very hard - only do this if necessary
	- Then use AppWiz.cpl to uninstall the following:
		1. MS SQL Server Compact 3.5 SP2 x64 (not always present)
		1. MS SQL Server Compact 3.5 SP2 for Devices (not always present)
		1. MS SQL Server Compact 3.5 SP2
		1. MS SQL Server 2005 Compact Edition
		1. MS .NET Compact Framework 3.5 (uninstall isn't an option)
		1. Microsoft Sync Framework 2.1 Core Components (x86)
		1. Microsoft Sync Framework 2.1 Database Providers (x86)
- Delete Data in Documents/VENDORD

### Uninstall from the Device

- Delete the VENDORD ApplicationData directory (warning - will delete data)

- Use AppWiz.cpl to uninstall the following

1 Vendord
2 SQL Server Compact 3.5 Core *
3 Microsoft .NET CF 3.5 EN-String Resources

* If this doesn't uninstall properly, considering deleting the MS SQL Server Compact folder from /Program Files

- Delete the Vendord.SmartDevice.App Program Files directory

