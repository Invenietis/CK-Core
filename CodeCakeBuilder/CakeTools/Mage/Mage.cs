using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cake
{
    /// <summary>
    /// Enables to re-sign ClickOnce application.
    /// Implements: http://msdn.microsoft.com/en-us/library/dd465299.aspx (Updating and Re-signing the Application and Deployment Manifests)
    /// </summary>
    public class ClickOnceSigner
    {
        static string _defaultMagePath;
        const string _mageFileName = "mage.exe";

        string _magePath;
        readonly string _pfxFilePath;
        readonly string _password;

        public ClickOnceSigner( string pfxFilePath, string password )
        {
            _pfxFilePath = pfxFilePath;
            _password = password ?? String.Empty;
        }

        public string MagePath
        {
            get { return _magePath; }
            set { _magePath = value; }
        }

        public string EnsureMagePath( IActivityMonitor m )
        {
            return _magePath ?? (_magePath = FindDefaultMagePath( m, true ));
        }

        class PublishedFolder
        {
            readonly ClickOnceSigner _signer;
            readonly string _path;
            readonly List<string> _deployedFiles;
            readonly string _applicationFile;
            readonly string _manifestFile;

            public PublishedFolder( ClickOnceSigner signer, IActivityMonitor m, string path )
            {
                _signer = signer;
                _path = path;
                _deployedFiles = new List<string>();
                foreach( var f in Directory.EnumerateFiles( _path, "*", SearchOption.AllDirectories ) )
                {
                    if( f.EndsWith( ".deploy" ) )
                    {
                        _deployedFiles.Add( f );
                    }
                    else if( f.EndsWith( ".application" ) )
                    {
                        if( _applicationFile != null ) m.Error().Send( "Only one file must end with .application extension." );
                        _applicationFile = f;
                    }
                    else if( f.EndsWith( ".manifest" ) )
                    {
                        if( _manifestFile != null ) m.Error().Send( "Only one file must end with .manifest extension." );
                        _manifestFile = f;
                    }
                    else m.Error().Send( "Unknown file extension for '{0}'.", f );
                }
                if( _applicationFile == null || _manifestFile == null ) m.Error().Send( "Missing .application or .manifest file." );
            }

            public List<string> RemoveDeployExtensionAndGetsExeAndDlls( IActivityMonitor m )
            {
                List<string> exeAndDlls = new List<string>();
                try
                {
                    foreach( var f in _deployedFiles )
                    {
                        string undeployed = f.Remove( ".deploy".Length );
                        File.Move( f, undeployed );
                        if( undeployed.EndsWith( ".exe", StringComparison.OrdinalIgnoreCase ) || undeployed.EndsWith( ".dll", StringComparison.OrdinalIgnoreCase ) )
                        {
                            exeAndDlls.Add( undeployed );
                        }
                    }
                }
                catch( Exception ex )
                {
                    m.Error().Send( ex, "While removing .deploy extension." );
                    return null;
                }
                return exeAndDlls;
            }

            public void UpdateApplicationManifest( IActivityMonitor m )
            {
                using( m.OpenInfo().Send( "Updating Application manifest." ) )
                {
                    string arguments = String.Format( @"-update ""{0}"" -CertFile ""{1}"" -Password ""{2}"" -TimestampUri ""http://timestamp.verisign.com/scripts/timstamp.dll""",
                                                                _manifestFile,
                                                                _signer._pfxFilePath,
                                                                _signer._password );
                    CallMage( m, arguments );
                }
            }

            public void UpdateAndSignDeploymentManifest( IActivityMonitor m )
            {
                using( m.OpenInfo().Send( "Updating and signing deployment manifest." ) )
                {
                    string arguments = String.Format( @"-update ""{0}"" -appmanifest ""{1}"" -CertFile ""{2}"" -Password ""{3}"" -TimestampUri ""http://timestamp.verisign.com/scripts/timstamp.dll""",
                                                                _applicationFile,
                                                                _manifestFile,
                                                                _signer._pfxFilePath,
                                                                _signer._password );
                    CallMage( m, arguments );
                }
            }

            public void RestoreDeployExtension( IActivityMonitor m )
            {
                try
                {
                    foreach( var f in _deployedFiles )
                    {
                        string undeployed = f.Remove( ".deploy".Length );
                        File.Move( undeployed, f );
                    }
                }
                catch( Exception ex )
                {
                    m.Error().Send( ex, "While removing .deploy extension." );
                }
            }

            void CallMage( IActivityMonitor m, string arguments )
            {
                try
                {
                    ProcessStartInfo info = new ProcessStartInfo( _signer._magePath );
                    info.Arguments = arguments;
                    info.CreateNoWindow = true;
                    info.UseShellExecute = false;
                    info.WindowStyle = ProcessWindowStyle.Hidden;
                    using( Process p = Process.Start( info ) )
                    {
                        p.WaitForExit();
                        if( p.ExitCode != 0 )
                        {
                            m.Error().Send( "mage.exe exited with code = {0}.", p.ExitCode );
                        }
                    }
                }
                catch( Exception ex )
                {
                    m.Error().Send( ex );
                }
            }
        }

        public bool ProcessPublishedDirectory( IActivityMonitor m, string publishedDirectory )
        {
            bool hasError = false;
            using( m.CatchCounter( _ => hasError = true ) )
            using( m.OpenInfo().Send( "Signing Application in folder '{0}'." ) )
            {
                EnsureMagePath( m );
                PublishedFolder folder = new PublishedFolder( this, m, publishedDirectory );
                if( hasError ) return false;
                var exeAndDlls = folder.RemoveDeployExtensionAndGetsExeAndDlls( m );
                if( exeAndDlls == null ) return false;
                using( m.OpenTrace().Send( "Strong naming {0} files.", exeAndDlls.Count ) )
                {
                    foreach( var f in exeAndDlls )
                    {
                        if( !_signer.ProcessFile( m, f ) )
                        {
                            Debug.Assert( hasError );
                            return false;
                        }
                    }
                }
                folder.UpdateApplicationManifest( m );
                if( hasError ) return false;
                folder.UpdateAndSignDeploymentManifest( m );
                if( hasError ) return false;
                folder.RestoreDeployExtension( m );
            }
            return !hasError;
        }

        public static string FindDefaultMagePath( IActivityMonitor m, bool logErrorIfNotFound )
        {
            return Tools.ToolPathFinder.FindDefaultPath( m, ref _defaultMagePath, _mageFileName, logErrorIfNotFound );
        }

    }
}
