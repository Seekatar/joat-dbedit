// Copyright (c) 2012 JOAT Services, Jim Wallace
// See the file license.txt for copying permission.using System;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace DbEdit
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
            FileVersionInfo fi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            Version.Text = String.Format("{0}.{0}", fi.ProductMajorPart, fi.ProductMinorPart );
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Link.NavigateUri.AbsoluteUri);
        }
    }
}
