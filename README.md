# JOAT DbEdit

This little program was written first as an extension for BeyondCompare to compare files that were stored in a SQLServer database.  The files may be any text such as scripts, XML, XAML, etc. 

The current version is more flexible in that it can simply be used to retrieve files from a table, store them as a local file for comparing, editing, etc., and then updating the database with the changes.  
## Basics
DbEdit gets files from a table into a folder as configured in a JOAT DbEdit .dbecfg XML configuration file.  DbEdit has a dialog for editing the file, or it may be edited manually.  The usual mode of operation is to create a folder for each table, and create a configuration file in that folder.  DbEdit will then populate the folder with data from the database. 

When getting the data from the database, your SELECT statement must include data to create a unique file name to avoid collisions in the folder, and to allow for UPDATEs to work.  It may be a compound key, made up of several columns.  See details below about configuring DbEdit.
## Running DbEdit
Since DbEdit is folder-centric, it’s easiest just to use Windows Explorer and double click the config file since the installer sets up a file association with it and DbEdit.  The main dialog will appear allowing you to get or save the files.
### Getting files from the database
To retrieve all the data from the database to your local files, you can use the pass the config file as the first parameter to the converter, or by double clicking the config file will open a dialog to allow getting all the files.  The files will be loaded from the database and save in the same folder as the config file.  When bulk loading the converter strips off the WHERE clause in the `<select>` element and creates file for each row in the folder.  The archive bit is cleared when files are retrieved so on save DbEdit will default to updating only changed files.

There are two options that can be used on get.  One is to delete all the files before the get which is useful if you deleted rows in the database and want to clean up the files, too.  The other option is to format XML files on download.  This was added for allowing easier comparing of XML files since they are normalized when retrieved (useful when using BeyondCompare, see below).
### Saving files to the database
The main dialog will also allow saving of any of the files in the folder that match the configured extension.  Any files changed since the last save will be selected for update.  You may choose to select or unselect any files before updating.
## JOAT DbEdit for use with BeyondCompare
When BeyondCompare is configured to use DbEdit as a converter for the file type, any folder with a DbEdit .dbecfg file in it will attempt to load that file from the database every time the file is compared in BeyondCompare.  
If the file is changed in BeyondCompare, saving file will update the database.  A message box shows any save errors.
When the installer is used a file association is setup for *.dbecfg file.  Opening the config file via BeyondCompare’s Explorer->Open context menu option will retrieve all the data from the database into the config file’s folder.
### BeyondCompare Setup
1.	Start BeyondCompare
2.	Open Tools->File Formats… dialog
3.	Select file format you want to use in the list (E.g. XML)
4.	Click on the Conversion tab 
5.	Select External program
6.	Enter the fully-qualified path for the converter, with the parameters as show in the image below and after the image for cutting and pasting

Loading: `<path>\ JOAT DbEdit\bin\Release\JOAT DbEdit.exe -l %s %t %n`

Saving: `<path>\JOAT DbEdit\bin\Release\JOAT DbEdit.exe -s %s %t %n`

If you want to format XML files on load use –lx instead of -l.  This makes comparing easier
## Editing the Config File in DbEdit
The options button in the UI allows editing of the config file (or you may edit the raw XML).  There are several options for getting the data from the database and creating file names.  Below is a list of all the items in the UI.  The names in parenthesis are the XML names

**Name (name)** is a descriptive name to show in the main DbEdit dialog that’s the name of the object in the database.  For example, Python Script, or Configuration File, etc.

**File Extension (suffix)** is the string to add to the end of files that are created (default is “.xml”).  If the file extension is store in the database already, this can be left blank.

**Key delimiter (key-delimiter)** is the string to separate keys when building or parsing keys from the file name if there are multiple keys to make a unique file name. (default is “-“)

**Key stop (key stop)** is the string to stop parsing when parsing keys from a filename (default is “.”).  This allows additional information to be put in the filename after the keys.  This is basically to tell DbEdit where the end of the file name is.

**Select  (select)** is the SQL SELECT statement to get one and only one row from the database.  The first column must be the file’s content followed by one or more columns that make the unique key.  The keys are used by to create the file names and must be unique.  The where clause will use key value placeholders like @key0…@keyN.  When bulk loading, the WHERE clause is stripped off of the SELECT statement.  See the examples below for details.

**Update (update)** is the SQL UPDATE statement used when updating the database.  @content is a placeholder for the contents of the file and as with the select, the where clause will use @key0…@keyN.
## Python Script Example
Here is a config for Python scripts stored in a table.  The NAME column of the join was the unique file name, with included the “.py” extension which is why suffix is an empty string
``` XML
<JoatDbEdit name="Python Scripts" suffix="" key-delimiter="" key-stop="">
  <connectionString>
    Server=localhost;Initial Catalog=MY_DB;Integrated Security=True;MultipleActiveResultSets=True
  </connectionString>
  <!-- select and update statements -->
  <statements>
    <!-- Must return one column with content -->
    <select>
	SELECT sf.Body, s.NAME
	FROM dbo.SCRIPT_FUNCTION sf JOIN dbo.SEMANTIC s ON sf.SEMANTIC_ID = s.SEMANTIC_ID
	WHERE s.name = @key0
    </select>
    <!-- Must update column with content -->
    <update>
	UPDATE dbo.SCRIPT_FUNCTION
	SET BODY = @content
	FROM dbo.SCRIPT_FUNCTION sf JOIN dbo.SEMANTIC s ON sf.SEMANTIC_ID = s.SEMANTIC_ID
	WHERE s.name = @key0
    </update>
  </statements>
</JoatDbEdit >
```
## Xamlx Example
This config file is for table that stored XAMLX files that required two columns to get a pretty file name.  It did have a numeric unique key, but using that for a file name wouldn’t have been useful.  The name did not include the extension, so the suffix is used.  The file names will be `<key0>-<key1>.xamlx (e.g. helpers-loademployee.xamlx)`
``` XML
<JoatDbEdit name="Workflows" suffix=".xamlx" key-delimiter="-" key-stop=".">
  <connectionString>
    Server=localhost;Initial Catalog=MY_DATABASE;Integrated Security=True;MultipleActiveResultSets=True
  </connectionString>
  <!-- select and update statements -->
  <statements>
    <!-- Must return one column with content -->
    <select>
      SELECT BODY, ps.NAME, s.NAME
      FROM dbo.LOGIC_MODEL lm JOIN dbo.SEMANTIC s ON lm.SEMANTIC_ID = s.SEMANTIC_ID
      JOIN dbo.SEMANTIC ps ON s.PARENT_SEMANTIC_ID = ps.SEMANTIC_ID
      WHERE ps.NAME = @key0 AND s.NAME = @key1
    </select>
    <!-- Must update column with content -->
    <update>
      UPDATE dbo.LOGIC_MODEL
      SET BODY = @content
      FROM dbo.LOGIC_MODEL lm JOIN dbo.SEMANTIC s ON lm.SEMANTIC_ID = s.SEMANTIC_ID
      JOIN dbo.SEMANTIC ps ON s.PARENT_SEMANTIC_ID = ps.SEMANTIC_ID
      WHERE ps.NAME = @key0 AND s.NAME = @key1
    </update>
  </statements>
</JoatDbEdit>
```