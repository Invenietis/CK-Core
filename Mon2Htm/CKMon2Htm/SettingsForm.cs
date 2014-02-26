using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CK.Mon2Htm
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            ReadSettings();
        }

        private void ReadSettings()
        {
            var settings = Properties.Settings.Default;

            bool monitorDirectories = settings.MonitorDirectory;
            bool autoAddFiles = settings.AutoSelectNewFiles;
            bool autoRefreshPage = settings.AutoRefreshPages;
            int entriesPerPage = settings.EntriesPerPage;

            this.watchFsCheckBox.Checked = monitorDirectories;
            this.addNewFilesCheckBox.Checked = autoAddFiles;
            this.refreshLogsCheckBox.Checked = autoRefreshPage;
            this.entriesPerPageNumericUpDown.Value = entriesPerPage;

            RefreshCacheSize();
            UpdateCheckboxAvailability();
        }

        private void UpdateCheckboxAvailability()
        {
            this.addNewFilesCheckBox.Enabled = this.watchFsCheckBox.Checked ? true : false;
            this.addNewFilesCheckBox.Checked = this.addNewFilesCheckBox.Enabled ? this.addNewFilesCheckBox.Checked : false;
        }

        private void RefreshCacheSize()
        {
            double cacheSize = CalculateCacheSize();
            this.clearCacheButton.Enabled = (cacheSize > 0);

            this.spaceUsedLabel.Text = String.Format( "Cache size used: {0}", HumanizeByteLength( cacheSize ) );
        }

        private void ClearCache()
        {
            var rootDirectory = new DirectoryInfo(MainForm.GetRootTempFolder());
            if( !rootDirectory.Exists ) return;

            foreach( var directory in rootDirectory.GetDirectories() )
            {
                directory.Delete( true );
            }
            foreach( var file in rootDirectory.GetFiles() )
            {
                file.Delete();
            }
            RefreshCacheSize();
        }

        private double CalculateCacheSize()
        {
            string ckmonFolder = MainForm.GetRootTempFolder();

            DirectoryInfo di = new DirectoryInfo( ckmonFolder );

            return CalculateDirectorySize( di );
        }

        private static string HumanizeByteLength( double len )
        {
            string[] sizes = { "B", "KB", "MB", "GB" };

            int order = 0;
            while( len >= 1024 && order + 1 < sizes.Length )
            {
                order++;
                len = len / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            string result = String.Format( "{0:0.##} {1}", len, sizes[order] );

            return result;
        }

        private double CalculateDirectorySize( DirectoryInfo di )
        {
            double dirSize = 0;
            try
            {
                if( !di.Exists ) return dirSize;

                foreach( var file in di.GetFiles() )
                {
                    try
                    {
                        dirSize += file.Length;
                    }
                    catch( Exception e )
                    {
                        Console.WriteLine( e.ToString() );
                    }
                }
                foreach( var directory in di.GetDirectories() )
                {
                    try
                    {
                        dirSize += CalculateDirectorySize( directory );
                    }
                    catch( Exception e )
                    {
                        Console.WriteLine( e.ToString() );
                    }
                }

                return dirSize;
            }
            catch( Exception e )
            {
                Console.WriteLine( e.ToString() );
            }
            return dirSize;
        }

        private void clearCacheButton_Click( object sender, EventArgs e )
        {
            ClearCache();
        }

        private void saveAndCloseButton_Click( object sender, EventArgs e )
        {
            SaveSettings();
            this.Close();
        }

        private void SaveSettings()
        {
            bool monitorDirectory = this.watchFsCheckBox.Checked;
            bool autoSelectFiles = this.watchFsCheckBox.Checked ? this.addNewFilesCheckBox.Checked : false;
            bool autoRefresh = this.refreshLogsCheckBox.Checked;

            int entriesPerPage = Convert.ToInt32( this.entriesPerPageNumericUpDown.Value );

            var settings = Properties.Settings.Default;

            settings.MonitorDirectory = monitorDirectory;
            settings.AutoSelectNewFiles = autoSelectFiles;
            settings.AutoRefreshPages = autoRefresh;
            settings.EntriesPerPage = entriesPerPage;

            settings.Save();
        }

        private void button1_Click( object sender, EventArgs e )
        {
            this.Close();
        }

        private void watchFsCheckBox_CheckedChanged( object sender, EventArgs e )
        {
            UpdateCheckboxAvailability();
        }
    }
}
