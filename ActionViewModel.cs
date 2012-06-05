// Copyright (c) 2012 JOAT Services, Jim Wallace
// See the file license.txt for copying permission.using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace DbEdit
{
    internal class SelectedFile : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        internal SelectedFile(string fn, bool set = false)
        {
            FileName = fn;
            Selected = set;
        }
        public String FileName { get; set; }
        private bool _selected;
        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (this._selected != value)
                {
                    this._selected = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Selected"));
                }
                else
                {
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Selected"));
                }
            }
        }

        public override string ToString()
        {
            return String.Format("{0} {1}", Selected ? "x" : "o", FileName);
        }
    }

    internal class ActionViewModel
    {
        private Program _model;

        internal ActionViewModel(Program m, Settings s, IList<SelectedFile> files)
        {
            Files = files;
            Settings = s;
            _model = m;
        }

        public Settings Settings;
        public bool DeleteAllBeforeGet 
        {
            get { return Settings.DeleteAllBeforeGet; }
            set { Settings.DeleteAllBeforeGet = value; } 
        }
        public bool FormatXmlOnGet
        {
            get { return Settings.FormatXmlOnGet; }
            set { Settings.FormatXmlOnGet = value; }
        }
        public IList<SelectedFile> Files { get; set; }


        internal int Upload()
        {
            return _model.Save(Files.Where(o => o.Selected == true).Select(o => o.FileName));
        }

        internal void Download()
        {
            _model.GetAll(Files.Select(o => o.FileName),DeleteAllBeforeGet);
        }

        internal void LaunchFile(string fname )
        {
            _model.Launch(fname);
        }
    }
}
