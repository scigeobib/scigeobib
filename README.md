SciGeoBib
=========

## About

SciGeoBib allows to make various files for visualization of  geographic data from citation databases.

It alows to create the following:
- *KML file for author Collaboration on different levels*
- *KML and CSV files for Publications in Countries*
- *KML and CSV files for Journals in Countries*

**Collaboration** data are extracted from fields:

- C1 in WoS file
- Affiliations in Scopus file

App creates collaboratiom matrix and allows to map author collaboriation in different levels:

- Institutional level
- City
- Country

Data for **Publications in Countries** are extracted from fields:
- TI in WoS file
- Title in Scopus file
*and*
- PA in WoS file
- Publisher in Scopus file


Data for **Journals in Countries** are extracted from fields:
- SO in WoS file
- Source Title in Scopus file
*and*
- PA in WoS file
- Publisher in Scopus file

## How to

### Input data

SciGeoBib can work with data from:
- Web of Science
- Scopus

When downloadling data from **Web of Science**, make sure to choose output format as *Plain Text* with option *Full Report*

When downloading data from **Scopus**, make sure to choose output format as *CSV* and select checkboxes for:
- *Document Title*
- *Source Title*
- *Affiliations*
- *Publisher*

When the desired data are downloaded, simply choose the file using the *Browse* button on line with description *WoS or Scopus File*

### Api key

Bing Maps API key is necessary for automated geocoding of extracted locations.
Getting a Bing Maps API Key is free, only Microsoft account is required (registration is also free). 
To get the key:
- Go to https://www.bingmapsportal.com/
- Sign in
- Go to *My Account* and choose *My Keys*
- Find a line: *Click here to create a new key.* and click on *here*
- Fill in the form and click on *Create*

Once you've got your API key, it allows you to geocode 200.000 locations, so you cann share it with someone if you want.

Copy the whole key into the *Bing Maps API Key* field.

### Start the process

Simply click on *Start* button.
You may stop the app simply by clicking on *Stop* button anytime.

Under the buttons you can see the output from the app. 
When the app finishes it will print *Done* on the last line.

## Output data

Now you're ready to download the files.

The app can generate following KML files:
- Collaborarion by Counntry
- Collaboration by City
- Collaboration by Institution
- Journals in Countries
- Publications in Countires

And following CSV files:
- Journals in Countries
- Publiacations in Countries

KML files are prepared to be used in Google Earth and Google Maps. We reccomend using Google Earth, because Google Maps does not allow to set icon of specific size, which depreciates the visualization.

You can also download a log file, where you can check if there were any problems during the process.

## Notes

Scopus output file is not very reliable when it comes to geographic data. The Publisher filed if often empty and it often contains only name of the publisher and no location data. 
It also happens, that the output file is not consistent and values are in the wrong columns. We reccomend to check the file first if it's consistent and contains the data needed.

Both data from WoS and Scopus might contain geographic data, which can not be geocoded due to wrong or incomplete content. You can often manually fix these issues.

## Build

* you can download binaries directly from [releases page](https://github.com/scigeobib/scigeobib/releases)
* Visual Studio - you can build:
  - WPF GUI version (ScigeobibWpf)
  - GTK GUI version (ScigeobibGtk)
  - command-line version (ScigeobibCmd)
* monodevelop (Windows or Linux) - you can build:
  - GTK GUI version (ScigeobibGtk)
  - command-line version (ScigeobibCmd)
* for GTK GUI version, you need GtkSharp installed

## Running

* GTK GUI version advanced mode
  - ScigeobibGtk.exe advanced
* command-line version arguments
  - ScigeobibCmd.exe --help

## Used libraries

- all dependencies are set up to be fetched using NuGet
- **third-party libraries used:**
  - [BingMapsRESTToolkit](https://github.com/Microsoft/BingMapsRESTToolkit)
  - [CsvHelper](https://github.com/JoshClose/CsvHelper)
  - [Json.NET](https://github.com/JamesNK/Newtonsoft.Json)
  - [NLog](https://github.com/NLog/NLog)
  - [Command Line Parser Library](https://github.com/gsscoder/commandline)

## License

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>

