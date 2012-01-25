// Copyright (c) 2012 JOAT Services, Jim Wallace
// See the file license.txt for copying permission.using System;
using System;
using System.Windows;

namespace DbEdit
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        Settings _model;
        public Options(Settings model)
        {
            _model = model;

            InitializeComponent();

            DataContext = _model;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            _model.Save(_model.ConfigFile);
            Close();
        }

        private bool _activatedOnce = false;
        private void Window_Activated(object sender, EventArgs e)
        {
            if (!_activatedOnce && !_model.Loaded)
            {
                _activatedOnce = true;
                TaskDialog.ShowMsg(DbEdit.Resources.MsgFirstTime, icon: TaskDialogIcon.Information);
            }
        }
    }
}
