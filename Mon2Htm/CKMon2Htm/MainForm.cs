using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CK.Core;
using CK.Monitoring;

namespace CK.Mon2Htm
{
    public partial class MainForm : Form
    {
        IActivityMonitor _m;
        List<string> _listedFiles;
        List<string> _filesToLoad;
        string _tempDirPath;

        public MainForm()
        {
            _listedFiles = new List<string>();
            _filesToLoad = new List<string>();
            _m = new ActivityMonitor();
            _m.SetFilter( LogFilter.Debug );
            _m.Output.RegisterClient( new ActivityMonitorConsoleClient() );

            InitializeComponent();

            // "Disable" selection (remove color effect)
            this.dataGridView1.DefaultCellStyle.SelectionBackColor = this.dataGridView1.DefaultCellStyle.BackColor;
            this.dataGridView1.DefaultCellStyle.SelectionForeColor = this.dataGridView1.DefaultCellStyle.ForeColor;

            LoadFromProgramArguments();

            UpdateButtonState();
        }

        private void viewHtmlButton_Click( object sender, EventArgs e )
        {
            string indexFilePath = GenerateHtml();

            RecurseTemporaryAttributes( _tempDirPath );

            if( indexFilePath != null )
            {
                Process.Start( indexFilePath );
            }
        }

        private void LoadFromProgramArguments()
        {
            var args = Environment.GetCommandLineArgs();
            for( int i = 1; i < args.Length; i++ ) // arg 0 is executable itself
            {
                string arg = args[i];
                AddPath( arg );
            }
        }

        private void AddPath( string path )
        {
            if( Directory.Exists( path ) )
            {
                AddDirectory( path, true );
            }
            else if( File.Exists( path ) )
            {
                int firstRow = 0;

                // Special treament: Add files in directory around target, but only select the first one.
                List<string> files = Directory.GetFiles( Path.GetDirectoryName( path ), "*.ckmon", SearchOption.TopDirectoryOnly ).ToList();
                files.Sort( ( a, b ) => String.Compare( Path.GetFileNameWithoutExtension( a ), Path.GetFileNameWithoutExtension( b ), StringComparison.InvariantCultureIgnoreCase ) );

                foreach( var f in files )
                {
                    // Set selected if same.
                    bool isSame = Path.GetFullPath( f ) == Path.GetFullPath( path );
                    int tmp = AddFile( f, isSame );
                    if( isSame ) firstRow = tmp;
                }

                this.dataGridView1.FirstDisplayedScrollingRowIndex = firstRow;
            }
        }

        private int AddFile( string filePath, bool addSelected = false )
        {
            if( _listedFiles.Contains( filePath ) ) return -1;
            _listedFiles.Add( filePath );

            return AddFileRow( filePath, addSelected );
        }

        private int AddFileRow( string filePath, bool viewSelected = false )
        {
            var row = new DataGridViewRow();

            var viewCheckbox = new DataGridViewCheckBoxCell();
            viewCheckbox.Value = viewSelected;

            var fileCell = new DataGridViewTextBoxCell();
            fileCell.Tag = filePath;
            fileCell.Value = Path.GetFileNameWithoutExtension( filePath );

            row.Cells.Add( viewCheckbox );
            row.Cells.Add( fileCell );

            return this.dataGridView1.Rows.Add( row );
        }

        private void AddDirectory( string directoryPath, bool recurse = true )
        {
            var files = Directory.GetFiles( directoryPath, "*.ckmon", recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly );
            foreach( var file in files ) AddFile( file );
        }

        private string GenerateHtml()
        {
            MultiLogReader.ActivityMap activityMap;

            using( MultiLogReader r = new MultiLogReader() )
            {
                r.Add( _filesToLoad );

                activityMap = r.GetActivityMap();
            }

            _tempDirPath = GetTempFolder();

            return HtmlGenerator.CreateFromActivityMap( activityMap, _m, _tempDirPath );
        }

        private static string GetTempFolder()
        {
            string tempFolderName = String.Format( "ckmon-{0}", Guid.NewGuid() );
            string tempFolderPath = Path.Combine( Path.GetTempPath(), tempFolderName );

            DirectoryInfo di = Directory.CreateDirectory( tempFolderPath );

            return tempFolderPath;
        }

        private static void RecurseTemporaryAttributes( string path )
        {
            var files = Directory.GetFiles( path, "*", SearchOption.AllDirectories );

            foreach( var file in files )
            {
                File.SetAttributes( file, FileAttributes.Temporary );
            }
        }

        private void addFileButton_Click( object sender, EventArgs e )
        {
            // Add file
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "Activity Monitor log files (.ckmon)|*.ckmon";
            d.FilterIndex = 0;
            d.ShowReadOnly = true;
            d.CheckFileExists = true;
            d.CheckPathExists = true;
            d.ReadOnlyChecked = true;
            d.InitialDirectory = CK.Mon2Htm.Properties.Settings.Default.LastOpenDirectory;

            var result = d.ShowDialog();

            if( result == DialogResult.OK )
            {
                foreach( var f in d.FileNames )
                {
                    AddFile( f );
                }

                CK.Mon2Htm.Properties.Settings.Default.LastOpenDirectory = Path.GetDirectoryName( d.FileName );
                CK.Mon2Htm.Properties.Settings.Default.Save();

                this.dataGridView1.Sort( this.dataGridView1.Columns[1], ListSortDirection.Ascending );
            }
        }

        private void addDirButton_Click( object sender, EventArgs e )
        {
            // Add directory
            FolderBrowserDialog d = new FolderBrowserDialog();
            d.ShowNewFolderButton = false;
            d.SelectedPath = Properties.Settings.Default.LastOpenDirectory;

            var result = d.ShowDialog();

            if( result == DialogResult.OK )
            {
                AddDirectory( d.SelectedPath );

                Properties.Settings.Default.LastOpenDirectory = d.SelectedPath;
                Properties.Settings.Default.Save();

                this.dataGridView1.Sort( this.dataGridView1.Columns[1], ListSortDirection.Ascending );
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

        private void UpdateButtonState()
        {
            this.viewHtmlButton.Enabled = _filesToLoad.Count > 0;
        }

        /// <summary>
        /// Push checked/unchecked changes immediately for chackbox column, without waiting for focus change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CurrentCellDirtyStateChanged( object sender, EventArgs e )
        {
            if( this.dataGridView1.CurrentCell.ColumnIndex == 0)
            {
                this.dataGridView1.CommitEdit( DataGridViewDataErrorContexts.Commit );
            }
        }
    }
}
