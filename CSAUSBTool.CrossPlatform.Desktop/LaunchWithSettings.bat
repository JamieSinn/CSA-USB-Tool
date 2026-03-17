@echo off
setlocal

set "EXE=%~dp0CSAUSBTool.CrossPlatform.Desktop.exe"
if not exist "%EXE%" (
  echo Could not find executable:
  echo %EXE%
  echo Place this .bat in the same folder as CSAUSBTool.CrossPlatform.Desktop.exe.
  pause
  exit /b 1
)

start "" "%EXE%" --showsetting
