# CSA-USB-Tool 

This tool downloads all the installers/packages for a given FRC season. CSA's commonly need these on hand in a USB to help
teams update to the latest version or to diagnose issues.

## Breaking changes incoming! (CSA USB Tool v2)

There is currently a very heavy rewrite being done in the background as I have time to do it.
Goals of this rewrite are as follows:

- Multi-platform support
	- Specifically support for OSX/Linux via a Command Line interface
	- Windows included for CLI
- Move off of WinForms
	- It sucks. 
- Migration to a better format/organization of the software available per season
	- Unknown whether this will still be a CSV, or if it'll be a different format like JSON.
	- This would also enable better support for external tools wanting to make use of the updated lists
- Provide better crash/bug reporting/logging and metrics (opt-out)
	- This is done via DevCycle (my work) - with anonymous data/metrics recording of what software is downloaded and when to provide better understanding of when the tool is used.
- Provide a proper installer for Windows
	- This would allow you to update the tool year to year without re-downloading it and just checking for an update (Windows only unfortunately).
- Signed Windows binaries
	- Code-signed binary to prevent smartscreen warnings.
- FTA specific tooling
	- TBD

These changes are expected to be a breaking change - where the old season file lists will be converted; but no new versions will be added to the "Years.txt" requiring an update to the base binary.

## Download/Usage

### C# GUI

Download the release zip file from the GitHub releases on the right hand side -> 

There is a C# GUI available as a github release (recommended)

PreRequisites: .NET 6 Runtime


### Python CLI

You can download the repo and use one of the two Python scripts.
One is designed for use with the old CSV files (FRC up to 2024 and FTC), and the other with the newer JSON files (FRC from 2024 onwards).

#### CSV

Example usage of the python script for CSV files on unix based systems:

```console
$ ./pyusbtool.py FRCSoftware<YEAR>.csv /path/to/drive/ --download
```

#### JSON

Example usage of the python script for jSON files on unix based systems:

```console
$ ./pyusbtool_json.py Lists/FRC<YEAR>.json /path/to/drive/ --download
```

