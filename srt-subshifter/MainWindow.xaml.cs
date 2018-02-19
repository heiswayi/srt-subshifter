using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace srt_subshifter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string srtFolder;
        private bool isOverwrite;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = "SRT SubShifter v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(2);
        }

        private void BrowseSubFile_Click(object sender, RoutedEventArgs e)
        {
            var fd = new OpenFileDialog();
            fd.InitialDirectory = "C:";
            fd.Filter = "SRT Files |*.srt";
            if (fd.ShowDialog() == true)
            {
                srtFolder = System.IO.Path.GetDirectoryName(fd.FileName);
                SubFile.Text = System.IO.Path.GetFileName(fd.FileName);
            }
        }

        private void IsOverwrite(object sender, RoutedEventArgs e)
        {
            if (Overwrite.IsChecked == true)
                isOverwrite = true;
            else
                isOverwrite = false;
        }

        private void BtnResync_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(srtFolder) || string.IsNullOrEmpty(SubFile.Text))
                return;

            var fullpath = System.IO.Path.Combine(srtFolder, SubFile.Text);
            if (!string.IsNullOrEmpty(fullpath) && !File.Exists(fullpath))
                return;

            double offset;
            if (double.TryParse(TimeShift.Text, out offset))
            {
                if (offset < -59 || offset > 59)
                {
                    MessageBox.Show("Maximum time shift is +/- 59 seconds.", "Validation", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var resync = new ResyncSrt();
                try
                {
                    BtnResync.IsEnabled = false;
                    LblStatus.Content = "Syncing...";
                    resync.GetResyncedSRT(srtFolder, SubFile.Text, offset, isOverwrite);
                }
                catch
                {
                    LblStatus.Content = "Error!";
                    LblStatus.Foreground = new SolidColorBrush(Colors.Red);
                }
                finally
                {
                    BtnResync.IsEnabled = true;
                    LblStatus.Content = "";

                    if (resync.IsComplete)
                    {
                        LblStatus.Content = "Complete";
                        LblStatus.Foreground = new SolidColorBrush(Colors.Green);
                        MessageBox.Show("Subtitle resync is complete!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            else
            {
                MessageBox.Show("Invalid time shift value.", "Validation", MessageBoxButton.OK, MessageBoxImage.Error);
                TimeShift.Text = "0";
            }
        }
    }
}