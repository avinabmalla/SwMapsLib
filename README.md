# SwMapsLib
A .NET library for reading and writing SW Maps project files.

SW Maps is a free GIS and mobile mapping app for collecting, presenting and sharing geographic information, available for Android. 

https://play.google.com/store/apps/details?id=np.com.softwel.swmaps

## Usage
SW Maps project files (SWMZ) can be read using the SwmzReader class. This creates an instance of the SwMapsProject class, which contains all the information read from the SWMZ file.

To create SW Maps projects, use the SwMapsProjectBuilder class. This creates an SwMapsProject, which can be written as a SW Maps V1 or V2 database, or an SWMZ file.
