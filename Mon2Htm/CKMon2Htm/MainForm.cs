#region LGPL License
/*----------------------------------------------------------------------------
* This file (Mon2Htm\CKMon2Htm\MainForm.cs) is part of CiviKey. 
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
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using CK.Core;
using CK.Monitoring;

namespace CK.Mon2Htm
{
    public partial class MainForm : Form
    {
        readonly IActivityMonitor _m;
        readonly List<string> _listedFiles;
        readonly List<string> _filesToLoad;

        bool _hasUpdatedResources;
        bool _debug = false;
        string _loadedDirectory;
        string _tempDirPath;
        string _baseTitle;
        BackgroundWorker _bw;
        FileSystemWatcher _dirWatcher;

        protected override void WndProc( ref Message m )
        {
            if( m.Msg == NativeMethods.WM_SHOWME )
            {
                ShowMe();
            }
            base.WndProc( ref m );
        }

        /// <summary>
        /// Manual activation of the window.
        /// </summary>
        private void ShowMe()
        {
            if( InvokeRequired )
            {
                BeginInvoke( (MethodInvoker)delegate() { ShowMe(); } );
                return;
            }

            if( WindowState == FormWindowState.Minimized )
            {
                WindowState = FormWindowState.Normal;
            }

            bool top = TopMost;
            TopMost = true;
            TopMost = top;

            Activate();

            LoadFilesFromStash();
        }

        void LoadFilesFromStash()
        {
            string[] filesInStash = Program.ReadStashFiles().ToArray();

            if( filesInStash.Length > 0 ) LoadPath( filesInStash );
        }

        public MainForm()
        {
#if DEBUG
            _debug = true;
#endif
            _listedFiles = new List<string>();
            _filesToLoad = new List<string>();
            _hasUpdatedResources = false;

            _m = new ActivityMonitor();
            //_m.Output.RegisterClient( new ActivityMonitorConsoleClient() );
            _m.Info().Send( "Starting CKMon2Htm." );

            InitializeComponent();

            // "Disable" selection (remove color effect)
            this.dataGridView1.DefaultCellStyle.SelectionBackColor = this.dataGridView1.DefaultCellStyle.BackColor;
            this.dataGridView1.DefaultCellStyle.SelectionForeColor = this.dataGridView1.DefaultCellStyle.ForeColor;


            this.Load += MainForm_Load;
        }

        void MainForm_Load( object sender, EventArgs e )
        {
            LoadFromProgramArguments();

            UpdateButtonState();
            UpdateVersionLabel();

            _baseTitle = this.Text;

            UpdateTitle();
        }

        private void WatchDirectory( string directoryPath )
        {
            CloseDirWatcher();

            _dirWatcher = new FileSystemWatcher( directoryPath, "*.ckmon" );
            _dirWatcher.Deleted += _dirWatcher_Deleted;
            _dirWatcher.Created += _dirWatcher_Created;
            _dirWatcher.Renamed += _dirWatcher_Renamed;

            _dirWatcher.SynchronizingObject = this;
            _dirWatcher.IncludeSubdirectories = true;

            NotifyFilters notificationFilters = new NotifyFilters();
            notificationFilters = notificationFilters | NotifyFilters.FileName;

            _dirWatcher.NotifyFilter = notificationFilters;
            _dirWatcher.EnableRaisingEvents = true;
        }

        private void CloseDirWatcher()
        {
            if( _dirWatcher != null )
            {
                _dirWatcher.EnableRaisingEvents = false;

                _dirWatcher.Dispose();
                _dirWatcher = null;
            }
        }

        private bool RequestOpenCkmon()
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Title = "Open log file";
            d.Filter = "Activity Monitor log files (.ckmon)|*.ckmon";
            d.FilterIndex = 0;
            d.CheckFileExists = true;
            d.CheckPathExists = true;
            d.AutoUpgradeEnabled = true;
            d.Multiselect = true;
            d.InitialDirectory = Properties.Settings.Default.LastOpenDirectory;

            var dialogResult = d.ShowDialog();

            if( dialogResult == System.Windows.Forms.DialogResult.OK )
            {
                LoadPath( d.FileNames );
                return true;
            }
            else
            {
                return false;
            }
        }

        private void UpdateVersionLabel()
        {
            Version pubVersion = GetPublishedVersion();
            if( pubVersion == null )
            {
                this.versionLabel.Text = String.Format( "Dev ({0})", Assembly.GetExecutingAssembly().GetName().Version.ToString() );
            }
            else
            {
                this.versionLabel.Text = pubVersion.ToString();
            }

#if DEBUG
            this.versionLabel.Text = this.versionLabel.Text + " DEBUG";
#endif
        }

        private void LoadFromProgramArguments()
        {
            string[] args = Environment.GetCommandLineArgs();
            string[] activationData = AppDomain.CurrentDomain.SetupInformation.ActivationArguments == null ? null : AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData;

            if( activationData != null && activationData.Length > 0 ) // ClickOnce file parameters
            {
                string arg = activationData[0];
                LoadPath( arg );

                this.viewHtmlButton.Focus();
            }
            else if( args.Length > 1 )
            {
                string arg = args[1];
                LoadPath( arg );

                this.viewHtmlButton.Focus();
            }
            else
            {
                bool hasSelectedFile = RequestOpenCkmon();

                if( !hasSelectedFile ) this.Close();
            }
            this.Activate();
        }

        /// <summary>
        /// Gets the current published version.
        /// </summary>
        /// <returns>Published version (ClickOnce publishing version), or null when executed outside ClickOnce context.</returns>
        private static Version GetPublishedVersion()
        {
            if( ApplicationDeployment.IsNetworkDeployed )
            {
                return ApplicationDeployment.CurrentDeployment.CurrentVersion;
            }
            return null;
        }

        private void LoadPath( string path )
        {
            LoadPath( new string[] { path } );
        }

        /// <summary>
        /// Loads file or directory path given as argument.
        /// </summary>
        /// <param name="path">File or directory path</param>
        private void LoadPath( string[] path )
        {
            if( path.Length > 0 && (Directory.Exists( path[0] ) || File.Exists( path[0] ) ))
            {
                LoadDirectory( path[0] );

                foreach (var item in path)
	            {
                    if( File.Exists( item ) )
                    {
                        SelectFile( item );

                        var row = GetRowOfFilePath( item );
                        this.dataGridView1.FirstDisplayedScrollingRowIndex = this.dataGridView1.Rows.IndexOf( row );
                    }
                }
            }
            else
            {
                MessageBox.Show( String.Format( "File {0} could not be found. Select another log file.", path ) );
                bool hasSelectedFile = RequestOpenCkmon();

                if( !hasSelectedFile ) this.Close();
            }
        }

        private void LoadDirectory( string path )
        {
            SetLoadedDirectory( path );

            Properties.Settings.Default.LastOpenDirectory = Path.GetDirectoryName( path );
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Adds file to internal collections and adds a row.
        /// </summary>
        /// <param name="filePath">File to add</param>
        /// <param name="addSelected">Select the file when adding it</param>
        /// <returns>Index of the new row in the list, or -1 on failure.</returns>
        private int AddFile( string filePath, bool addSelected = false )
        {
            if( _listedFiles.Contains( filePath ) ) return -1;
            _listedFiles.Add( filePath );

            if( addSelected && !_filesToLoad.Contains( filePath ) ) _filesToLoad.Add( filePath );

            UpdateButtonState();

            return AddFileRow( filePath, addSelected );
        }

        /// <summary>
        /// Removes file from internal collections and rows.
        /// </summary>
        /// <param name="filePath">File to remove</param>
        private void RemoveFile( string filePath )
        {
            if( _filesToLoad.Contains( filePath ) ) _filesToLoad.Remove( filePath );
            if( _listedFiles.Contains( filePath ) ) _listedFiles.Remove( filePath );

            var row = GetRowOfFilePath( filePath );
            if( row != null ) this.dataGridView1.Rows.Remove( row );

            UpdateButtonState();
        }

        /// <summary>
        /// Creates a row from a file path
        /// </summary>
        /// <param name="filePath">File to add</param>
        /// <param name="viewSelected">Select the view checkbox when adding it</param>
        /// <returns>Index of the new row</returns>
        private int AddFileRow( string filePath, bool viewSelected = false )
        {
            var row = new DataGridViewRow();

            var viewCheckbox = new DataGridViewCheckBoxCell();
            viewCheckbox.TrueValue = true;
            viewCheckbox.FalseValue = false;
            viewCheckbox.Value = viewSelected;

            var fileCell = new DataGridViewTextBoxCell();
            fileCell.Tag = filePath;
            var relativePath = GetRelativePathOfFile( filePath );

            fileCell.Value = relativePath.Substring( 0, relativePath.Length - 6 );

            row.Cells.Add( viewCheckbox );
            row.Cells.Add( fileCell );

            return this.dataGridView1.Rows.Add( row );
        }

        /// <summary>
        /// Adds a directory's .ckmon files to the file list
        /// </summary>
        /// <param name="directoryPath">Directory to scan</param>
        /// <param name="recurse">Recurse into subdirectories</param>
        private void AddDirectoryFiles( string directoryPath, bool recurse = true )
        {
            var files = Directory.GetFiles( directoryPath, "*.ckmon", recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly );
            foreach( var file in files ) AddFile( file );
        }

        private void SetLoadedDirectory( string directoryPath )
        {
            if( File.Exists( directoryPath ) ) directoryPath = Path.GetDirectoryName( directoryPath );
            if( !Directory.Exists( directoryPath ) ) throw new DirectoryNotFoundException( String.Format( "Attempted to load on a directory that does not exist: {0}", directoryPath ) );

            _loadedDirectory = directoryPath;
            UpdateTitle();

            if( Properties.Settings.Default.MonitorDirectory ) WatchDirectory( directoryPath );

            AddDirectoryFiles( directoryPath );
            SortGrid();
        }

        private void UpdateTitle()
        {

            if( String.IsNullOrWhiteSpace( _loadedDirectory ) )
            {
                this.Text = _baseTitle;
            }
            else
            {
                DirectoryInfo d = new DirectoryInfo( _loadedDirectory );

                this.Text = String.Format( "{0} ({1}) - Mon2Htm", d.Name, d.FullName );
            }

            if( _debug ) this.Text = String.Format( "{0} (Debug)", this.Text );
        }

        private void SortGrid()
        {
            System.Collections.IComparer comparer = new SortGridHelper();

            this.dataGridView1.Sort( comparer );
        }

        private void SelectFile( string filePath )
        {
            DataGridViewCheckBoxCell cell = GetRowOfFilePath( filePath ).Cells[0] as DataGridViewCheckBoxCell;
            cell.Value = true;
        }

        private DataGridViewRow GetRowOfFilePath( string filePath )
        {
            return this.dataGridView1.Rows.Cast<DataGridViewRow>().Where( x => x.Cells[1].Tag.ToString() == filePath ).FirstOrDefault();
        }

        private string GetRelativePathOfFile( string filePath )
        {
            if( _loadedDirectory == null ) return Path.GetFileName( filePath );
            string loadedDirectory = Path.GetFullPath( _loadedDirectory );
            filePath = Path.GetFullPath( filePath );

            // + 1 removes the first \.
            return filePath.Substring( loadedDirectory.Length + 1 );
        }

        /// <summary>
        /// Generates a HTML structure from the loaded and selected files.
        /// </summary>
        /// <returns>Path of the created index file</returns>
        private string GenerateHtml()
        {
            if( _bw != null ) return null;

            _bw = new BackgroundWorker();
            _bw.ProgressChanged += _bw_ProgressChanged;
            _bw.RunWorkerCompleted += _bw_RunWorkerCompleted;

            try
            {
                this.viewHtmlButton.Enabled = false;
                this.progressBar1.Visible = true;

                MultiLogReader.ActivityMap activityMap;

                using( MultiLogReader r = new MultiLogReader() )
                {
                    r.Add( _filesToLoad );

                    activityMap = r.GetActivityMap();
                }

                _tempDirPath = GetTempFolder( activityMap );
                string rootFolder = GetRootTempFolder();

                string indexFilePath = Path.Combine( _tempDirPath, "index.html" );

                if( _debug ) _m.Info().Send( "CKMon2Htm is running in debug mode and will not cache generated files." );

                if( !_debug || !_hasUpdatedResources || !Directory.Exists( Path.Combine( rootFolder, "css" ) ) )
                {
                    HtmlGenerator.CopyResourcesToDirectory( rootFolder );
                    _hasUpdatedResources = true;
                }

                int entriesPerPage = Properties.Settings.Default.EntriesPerPage;

                if( _debug || !File.Exists( indexFilePath ) )
                {
                    HtmlGenerator gen = new HtmlGenerator( activityMap, _tempDirPath, _m, entriesPerPage, "../" );
                    gen.ConfigureBackgroundWorker( _bw );

                    _bw.RunWorkerAsync();
                    return null;
                }
                else
                {
                    _bw.Dispose();
                    _bw = null;
                    if( File.Exists( indexFilePath ) ) return indexFilePath;
                    else return null;
                }
            }
            catch( Exception ex )
            {
                this.viewHtmlButton.Enabled = true;
                this.viewHtmlButton.Text = @"View HTML";
                this.progressBar1.Visible = false;

                _bw.Dispose();
                _bw = null;

                throw ex;
            }
        }

        void _bw_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
        {
            if( InvokeRequired )
            {
                BeginInvoke( (MethodInvoker)delegate() { _bw_RunWorkerCompleted( sender, e ); } );
                return;
            }

            this.viewHtmlButton.Enabled = true;
            this.viewHtmlButton.Text = @"View HTML";
            this.progressBar1.Visible = false;

            string indexFilePath = e.Result.ToString();

            DoRun( indexFilePath );

            _bw.Dispose();
            _bw = null;
        }

        void _bw_ProgressChanged( object sender, ProgressChangedEventArgs e )
        {
            if( InvokeRequired )
            {
                BeginInvoke( (MethodInvoker)delegate() { _bw_ProgressChanged( sender, e ); } );
                return;
            }

            if( e.ProgressPercentage == -1 )
            {
                this.progressBar1.Value = 0;
                this.progressBar1.Style = ProgressBarStyle.Marquee;
            }
            else
            {
                this.progressBar1.Value = e.ProgressPercentage;
                this.progressBar1.Style = ProgressBarStyle.Continuous;
            }
            if( e.UserState != null ) this.viewHtmlButton.Text = e.UserState.ToString();

        }

        /// <summary>
        /// Attempts to update this app.
        /// </summary>
        /// <remarks>
        /// If the app is used outside its ClickOnce context (eg. when executing it without the ClickOnce wrapper), this does nothing.
        /// </remarks>
        private void InstallUpdateSyncWithInfo()
        {
            UpdateCheckInfo info = null;

            if( ApplicationDeployment.IsNetworkDeployed )
            {
                ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;

                try
                {
                    info = ad.CheckForDetailedUpdate();

                }
                catch( DeploymentDownloadException dde )
                {
                    MessageBox.Show( "The new version of the application cannot be downloaded at this time.\n\nPlease check your network connection, or try again later.\nError: " + dde.Message );
                    return;
                }
                catch( InvalidDeploymentException ide )
                {
                    MessageBox.Show( "Cannot check for a new version of the application.\n\nThe ClickOnce deployment is corrupt. Please redeploy the application and try again.\nError: " + ide.Message );
                    return;
                }
                catch( InvalidOperationException ioe )
                {
                    MessageBox.Show( "This application cannot be updated.\n\nIt is likely not a ClickOnce application.\nError: " + ioe.Message );
                    return;
                }

                if( info.UpdateAvailable )
                {
                    Boolean doUpdate = true;

                    if( !info.IsUpdateRequired )
                    {
                        DialogResult dr = MessageBox.Show( "An update is available. Would you like to update the application now?", "Update Available", MessageBoxButtons.OKCancel );
                        if( !(DialogResult.OK == dr) )
                        {
                            doUpdate = false;
                        }
                    }
                    else
                    {
                        // Display a message that the app MUST reboot. Display the minimum required version.
                        MessageBox.Show( "This application has detected a mandatory update from your current " +
                            "version to version " + info.MinimumRequiredVersion.ToString() +
                            ". The application will now install the update and restart.",
                            "Update Available", MessageBoxButtons.OK,
                            MessageBoxIcon.Information );
                    }

                    if( doUpdate )
                    {
                        try
                        {
                            ad.Update();
                            MessageBox.Show( "The application has been upgraded, and will now restart." );
                            Application.Restart();
                        }
                        catch( DeploymentDownloadException dde )
                        {
                            MessageBox.Show( "Cannot install the latest version of the application. \n\nPlease check your network connection, or try again later. Error: " + dde );
                            return;
                        }
                    }
                }
                else
                {
                    MessageBox.Show( "No update is available at this time." );
                }
            }
        }

        internal static string GetRootTempFolder()
        {
            string tempFolderName = String.Format( "ckmon2htm" );
            string tempFolderPath = Path.Combine( Path.GetTempPath(), tempFolderName );

            DirectoryInfo di = Directory.CreateDirectory( tempFolderPath );

            return tempFolderPath;
        }

        private static string GetActivityMapHash( MultiLogReader.ActivityMap activityMap )
        {
            StringBuilder sb = new StringBuilder();
            sb.Append( activityMap.FirstEntryDate.ToString() );
            sb.Append( activityMap.LastEntryDate.ToString() );
            foreach( var monitor in activityMap.Monitors )
            {
                sb.Append( '_' );
                sb.Append( monitor.MonitorId.ToString() );
                sb.Append( '_' );
                sb.Append( monitor.FirstEntryTime.ToString() );
                sb.Append( '_' );
                sb.Append( monitor.LastEntryTime.ToString() );
            }

            return CalculateMD5Hash( sb.ToString() );
        }

        public static string CalculateMD5Hash( string input )
        {
            MD5 md5 = System.Security.Cryptography.MD5.Create();

            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes( input );
            byte[] hash = md5.ComputeHash( inputBytes );

            StringBuilder sb = new StringBuilder();

            for( int i = 0; i < hash.Length; i++ )
            {
                sb.Append( hash[i].ToString( "X2" ) );
            }

            return sb.ToString();
        }

        /// <summary>
        /// Creates a new, random folder in the user's temporary files.
        /// </summary>
        /// <returns>Created Temporary folder path</returns>
        private string GetTempFolder( MultiLogReader.ActivityMap activityMap )
        {
            string tempFolderName = GetActivityMapHash( activityMap );
            string tempFolderPath = Path.Combine( GetRootTempFolder(), tempFolderName );

            DirectoryInfo di = Directory.CreateDirectory( tempFolderPath );

            return tempFolderPath;
        }

        /// <summary>
        /// Sets the temporary attribute on all files within a directory.
        /// </summary>
        /// <param name="path">Directory to scan</param>
        private static void RecurseTemporaryAttributes( string path )
        {
            var files = Directory.GetFiles( path, "*", SearchOption.AllDirectories );

            foreach( var file in files )
            {
                File.SetAttributes( file, FileAttributes.Temporary );
            }
        }

        /// <summary>
        /// Updates the View HTML button state.
        /// </summary>
        private void UpdateButtonState()
        {
            this.viewHtmlButton.Enabled = _filesToLoad.Count > 0;
        }

        /// <summary>
        /// Sets all View checkboxes to value. (true: checked, false: unchecked)
        /// </summary>
        /// <param name="value"></param>
        private void SetAllViewValues( bool value )
        {
            foreach( var row in this.dataGridView1.Rows )
            {
                DataGridViewRow r = row as DataGridViewRow;

                DataGridViewCheckBoxCell c = r.Cells[0] as DataGridViewCheckBoxCell;

                c.Value = value;
            }
            this.dataGridView1.RefreshEdit(); // Flush CurrentCell value
            this.dataGridView1.InvalidateCell( this.dataGridView1.CurrentCell ); // Repaint it
        }

        #region FileSystemWatcher event handlers
        void _dirWatcher_Renamed( object sender, RenamedEventArgs e )
        {
            bool wasChecked = _filesToLoad.Contains( e.OldFullPath ) || Properties.Settings.Default.AutoSelectNewFiles;
            RemoveFile( e.OldFullPath );

            if( e.FullPath.EndsWith( ".ckmon" ) )
            {
                AddFile( e.FullPath, wasChecked );
                SortGrid();
            }
        }

        void _dirWatcher_Created( object sender, FileSystemEventArgs e )
        {
            Debug.Assert( e.FullPath.EndsWith( ".ckmon" ) );
            AddFile( e.FullPath, Properties.Settings.Default.AutoSelectNewFiles );
            SortGrid();
        }

        void _dirWatcher_Deleted( object sender, FileSystemEventArgs e )
        {
            Debug.Assert( e.FullPath.EndsWith( ".ckmon" ) );
            RemoveFile( e.FullPath );
        }
        #endregion

        #region Control event handlers
        private void viewHtmlButton_Click( object sender, EventArgs e )
        {
            string indexFilePath = GenerateHtml();

            if( indexFilePath != null ) DoRun( indexFilePath );
            // Otherwise, the BackgroundWorker is running and will callback later.
        }

        void DoRun( string indexFilePath )
        {
            RecurseTemporaryAttributes( _tempDirPath );

            if( indexFilePath != null && File.Exists( indexFilePath ) )
            {
                Process.Start( indexFilePath );
            }
        }

        private void dataGridView1_CellValueChanged( object sender, DataGridViewCellEventArgs e )
        {
            if( e.ColumnIndex < 0 || e.RowIndex < 0 ) return; // Called when loading form
            if( e.ColumnIndex == 0 )
            {
                DataGridViewCheckBoxCell viewCell = (DataGridViewCheckBoxCell)this.dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];
                DataGridViewTextBoxCell fileCell = (DataGridViewTextBoxCell)this.dataGridView1.Rows[e.RowIndex].Cells[1];

                bool isChecked = (bool)viewCell.Value;
                string filePath = (string)fileCell.Tag;

                if( isChecked && !_filesToLoad.Contains( filePath ) ) _filesToLoad.Add( filePath );
                if( !isChecked && _filesToLoad.Contains( filePath ) ) _filesToLoad.Remove( filePath );
            }

            UpdateButtonState();
        }

        /// <summary>
        /// Push checked/unchecked changes immediately for chackbox column, without waiting for focus change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CurrentCellDirtyStateChanged( object sender, EventArgs e )
        {
            if( this.dataGridView1.CurrentCell.ColumnIndex == 0 )
            {
                this.dataGridView1.CommitEdit( DataGridViewDataErrorContexts.Commit );
            }
        }

        private void selectAllToolStripMenuItem_Click( object sender, EventArgs e )
        {
            SetAllViewValues( true );
        }

        private void selectNoneToolStripMenuItem_Click( object sender, EventArgs e )
        {
            SetAllViewValues( false );
        }

        private void versionLabel_DoubleClick( object sender, EventArgs e )
        {
            InstallUpdateSyncWithInfo();
        }
        #endregion

        private void settingsButton_Click( object sender, EventArgs e )
        {
            SettingsForm form = new SettingsForm();

            form.Owner = this;
            this.AddOwnedForm( form );

            form.ShowDialog( this );

            ReloadSettings();
        }

        private void ReloadSettings()
        {
            var settings = Properties.Settings.Default;

            if( settings.MonitorDirectory )
            {
                WatchDirectory( _loadedDirectory );
                RefreshDirectory();
            }
            else
            {
                CloseDirWatcher();
            }
        }

        private void RefreshDirectory()
        {
            AddDirectoryFiles( _loadedDirectory );

            foreach( var file in _listedFiles.ToList() )
            {
                if( !File.Exists( file ) ) RemoveFile( file );
            }

            SortGrid();
        }
    }

    internal class SortGridHelper : System.Collections.IComparer
    {
        #region IComparer<DataGridViewRow> Members

        public int Compare( object x, object y )
        {
            string pathX = (x as DataGridViewRow).Cells[1].Value.ToString();
            string pathY = (y as DataGridViewRow).Cells[1].Value.ToString();

            string directoryX = Path.GetDirectoryName( pathX );
            string directoryY = Path.GetDirectoryName( pathY );

            int c = String.Compare( directoryX, directoryY, true );
            if( c != 0 ) return c;

            string fileNameX = Path.GetFileName( pathX );
            string fileNameY = Path.GetFileName( pathY );

            return String.Compare( fileNameY, fileNameX, true );
        }

        #endregion
    }
}
