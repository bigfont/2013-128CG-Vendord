2013-128CG-Vendord
==================

Publish the Desktop App
--

- Open Visual Studio 2008 > Vendord

- Publish Vendord.Desktop.App
	- I.e. Right Click > Publish
	- Location to Publish > C:\Users\Shaun\Documents\GitHub\BigFont\BigFont.MVC\Software\Vendord\Win7\
	- From a Website > http://bigfont.ca/software/vendord/win7/
	- Yes. This app is available online and offline.

- Include the new files in BigFont.MVC
	- Open Visual Studio 2013 > BigFont.MVC	
	- Expand to \Software\Vendord\Win7\ApplicationFiles
	- Show all files
	- Right click on the newly published version of Vendord > Include in Project
	- (Also include any other new files)
	- Save the solution via Ctrl + Shift + S

- Push the publish files to the Internet
	- Open Git > BigFont
	- Run the following: 
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
