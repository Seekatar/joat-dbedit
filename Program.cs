// Copyright (c) 2012 JOAT Services, Jim Wallace
// See the file license.txt for copying permission.using System;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Transactions;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using System.Linq;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.ObjectModel;

namespace DbEdit
{
    /// <summary>
    /// program for using a config XML file to get data to and from db 
    /// </summary>
    internal class Program
    {
        private Settings _settings = new Settings(); // final settings

        /// <summary>
        /// dump out some help
        /// </summary>
        static void ShowHelpAndExit()
        {
            TaskDialog.ShowMsg(String.Format(Resources.MsgFormatHelp,Settings.DefaultConfigFileName), buttons:TaskDialogStandardButtons.Ok, icon:TaskDialogStandardIcon.Information);
            System.Environment.Exit(7);
        }

        /// <summary>
        /// try loading the XML doc, checking the root name to see if it's ours
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        static bool tryLoadXml(string fname, out XDocument doc)
        {
            doc = null;
            try
            {
                doc = XDocument.Load(fname);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// mainline creates the app and runs it
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [STAThread]
        static int Main(string[] args)
        {
            char firstChar = 'h';

            if (args.Length > 0 && args[0].Length > 1 && args[0][0] == '-')
                firstChar = args[0].ToLower()[1];

            Program p = new Program();

            int ret = 9;
            try
            {
                if ( args.Length == 0 )
                {
                    args = new String[1] { Path.Combine(System.Environment.CurrentDirectory, Settings.DefaultConfigFileName) };
                }

                if (args.Length == 1 && String.Equals(Path.GetFileName(args[0]), Settings.DefaultConfigFileName, StringComparison.CurrentCultureIgnoreCase))
                {
                    p.ShowDialog(args[0]);
                    //p.bulkLoad(args[0]);
                }
                else
                {
                    switch (firstChar)
                    {
                        case 'l':
                            if (args.Length < 4) 
                                ShowHelpAndExit();
                            bool convertXml = args[0].Length > 2 && args[0].ToLower()[2] == 'x';

                            ret = p.load(args[1], args[2], args[3], convertXml);
                            break;
                        case 's':
                            if (args.Length < 4) 
                                ShowHelpAndExit();
                            ret = p.SaveFile(args[1],args[2],args[3]);
                            break;
                        default:
                            ShowHelpAndExit();
                            break;
                    }
                }
            }
            catch (Exception e)
            {
#if DEBUG
                throw e;
#else
                MessageBox.Show("Got exception:" + e.GetType().Name + Environment.NewLine + e.Message);
#endif
            }

            return ret;
        }

        /// <summary>
        /// show the dialog to allow the user to gt/save/configure
        /// </summary>
        /// <param name="configFileName"></param>
        private void ShowDialog(string configFileName)
        {
            var files = new ObservableCollection<SelectedFile>();
            _settings = new Settings();
            if (_settings.Load(configFileName))
            {
                PopulateFiles(Path.GetDirectoryName(configFileName), files);
            }
            var ad = new ActionDlg(new ActionViewModel(this, _settings, files));
            ad.ShowDialog();
        }

        /// <summary>
        /// populate the list of selected files based on extension, setting selected flag
        /// </summary>
        /// <param name="p"></param>
        /// <param name="files"></param>
        public void PopulateFiles(string p, IList<SelectedFile> files)
        {
            // get the list of files 
            foreach (var f in Directory.EnumerateFiles(p))
            {
                if (!Path.GetExtension(f).Equals(Settings.DefaultConfigFileNameExtension, StringComparison.CurrentCultureIgnoreCase) && 
                    (String.IsNullOrWhiteSpace(_settings.Suffix) || 
                    (Path.GetExtension(f).Equals(_settings.Suffix, StringComparison.CurrentCultureIgnoreCase))))
                {
                    files.Add( new SelectedFile( Path.GetFileName(f), (File.GetAttributes(f) & FileAttributes.Archive) != 0 ));
                }
            }

        }

        /// <summary>
        /// load the config file
        /// </summary>
        /// <param name="defaultFname"></param>
        private bool loadSettings(string defaultFname)
        {
            return _settings.Load(defaultFname);
        }

        /// <summary>
        /// parse out the keys from the filename
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private string[] getKeys(string fname)
        {
            string[] parms = null;
            string keys = Path.GetFileName(fname);
            int stopPos = keys.IndexOf(_settings.StopChar);
            if (stopPos > 0)
                keys = keys.Substring(0, stopPos);

            if (String.IsNullOrEmpty(_settings.Delimiter))
                parms = new string[1] { keys };
            else
                parms = keys.Split(_settings.Delimiter.ToCharArray());

            if (parms.Length == 0)
            {
                TaskDialog.ShowMsg(Resources.MsgMissingName);
                return null;
            }
            return parms;
        }

        /// <summary>
        /// build the SQL command, parsing the keys and setting the Parameters
        /// </summary>
        /// <param name="fname"></param>
        /// <param name="cmdStr"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        SqlCommand buildCommand(string fname, string cmdStr, SqlConnection connection)
        {
            string[] parms = getKeys(fname);
            if (parms == null)
                return null;

            var command = new SqlCommand(cmdStr, connection);
            for (int i = 0; i < parms.Length; i++)
            {
                command.Parameters.AddWithValue(String.Format("@key{0}", i), parms[i]);
            }

            return command;
        }

        /// <summary>
        /// save the changed file back to the database
        /// args are: -s <source> <dest> <orig>
        /// which in BC is -s %s %t %n
        /// source is temp file BC wrote 
        /// dest is temp file BC wants us to write
        /// orig is the original file, with connection string if for config
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        internal int SaveFile(string sourceFile, string destinationFile = null, string originalFile = null )
        {
            if (originalFile == null)
                originalFile = sourceFile;

            if (File.Exists(originalFile))
            {
                string fname = originalFile;
                string defaultFname = Path.Combine(Path.GetDirectoryName(originalFile), Settings.DefaultConfigFileName);

                if (loadSettings(defaultFname))
                {
                    if (String.IsNullOrWhiteSpace(_settings.Update))
                    {
                        TaskDialog.ShowMsg(Resources.MsgNoUpdate, icon: TaskDialogStandardIcon.Warning);
                        return 9;
                    }
                    var connection = new SqlConnection(_settings.ConnectionStr);
                    connection.Open();

                    SqlCommand command = buildCommand(originalFile, _settings.Update, connection);
                    if (command == null)
                    {
                        TaskDialog.ShowMsg(Resources.MsgSqlFailed, icon: TaskDialogStandardIcon.Warning);
                        return 9;
                    }

                    string content = File.ReadAllText(sourceFile);
                    command.Parameters.AddWithValue("@content", content);
                    try
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            int rows = command.ExecuteNonQuery();
                            if (rows == 1)
                            {
                                ts.Complete();
                                // clear the archive bit
                                File.SetAttributes(sourceFile, File.GetAttributes(sourceFile) & ~FileAttributes.Archive);

                            }
                            else
                                TaskDialog.ShowMsg(String.Format(Resources.MsgFormatUpdated, rows));
                        }
                    }
                    catch (Exception e)
                    {
                        TaskDialog.ShowMsg(e.ToString());
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
                if (destinationFile != null)
                {
                    // just copy it to dest file
                    File.WriteAllText(destinationFile, File.ReadAllText(sourceFile));

                    // clear the archive bit
                    File.SetAttributes(destinationFile, File.GetAttributes(destinationFile) & ~FileAttributes.Archive);
                }
            }
            else
            {
                TaskDialog.ShowMsg(String.Format(Resources.MsgFormatNotExist));
                return 9;
            }


            return 0;
        }

        internal void GetAll( IEnumerable<string> files, bool deleteAllBeforeGet = false )
        {
            if (deleteAllBeforeGet)
            {
                string dir = Path.GetDirectoryName(_settings.ConfigFile);
                foreach (var f in files)
                {
                    File.Delete(Path.Combine(dir, f));
                }
            }
            bulkLoad(_settings.ConfigFile);
        }

        /// <summary>
        /// load the config from the db, or copy xml
        /// args are: -l <source> <dest> <orig>
        /// which in BC is -l %s %t %n
        /// source is original file
        /// dest is temp file BC wants us to write
        /// orig is also the original file
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        int load(string sourceFile, string destinationFile, string originalFile, bool convertXml)
        {
            if (File.Exists(originalFile))
            {
                XDocument doc;

                string fname = originalFile;
                string defaultFname = Path.Combine(Path.GetDirectoryName(originalFile), Settings.DefaultConfigFileName);
                if (loadSettings(defaultFname))
                {
                    // try to get from db
                    try
                    {
                        var connection = new SqlConnection(_settings.ConnectionStr);
                        connection.Open();

                        SqlCommand command = buildCommand(sourceFile, _settings.Select, connection);
                        if (command == null)
                            return 9;

                        var MyReader = command.ExecuteReader();

                        if (MyReader.Read())
                        {
                            // format XML, if it parses
                            try
                            {
                                doc = XDocument.Parse(MyReader[0].ToString());
                                File.WriteAllText(destinationFile, doc.ToString().Replace("xmlns:", Environment.NewLine + "    xmlns:")); // write formatted to to output file.
                            }
                            catch (XmlException)
                            {
                                // not XML TaskDialog.ShowMsg(MyReader[0].ToString());
                                File.WriteAllText(destinationFile, MyReader[0].ToString());
                            }
                        }
                        else
                            TaskDialog.ShowMsg(Resources.MsgFailedToGetConfig);

                        MyReader.Close();
                        connection.Close();
                    }
                    catch (Exception e)
                    {
                        TaskDialog.ShowMsg(Resources.MsgLoadException + e.ToString());
                    }
                    // clear archive bit
                    File.SetAttributes(destinationFile, File.GetAttributes(destinationFile) & ~FileAttributes.Archive);
                }
                else if (convertXml && tryLoadXml(originalFile, out doc))
                {
                    string output = doc.ToString();
                    // assume Xml and just copy it, trying to format first with XDocument
                    try
                    {
                        output = doc.ToString().Replace("xmlns:", Environment.NewLine + "    xmlns:");
                    }
                    catch (Exception)
                    {
                    }
                    File.WriteAllText(destinationFile, output);
                }
                else
                {
                    // not in db or XML, just copy it
                    File.WriteAllText(destinationFile, File.ReadAllText(originalFile));
                }
            }

            return 0;
        }

        /// <summary>
        /// load all the files and content from the database
        /// args are: -b <sourceconfig> <destdir>
        /// sourceconfig is a config file with all the settings
        /// destdir is where to put the content files, if not supplied, it is generated
        /// </summary>
        /// This is used as a helper to get all the data from the db for later
        /// comparing.  In BC you can set the OpenWith for the config file
        /// <param name="args"></param>
        /// <returns></returns>
        int bulkLoad(string configFile)
        {
            int count = 0;
            XDocument doc;
            if (!String.Equals(Path.GetFileName(configFile), Settings.DefaultConfigFileName, StringComparison.CurrentCultureIgnoreCase ) )
                TaskDialog.ShowMsg(Resources.MsgFirstNotFile);
            else if (loadSettings(configFile))
            {
                // string off the WHERE clause
                int whereIsIt = _settings.Select.ToUpper().IndexOf("WHERE");
                if (whereIsIt > 0 && _settings.Select.Length > whereIsIt)
                {
                    string dir = Path.GetDirectoryName(configFile);

                    if (!Directory.Exists(dir))
                    {
                        // try to create it
                        try
                        {
                            DirectoryInfo di = Directory.CreateDirectory(dir);
                        }
                        catch (Exception e)
                        {
                            TaskDialog.ShowMsg(String.Format(Resources.MsgFormatDestCreatError, dir, e.Message));
                            return 8;
                        }
                    }

                    string select = _settings.Select.Substring(0, whereIsIt);

                    // try to get from db
                    try
                    {
                        var connection = new SqlConnection(_settings.ConnectionStr);
                        connection.Open();

                        SqlCommand command = new SqlCommand(select, connection);

                        var MyReader = command.ExecuteReader();

                        List<String> fnames = new List<string>();

                        while (MyReader.Read())
                        {
                            if (MyReader.FieldCount < 2)
                            {
                                TaskDialog.ShowMsg(Resources.MsgSelectMissingKey);
                                return 7;
                            }

                            // write the downloaded content
                            string fname = MyReader[1].ToString();

                            for (int i = 2; i < MyReader.FieldCount; i++)
                                fname += _settings.Delimiter + MyReader[i];

                            fname += _settings.Suffix;

                            fname = makeValidFileName(fname);

                            if (fnames.Contains(fname))
                            {

                                TaskDialog.ShowMsg(String.Format(Resources.MsgDuplicate, fname), Resources.MsgDuplicateDetails, icon: TaskDialogStandardIcon.Warning); 
                                continue;
                            }
                            fnames.Add(fname);
                            try
                            {
                                doc = XDocument.Parse(MyReader[0].ToString());
                                File.WriteAllText(Path.Combine(dir, fname), doc.ToString().Replace("xmlns:", Environment.NewLine + "    xmlns:")); // write formatted to to output file.
                            }
                            catch (XmlException)
                            {
                                File.WriteAllText(Path.Combine(dir, fname), MyReader[0].ToString());
                            }
                            // clear archive bit
                            File.SetAttributes(Path.Combine(dir, fname), File.GetAttributes(Path.Combine(dir, fname)) & ~FileAttributes.Archive);

                            count++;
                        }

                        MyReader.Close();
                        connection.Close();
                    }
                    catch (Exception e)
                    {
                        TaskDialog.ShowMsg(Resources.MsgDbLoadException + e.ToString());
                    }
                    TaskDialog.ShowMsg(String.Format(Resources.MsgFormatLoaded, count, count != 1 ? "s" : String.Empty), icon: TaskDialogStandardIcon.Information);

                }
                else
                {
                    TaskDialog.ShowMsg(String.Format(Resources.MsgFormatInvalidSelect, configFile));
                }
            }
            else
            {
                TaskDialog.ShowMsg(String.Format(Resources.MsgFormatBulk, Settings.DefaultConfigFileName), icon: TaskDialogStandardIcon.Information);
            }
            return 0;
        }

        private string makeValidFileName(string fname)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var i in fname)
            {
                if (Path.GetInvalidFileNameChars().Contains(i))
                    sb.Append("_");
                else
                    sb.Append(i);
            }
            return sb.ToString();
        }


        internal int Save(IEnumerable<string> filenames)
        {
            int ret = 0;
            foreach (var s in filenames)
            {
                SaveFile(Path.Combine(Path.GetDirectoryName(_settings.ConfigFile),s));
                ret++;
            }
            return ret;
        }

        internal void Launch(string fname)
        {
            var proc = System.Diagnostics.Process.Start(Path.Combine(Path.GetDirectoryName(_settings.ConfigFile),fname));
        }
    }
}
