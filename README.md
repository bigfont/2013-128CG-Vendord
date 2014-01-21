2013-128CG-Vendord
==================

Publish the Desktop App
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
	- BigFont.MVC in Visual Studio 2013
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
	- Go to manage.windowsazure.com	
	- Websites > bigfont > deployments
	- Check that the deployment succeeded.
	- If it did, go to http://bigfont.ca/software/vendord/win7/publish.htm > Install
