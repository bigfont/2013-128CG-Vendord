# 2013-128CG-Vendord

## Programs and Features

### Development Environment

- Visual Studio 2008
- [Microsoft .NET Compact Framework 3.5 Redistributable]
- Compact View 1.4.3.0

### Sync Framework and SQL Compact Edition

- Microsoft Sync Framework 2.1 Core Components (x86)
- Microsoft Sync Framework 2.1 Database Providers (x86)
- Microsoft SQL Server Compact 3.5 SP2 (x86)
- Microsoft SQL Server Compact 3.5 SP2 (x64)
- Microsoft SQL Server Compact 3.5 SP2 for Devices

### Widows MObile Device Center

- Windows Mobile Device Center
- Windows Mobile Device Center Driver Update

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
- Known Issues
    - Installing Windows Mobile Device Center (WMDC) hangs
    - Workaround: On your PC, locate this folder C:\Windows\WindowsMobile
    - Run setup.exe from there.
    - Then download and install WMDC 6.1 Update
        - [Microsoft Windows Mobile Device Center 6.1 for Windows Vista (32-bit)]
        - [Microsoft Windows Mobile Device Center 6.1 Driver for Windows Vista (64-bit)]
    - Once WMDC installs, re-run the Vendord setup.exe

## Uninstall Vendord.Desktop.App

### Uninstall Vendord

- Open Add/Remove Programs (appwiz.cpl)
- Uninstall Vendord

### Uninstall dependencies

First try with Add/Remove Programs then with [Revo].

1. Windows Mobile Device Center
1. Microsoft SQL Server Compact 3.5 SP2
1. Microsoft SQL Server Compact 3.5 SP2 x64 (not always present)
1. Microsoft SQL Server Compact 3.5 SP2 for Devices (not always present)
1. Microsoft SQL Server 2005 Compact Edition (not always present)
1. Microsoft Sync Framework 2.1 Database Providers (x86)
1. Microsoft Sync Framework 2.1 Core Components (x86)

Delete Documents/VENDORD (unless you want to keep user data).

## Install Vendord.SmartDevice.App

### Install SQL Server Compact Editition 3.5

- Download [SQL CE 3.5 for Devices SP1]
    - It will not work with the [SQL CE for Devices]
    - You must use SP1
- Run the downloaded MSI
- Open "C:\Program Files (x86)\Microsoft SQL Server Compact Edition\v3.5\Devices\wce500\armv4i\"
- Copy sqlce.wce5.armv4i.CAB to the device via WMDC > File Management
- Run the CAB *from the device* to install it.

### Install Vendord.Device.App

- Install the prerequisites first (previous heading).
- Connect the device through Windows Mobile Device Center
- Open Vendord.sln in Visual Studio 2008
    - You will need to have installed [.NET Compact Framework 3.5 Redistributable]
- Expand the solution explorer (Ctrl + Alt + l)
- Right click on Vendord.SmartDevice.App
- Choose Deploy (or choose Debug to test).
- Find the EXE in My Device > Program Files > vendor 

## Uninstall Vendord.SmartDevice.App

- Turn off the Windows CE 6.0 Device
- i.e. press the Power Button for 5 secs
- Then do a cold boot
- i.e. hold and release 1 + 9 + Power Button
- This will reset the device to factory settings.

## Use Vendord

1. Import and sync products, vendors, and departments
1. Create an order and start scanning
1. Sync orders
1. View, modify, and print orders

<!-- Links -->

[.NET Compact Framework 3.5 Redistributable]:
http://www.microsoft.com/en-ca/download/details.aspx?id=65

[Revo]:
http://www.revouninstaller.com/

[SQL CE for Devices]:
http://www.microsoft.com/en-ca/download/details.aspx?id=12264

[SQL CE 3.5 for Devices SP1]:
http://www.microsoft.com/en-us/download/details.aspx?id=17020

[Microsoft Windows Mobile Device Center 6.1 for Windows Vista (32-bit)]:
http://www.microsoft.com/en-ca/download/details.aspx?id=14

[Microsoft Windows Mobile Device Center 6.1 Driver for Windows Vista (64-bit)]:
http://www.microsoft.com/en-ca/download/details.aspx?id=3182


