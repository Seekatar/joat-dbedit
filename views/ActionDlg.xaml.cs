// Copyright (c) 2012 JOAT Services, Jim Wallace
// See the file license.txt for copying permission.using System;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DbEdit
{
    /// <summary>
    /// Interaction logic for ActionDlg.xaml
    /// </summary>
    public partial class ActionDlg : Window
    {
        private ActionViewModel _model;

        internal ActionDlg(ActionViewModel avm)
        {
            _model = avm;

            InitializeComponent();

            lblTitle.Content = String.Format("For {0}:", _model.Settings.Name);
            DataContext = _model;
        }

        private void Option_Click(object sender, RoutedEventArgs e)
        {
            Options opt = new Options(_model.Settings);
            opt.ShowDialog();
        }

        private void File_Checked(object sender, RoutedEventArgs e)
        {
            bool found = false;
            var curr = dgFiles.CurrentItem;
            foreach (var i in dgFiles.SelectedItems)
            {
                if (curr == i)
                {
                    found = true;
                    break;
                }
            }

            if (found)
            {
                foreach (var i in dgFiles.SelectedItems)
                {
                    (i as SelectedFile).Selected = (sender as CheckBox).IsChecked.Value;
                }
            }
        }

        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            if (_model.Files.Count(o => o.Selected) == 0)
                TaskDialog.ShowMsg("No items selected.", icon: TaskDialogIcon.Information);
            else
            {
                int count = _model.Upload();
                TaskDialog.ShowMsg(String.Format("Uploaded {0} file{1}", count, count == 0 ? "" : "s"), icon:TaskDialogIcon.Information);
                this.Close();
            }
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            if (!_model.DeleteAllBeforeGet || TaskDialog.ShowMsg(this, DbEdit.Resources.MsgConfirmDelete, 
                        "This will delete all the files in this folder of the type retrieved by this config file.",
                        buttons:TaskDialogButtons.YesNoCancel, icon:TaskDialogIcon.Warning) == TaskDialogResult.Yes )
            {
                _model.Download();
                this.Close();
            }
        }

        private bool _activatedOnce = false;
        private void Window_Activated(object sender, EventArgs e)
        {
            if (!_activatedOnce && !_model.Settings.Loaded)
            {
                _activatedOnce = true;
                System.Windows.Forms.FolderBrowserDialog ofd = new System.Windows.Forms.FolderBrowserDialog();
                ofd.RootFolder = System.Environment.SpecialFolder.MyComputer;
                ofd.SelectedPath = System.Environment.CurrentDirectory;
                ofd.Description = DbEdit.Resources.GetFolderDesc;

                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Settings s = new Settings();
                    s.ConfigFile = Path.Combine(ofd.SelectedPath, Settings.DefaultConfigFileName);
                    Options od = new Options(s);
                    bool? odret = od.ShowDialog();
                    if (odret.HasValue && odret.Value)
                        _model.Settings.Load(Path.Combine(ofd.SelectedPath, Settings.DefaultConfigFileName));
                    else
                        Close();
                }
                else
                    Close();
            }
        }

    }
}
