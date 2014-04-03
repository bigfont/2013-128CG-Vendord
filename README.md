# 2013-128CG-Vendord

## Development Environment

- Visual Studio 2008
- [.NET Compact Framework 3.5 Redistributable](http://www.microsoft.com/en-ca/download/details.aspx?id=65)

## ClickOnce Deployment of Vendord.Desktop.App

### Publish Vendord.Desktop.App

- Open Vendord.sln in Visual Studio 2008.
- Right Click Vendord.Desktop.App > Publish
- Where do you want to publish the application?
	- C:\Users\Shaun\Documents\GitHub\BigFont\BigFont.MVC\Software\Vendord\Win7\
	- Click Next
- How will users install the application? 
	- From a website: 
	- http://bigfont.ca/software/vendord/win7/
	- Click Next
- Will the application be available offline?
	- Yes. This app is available online and offline.
	- Next   
	- Finish
- Close Visual Studio 2008, saving changes.

### Include the new files in BigFont.MVC

- Open BigFont.MVC in Visual Studio 2013
- In the solution explorer, expand to \Software\Vendord\Win7\ApplicationFiles
- Show all files in the solution explorer.
- Right click on the newly published version of Vendord (e.g. Vendord.Desktop.App_1_0_0_64) 
- Choose Include in Project
- (Also include any other files that are new in this version of Vendord.Desktop.App)	
- Close Visual Studio 2013, saving changes.

### Push the new files to the Internet

- Open the BigFont website in Git
- Run the following commands

```
git add -A
git commit -m "Publish version __ of Vendord.Desktop.App."
git push
```

### Test the installation package.

- Go to http://manage.windowsazure.com	
- Websites > bigfont > deployments
- Check that the deployment succeeded.
- If it did, go to http://bigfont.ca/software/vendord/win7/publish.htm > Install
- Choose "Save File"
- Run setup.exe
- Accept the myriad security prompts
- Once it is downloaded, choose to Run setup.exe
- Accept the myriad security prompts
- Installation will eventually complete and Vendord will open.
- The whole process takes about five minutes on my computer.

## Uninstall Vendord.Desktop.App

### Uninstall Vendord

- Open Add/Remove Programs (appwiz.cpl)
- Uninstall Vendord

### Uninstall dependencies

First try with Add/Remove Programs then with [Revo](http://www.revouninstaller.com/).

1. Windows Mobile Device Center
1. MS SQL Server Compact 3.5 SP2 x64 (not always present)
1. MS SQL Server Compact 3.5 SP2 for Devices (not always present)
1. MS SQL Server Compact 3.5 SP2
1. MS SQL Server 2005 Compact Edition (not always present)
1. MS .NET Compact Framework 3.5 (uninstall sometimes isn't an option)
1. Microsoft Sync Framework 2.1 Core Components (x86)
1. Microsoft Sync Framework 2.1 Database Providers (x86)

Delete Documents/VENDORD (unless you want to keep user data).

## ClickOnce Deployment of Vendord.SmartDevice.App

### Publish Vendord.SmartDevice.Setup

- Rebuild Vendord.SmartDevice.Setup
- Copy the release MSI to C:\Users\Shaun\Documents\GitHub\BigFont\BigFont.MVC\Software\Vendord\WinCe6
- Commit and push BigFont.MVC

### Download and Autmatically Install on The Device

- Plug in the device.
- Connect it to Windows Mobile Device Center.
- Go to http://www.bigfont.ca/software/vendord/wince6/publish.htm
- Click Install
- Run the setup.exe
- It will install the software on the connected device.

### Manually Install on Device after Download

- Connect your device through Windows Mobile Device Center.
- Go to File Management > Browse
- Open "This PC\WindowsCE\Temp"
- Copy "C:\Program Files (x86)\BigFont\Vendord.SmartDevice.Setup"
- Through the devices own interface, run any CAB files within the copied Setup folder.
- The installations will start.
- Click Okay to accept the default install location.
- After install, you can the Vendord.SmartDevice.Setup directory from the Device and the PC.

## Uninstall Vendord.SmartDevice.App

- Connect the device.
- Windows Mobile Device Center will open.
- Choose connect without setting up.
- Click programs/services > more > Add/Remove Programs
- Uncheck the following programs:
	- Vendord
	- SQL Server Compact 3.5 Core
		- If this doesn't uninstall, 
		- considering deleting the MS SQL Server Compact folder from \Program Files
	- Microsoft .NET CF 3.5 EN-String Resources
	- Click Okay to finish.
- Go back to the Windows Mobile Device Center device page
- Click file management > browse
- Delete the following:
	- "This PC\WindowsCE\Application Data\VENDORD\"
	- "This PC\WindowsCE\Program Files\vendord.smartdevice.app\"

