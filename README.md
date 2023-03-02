# CSA-USB-Tool 

Tool to download all files in a given FIRST season needed to support teams.

This tool downloads all the installers/packages for a given FRC season. CSA's commonly need these on hand in a USB to help
teams update to the latest version or to diagnose issues.

How to use this tool on MacOS

1. Install Parallels
1. Set up a Windows 10 image
1. Install .NET 6.0 SDK from https://dotnet.microsoft.com/en-us/download
1. In the File Explorer, navigate to the directory where you have downloaded the CSA tool 
  * E.g. This PC > Documents > GitHub > CSA-USB-Tool
1. Open the directory called “CSAUSBTool”.
1. Shift-right click on the background of the window.  
  * Note: You’ll need to find a two-button mouse, as it doesn’t accept shift-control click.
1. Select Open PowerShell window here
1. Run dotnet using:
```
 'C:\Program Files (x86)\dotnet\dotnet.exe' run
```
1. Wait for the app window to open.  This may take a few minutes.
1. Check that the year is correct.
1. Select all the items in the left-hand pane
  * By default the tool will only download NI-LabVIEW
1. Click “Download”
1. Wait until all the files are downloaded
1. Find the downloaded files in a sub-directory like `GitHub/CSA-USB-Tool/CSAUSB/Tool/FRC2023`
