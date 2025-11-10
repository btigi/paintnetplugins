iiIEPaintNetPlugins
=====

iiIEPaintNetPlugins is a plugin for [Paint.Net](https://www.getpaint.net/) which allows saving and loading of MOS V1 and TIS V1 files, as used in Infinity Engine games (Baldur's Gate I, Baldur's Gate II, Icewind Dale I, Icewind Dale II and Placescape: Torment). The plugins supports loading both compressed and uncompressed MOS files, though always saves uncompressed files.

The plugin is tested with Paint.Net 5.1.9 (Stable 5).

Note: When starting Paint.NET by loading a compressed MOS file (e.g. Paint.Net is not running, you double click a file in Windows Explorer, this opens Paint.Net and displays the image) the image can sometimes be corrupted. This appears to be an issue with Paint.Net. The recommended workaround is to open Paint.Net first.

## Installation

1. Place the plugin in the FileTypes directory within the main Paint.Net installation directory (usually C:\Program Files\Paint.NET), e.g. C:\Program Files\Paint.NET\FileTypes

2. You'll also need a copy of [iiInfinityEngine.dll](https://github.com/btigi/iiInfinityEngine) commit 3d449c6f7f5f214023cb9931e0b54aba87a20d4a or placed in the same directory.

3. TIS files do not store their dimensions. In order to find the dimensions iiIEPaintNetPlugins calculates a hash of the file then uses that hash to lookup the dimensions in areadatasplugin.xml. The areadatasplugin.xml file is assumed to be in the current user's user directory, e.g. C:\users\%username%\areadatasplugin.xml. The location is this file can be configured using an environment variable named iiIEPaintNetPlugins, which should contain the full path and filename of the areadatasplugin.xml file to be used. A areadatasplugin.xml file containing the hashes for all BG1, BG2, PST, IWD and IWD2 TIS files can be found in the resources folder.

## Compiling

To clone and run this application, you'll need [Git](https://git-scm.com) and [.NET](https://dotnet.microsoft.com/) installed on your computer. From your command line:

```
# Clone this repository
$ git clone https://github.com/btigi/iiiepaintnetplugins

# Go into the repository
$ cd src

# Build  the app
$ dotnet build
```

## Licencing

iiIEPaintNetPlugins is licenced under the MIT License. Full licence details are available in licence.md