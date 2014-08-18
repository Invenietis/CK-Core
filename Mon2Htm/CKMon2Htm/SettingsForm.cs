#region LGPL License
/*----------------------------------------------------------------------------
* This file (Mon2Htm\CKMon2Htm\SettingsForm.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
            if( entriesPerPage <= 0 ) entriesPerPage = 500;

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
            var rootDirectory = new DirectoryInfo( MainForm.GetRootTempFolder() );
            if( !rootDirectory.Exists ) return;

            foreach( var directory in rootDirectory.GetDirectories() )
            {
                try
                {
                    directory.Delete( true );
                }
                catch( Exception e )
                {
                    Console.WriteLine( e.ToString() );
                }
            }
            foreach( var file in rootDirectory.GetFiles() )
            {
                try
                {
                    file.Delete();
                }
                catch( Exception e )
                {
                    Console.WriteLine( e.ToString() );
                }
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

            // If the entry count changes, existing files are no longer valid.
            if( entriesPerPage != settings.EntriesPerPage ) ClearCache();

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
