// Copyright (c) 2012 JOAT Services, Jim Wallace
// See the file license.txt for copying permission.using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace DbEdit
{
    /// <summary>
    /// helper class to contain settings from XML config file
    /// </summary>
    public class Settings
    {
        private bool _loaded = false;

        /// <summary>
        /// was the config file loaded?
        /// </summary>
        public bool Loaded
        {
            get { return _loaded; }
            set { _loaded = value; }
        }

        // Default values, must be public properties for binding
        private string _name = "Unnamed Objects";

        /// <summary>
        /// Descriptive name of the column shown in the main dialog.  E.g. Python Scripts
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        private string _delimiter = "-";

        /// <summary>
        /// Delimiter between multiple keys
        /// </summary>
        public string Delimiter
        {
            get { return _delimiter; }
            set { _delimiter = value; }
        }
        private string _stopChar = ".";

        /// <summary>
        /// Char to stop parsing in key to get the name.
        /// </summary>
        public string StopChar
        {
            get { return _stopChar; }
            set { _stopChar = value; }
        }
        private string _select =
@"SELECT MY_TEXT_COLUMN, MY_KEY_0, MY_KEY_1
FROM MY_TABLE_NAME
WHERE MY_KEY0 = @key0 AND MY_KEY1 = @key1";

        /// <summary>
        /// SQL Select statement to get data.  See documentation for details.
        /// </summary>
        public string Select
        {
            get { return _select; }
            set { _select = value; }
        }
        private string _update = 
@"UPDATE MY_TABLE_NAME
SET MY_TEXT_COLUMN = @content
WHERE MY_KEY0 = @key0 AND MY_KEY1 = @key1";

        /// <summary>
        /// SQL Update statement to set data.  See documentation for details.
        /// </summary>
        public string Update
        {
            get { return _update; }
            set { _update = value; }
        }
        private string _connectionStr = "Server=mySqlServer;Initial Catalog=myDataBase;Integrated Security=True;MultipleActiveResultSets=True;";

        /// <summary>
        /// Connection string for the database
        /// </summary>
        public string ConnectionStr
        {
            get { return _connectionStr; }
            set { _connectionStr = value; }
        }
        private string _suffix = ".xml";

        /// <summary>
        /// Extension added to key values.  Many be blank if name in database contains extension.
        /// </summary>
        public string Suffix
        {
            get { return _suffix; }
            set { _suffix = value; }
        }
        private bool _deleteAllBeforeGet = false;

        /// <summary>
        /// Delete all the files with the configured extension before getting.  If extension is empty, all files are deleted
        /// </summary>
        public bool DeleteAllBeforeGet
        {
            get { return _deleteAllBeforeGet; }
            set { _deleteAllBeforeGet = value; }
        }
        private bool _formatXmlOnGet = true;

        public bool FormatXmlOnGet
        {
            get { return _formatXmlOnGet; }
            set { _formatXmlOnGet = value; }
        }

        internal static string RootNode = "JoatDbEdit"; // pick an unlikely name to avoid collisions
        internal static string DefaultConfigFileNameExtension = ".dbecfg";
        internal static string DefaultConfigFileName = "DbEdit" + DefaultConfigFileNameExtension;
     

        internal string ConfigFile;

        /// <summary>
        /// read the values from the config file, if they exist
        /// </summary>
        /// <param name="doc"></param>
        private bool readValues(XDocument doc, bool defaultLoad = false)
        {
            XAttribute attr = doc.Root.Attribute("key-Delimiter");
            if (attr != null) // allow space & empty string
                Delimiter = attr.Value.Trim();

            attr = doc.Root.Attribute("key-stop");
            if (attr != null) // allow space & empty string
                StopChar = attr.Value.Trim();

            attr = doc.Root.Attribute("suffix");
            if (attr != null) // allow space & empty string
                Suffix = attr.Value.Trim();

            attr = doc.Root.Attribute("name");
            if (attr != null) // allow space & empty string
                _name = attr.Value.Trim();

            attr = doc.Root.Attribute("deleteAllBeforeGet");
            if (attr != null) // allow space & empty string
                DeleteAllBeforeGet = attr.Value != "0";

            attr = doc.Root.Attribute("formatXmlOnGet");
            if (attr != null) // allow space & empty string
                FormatXmlOnGet = attr.Value != "0";

            XElement e = doc.Descendants("connectionString").FirstOrDefault();
            if (e != null && !String.IsNullOrWhiteSpace(e.Value))
                ConnectionStr = e.Value.Trim();

            e = doc.Descendants("select").FirstOrDefault();
            if (e != null && !String.IsNullOrWhiteSpace(e.Value))
                Select = e.Value.Trim();

            e = doc.Descendants("update").FirstOrDefault();
            if (e != null && !String.IsNullOrWhiteSpace(e.Value))
                Update = e.Value.Trim();

            return !defaultLoad ||
                   (!String.IsNullOrWhiteSpace(ConnectionStr) &&
                    !String.IsNullOrWhiteSpace(Select) &&
                    !String.IsNullOrWhiteSpace(Update));
        }

        /// <summary>
        /// load the config from the default file (config)
        /// </summary>
        /// <param name="defaultFname"></param>
        internal bool Load(string defaultFname)
        {
            Loaded = false;
            ConfigFile = defaultFname;
            if (File.Exists(defaultFname))
            {
                XDocument doc2 = XDocument.Load(defaultFname);
                if (readValues(doc2, true))
                {
                    Loaded = true;
                }
            }
            return Loaded;
        }

        internal void Save(string fname)
        {
            XDocument doc = new XDocument();
            XElement root = new XElement(RootNode);
            root.Add(new XAttribute("name", _name),
                        new XAttribute("key-Delimiter", Delimiter),
                        new XAttribute("key-stop", StopChar),
                        new XAttribute("suffix", Suffix),
                        new XAttribute("deleteAllBeforeGet", DeleteAllBeforeGet),
                        new XAttribute("formatXmlOnGet", FormatXmlOnGet),
                        new XElement("connectionString", ConnectionStr),
                        new XElement("select", Select),
                        new XElement("update", Update));
            doc.Add(root);
            doc.Save(fname);
        }
    }

}
