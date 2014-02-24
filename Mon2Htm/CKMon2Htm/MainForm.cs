using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Deployment.Application;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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
        string _loadedDirectory;
        string _tempDirPath;
        FileSystemWatcher _dirWatcher;

        public MainForm()
        {
            _listedFiles = new List<string>();
            _filesToLoad = new List<string>();
            _hasUpdatedResources = false;

            _m = new ActivityMonitor();
            _m.SetMinimalFilter( LogFilter.Debug );
            _m.Output.RegisterClient( new ActivityMonitorConsoleClient() );

            InitializeComponent();

            // "Disable" selection (remove color effect)
            this.dataGridView1.DefaultCellStyle.SelectionBackColor = this.dataGridView1.DefaultCellStyle.BackColor;
            this.dataGridView1.DefaultCellStyle.SelectionForeColor = this.dataGridView1.DefaultCellStyle.ForeColor;

            LoadFromProgramArguments();

            UpdateButtonState();

            UpdateVersionLabel();
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
                LoadPath( d.FileName );
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
                string arg = args[0];
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

        /// <summary>
        /// Loads file or directory path given as argument.
        /// </summary>
        /// <param name="path">File or directory path</param>
        private void LoadPath( string path )
        {
            if( Directory.Exists( path ) )
            {
                SetLoadedDirectory( path );

                Properties.Settings.Default.LastOpenDirectory = Path.GetDirectoryName( path );
                Properties.Settings.Default.Save();
            }
            else if( File.Exists( path ) )
            {
                SetLoadedDirectory( path );

                SelectFile( path );

                Properties.Settings.Default.LastOpenDirectory = Path.GetDirectoryName( path );
                Properties.Settings.Default.Save();

                var row = GetRowOfFilePath( path );
                this.dataGridView1.FirstDisplayedScrollingRowIndex = this.dataGridView1.Rows.IndexOf( row );
            }
            else
            {
                MessageBox.Show( String.Format( "File {0} could not be found. Select another log file.", path ) );
                bool hasSelectedFile = RequestOpenCkmon();

                if( !hasSelectedFile ) this.Close();
            }
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

            this.dataGridView1.Rows.Remove( GetRowOfFilePath( filePath ) );

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

            WatchDirectory( directoryPath );
            AddDirectoryFiles( directoryPath );

            SortGrid();
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

        private string GetRelativePathOfFile(string filePath)
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
            MultiLogReader.ActivityMap activityMap;

            using( MultiLogReader r = new MultiLogReader() )
            {
                r.Add( _filesToLoad );

                activityMap = r.GetActivityMap();
            }

            _tempDirPath = GetTempFolder();
            string rootFolder = GetRootTempFolder();

            string indexFilePath = Path.Combine( _tempDirPath, "index.html" );

            if( !_hasUpdatedResources )
            {
                HtmlGenerator.CopyResourcesToDirectory( rootFolder );
                _hasUpdatedResources = true;
            }

            if( !File.Exists( indexFilePath ) )
            {
                indexFilePath = HtmlGenerator.CreateFromActivityMap( activityMap, _m, 500, _tempDirPath, "../" );
            }

            return indexFilePath;
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

        private string GetRootTempFolder()
        {
            string tempFolderName = String.Format( "ckmon2htm" );
            string tempFolderPath = Path.Combine( Path.GetTempPath(), tempFolderName );

            DirectoryInfo di = Directory.CreateDirectory( tempFolderPath );

            return tempFolderPath;
        }

        private string GetSelectionHash()
        {
            StringBuilder sb = new StringBuilder();
            foreach( var f in _filesToLoad )
            {
                FileInfo fi = new FileInfo(f);
                sb.Append( fi.Name );
                sb.Append( '_' );
                sb.Append( fi.Length );
                sb.Append( '_' );
            }

            return CalculateMD5Hash( sb.ToString() );
        }

        public string CalculateMD5Hash( string input )
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
        private string GetTempFolder()
        {
            string tempFolderName = GetSelectionHash();
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
            bool wasChecked = _filesToLoad.Contains( e.OldFullPath );
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
            AddFile( e.FullPath );
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

            RecurseTemporaryAttributes( _tempDirPath );

            if( indexFilePath != null )
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
