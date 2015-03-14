# BeyondCompare Setup #
Using DbEdit with BeyondCompare allows for easy comparing and editing of files stored in a database using BeyondCompare

When BeyondCompare is configured to use DbEdit as a converter for the file type, any folder with a DbEdit .dbecfg file in it will attempt to load that file from the database every time the file is compared in BeyondCompare.

If the file is changed in BeyondCompare, saving file will update the database.  A message box shows any save errors.

When the installer is used a file association is setup for `*`.dbecfg file.  Opening the config file via BeyondCompare’s Explorer->Open context menu option will retrieve all the data from the database into the config file’s folder.

## File Format Setup ##
Using this method, you can use BC to automatically get from and save to the database when comparing files.  Alternatively, you may manually get and save files using the [Using Open With](DbEditAndBeyondCompare#Using_Open_With.md) option below.

  1. Start BeyondCompare
  1. Open Tools->File Formats… dialog
  1. Select file format you want to use in the list (E.g. XML)
  1. Click on the Conversion tab
  1. Select External program
  1. Enter the fully-qualified path for the converter, with the parameters as show in the image below and after the image for cutting and pasting

**Loading:** `<path>\JOAT Services\JOAT DbEdit\DbEdit.exe -l %s %t %n`

**Saving:** `<path>\JOAT Services\JOAT DbEdit\DbEdit.exe -s %s %t %n`

Where `<path>` is `\Program Files (x86)\` on 64-bit systems or `\Program Files\` on 32-bit.

If you want to format XML files on load use –lx instead of -l.  This makes comparing easier.

### Caveats To File Format Setup ###
When opening a file for compare it is retrieved from the database.  When saving from the File compare screen, the file is written to the database.  If you use Open With->Text Edit, the file is retrieved from the database and saving will save it.

**When the file is _not_ retrieved from the database**
  * Using Open on the context menu
  * Working with files at the folder level.  E.g. Copy To Right/Left
  * Syncing



## Using Open With ##
If you do not want files retrieved and saved by BC every time it opens a file, do not configure the File Format, but use Tools->Options->Open With to configure DbEdit.

For the Command Line, use the `<installDir>\DbEdit.exe  %p\DBEdit.dbecfg`

This will open DbEdit so you can get files from the database, and selectively update the files.

You may also set a Shortcut key to launch it.

## Initial downloading ##
To get the files locally the first time, or do download new ones, you can use the Open With option mentioned above, or use the Explorer->Open context menu on the DbEdit.dbecfg file.