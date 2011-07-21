#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Host\Impl\StandardContextHost.cs) is part of CiviKey. 
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
* Copyright © 2007-2011, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.IO;
using System.Linq;
using CK.Plugin.Config;
using CK.Storage;
using System.Diagnostics;
using CK.Core;
using System.Reflection;

namespace CK.Context
{
    public class ContextPathRequiredEventArgs : EventArgs
    {
        public string ContextPath { get; set; }
    }

    public class StandardContextHost : ContextHost, IHostInformation
    {
        Version _appVersion;
        string _commonAppData;
        string _appDataPath;

        public event EventHandler<ContextPathRequiredEventArgs> ContextPathRequired;

        /// <summary>
        /// Initializes a new <see cref="StandardContextHost"/> with an application name and an optional subordinated name. 
        /// These are used to build the <see cref="ApplicationDataPath"/> and <see cref="CommonApplicationDataPath"/>.
        /// This constructor does no more than validating its parameters, and since the base <see cref="ContextHost"/> constructor 
        /// does nothing, it is totally safe and secure as long as <paramref name="appName"/> and <see cref="subAppName"/> are valid.
        /// </summary>
        /// <param name="appName">
        /// Name of the application (Civikey-Standard for instance for the Civikey Standard application). 
        /// Must be an indentifier (no /, \ or other special characters in it: see <see cref="Path.GetInvalidPathChars"/>).
        /// </param>
        /// <param name="subAppName">Optional second name (can be null). When not null, it must be an identifier just like <paramref name="appName"/>.</param>
        public StandardContextHost( string appName, string subAppName )
        {
            char[] illegal = Path.GetInvalidPathChars();
            if( String.IsNullOrEmpty( appName ) ) throw new ArgumentNullException( "appName" );
            if( appName.IndexOf( '/' ) >= 0 || appName.IndexOf( '\\' ) >= 0 ) throw new ArgumentException( "appName" );
            if( subAppName != null )
            {
                subAppName = subAppName.Trim();
                if( subAppName.Length == 0 )
                    subAppName = null;
                else
                {
                    if( subAppName.IndexOf( '/' ) >= 0 || subAppName.IndexOf( '\\' ) >= 0 ) throw new ArgumentException( "subAppName" );
                    if( subAppName.Any( c => illegal.Contains( c ) ) ) throw new ArgumentException( "subAppName" );
                }
            }
            if( appName.Any( c => illegal.Contains( c ) ) ) throw new ArgumentException( "appName" );

            AppName = appName;
            SubAppName = subAppName;
        }

        /// <summary>
        /// Gets the name of the application. Civikey-Standard for instance for the Civikey Standard application. 
        /// It is an indentifier (no /, \ or other special characters in it: see <see cref="Path.GetInvalidPathChars"/>).
        /// </summary>
        public string AppName { get; private set; }

        /// <summary>
        /// Gets an optional second name (can be null).
        /// When not null, it is an identifier just like <see cref="AppName"/>.
        /// </summary>
        public string SubAppName { get; private set; }

        /// <summary>
        /// Gets the current version of application.
        /// By default, it is stored in the system configuration file.
        /// </summary>
        public virtual Version AppVersion
        {
            get { return _appVersion ?? (_appVersion = new Version( (string)SystemConfig.GetOrSet( "Version", "1.0" ) )); }
        }

        #region File paths

        string GetFilePath( Environment.SpecialFolder specialFolder )
        {
            string p = Environment.GetFolderPath( specialFolder )
                    + Path.DirectorySeparatorChar
                    + AppName
                    + Path.DirectorySeparatorChar;
            if( SubAppName != null )
                p += SubAppName + Path.DirectorySeparatorChar;
            if( !Directory.Exists( p ) ) Directory.CreateDirectory( p );
            return p;
        }

        /// <summary>
        /// Gets the full path of application-specific data repository, for the current user.
        /// Ends with <see cref="Path.DirectorySeparatorChar"/>.
        /// The directory is created if it does not exist.
        /// </summary>
        public string ApplicationDataPath
        {
            get { return _appDataPath ?? (_appDataPath = GetFilePath( Environment.SpecialFolder.ApplicationData )); }
        }

        /// <summary>
        /// Gets the full path of application-specific data repository, for all users.
        /// Ends with <see cref="Path.DirectorySeparatorChar"/>.
        /// The directory is created if it does not exist.
        /// </summary>
        public string CommonApplicationDataPath
        {
            get { return _commonAppData ?? (_commonAppData = GetFilePath( Environment.SpecialFolder.CommonApplicationData )); }
        }

        /// <summary>
        /// Gets the full path of the machine configuration file.
        /// Defaults to "System.config.ck" file in <see cref="CommonApplicationDataPath"/>.
        /// </summary>
        public virtual string DefaultSystemConfigPath
        {
            get { return CommonApplicationDataPath + "System.config.ck"; }
        }

        /// <summary>
        /// Gets or sets the full path of the user configuration file.
        /// Defaults to "User.config.ck" file in <see cref="ApplicationDataPath"/>.
        /// </summary>
        public string DefaultUserConfigPath
        {
            get { return ApplicationDataPath + "User.config.ck"; }
        }

        public string ContextPath
        {
            get { return (string)UserConfig["LastContextPath"]; }
            set { UserConfig["LastContextPath"] = value; }
        }

        private string EnsureContextPath()
        {
            if( ContextPath == null )
            {
                var e = new ContextPathRequiredEventArgs() { ContextPath = Path.Combine( _appDataPath, "Context.xml" ) };
                var h = ContextPathRequired;
                if( h != null )
                {
                    h( this, e );
                    if( !String.IsNullOrEmpty( e.ContextPath ) ) ContextPath = e.ContextPath;
                }
                if( String.IsNullOrEmpty( e.ContextPath ) ) throw new CKException( R.ContextPathEmpty );
                ContextPath = e.ContextPath;
            }
            return ContextPath;
        }

        #endregion

        public override IContext CreateContext()
        {
            IContext c = base.CreateContext();
            c.ServiceContainer.Add<IHostInformation>( this );
            return c;
        }

        IUserProfile EnsureCurrentUserProfile()
        {
            IUserProfile currentProfile = Context.ConfigManager.SystemConfiguration.UserProfiles.LastProfile;
            if( currentProfile == null || currentProfile.Type != ConfigSupportType.File )
            {
                currentProfile = Context.ConfigManager.SystemConfiguration.UserProfiles.AddOrSet( Environment.UserName, DefaultUserConfigPath, ConfigSupportType.File, true );
            }
            return currentProfile;
        }

        private void WriteContext( IContext context, string filePath )
        {
            String tmpPath = filePath + ".new";

            using( Stream str = new FileStream( tmpPath, FileMode.Create ) )
            {
                using( IStructuredWriter sr = SimpleStructuredWriter.CreateWriter( str, context ) )
                {
                    context.SaveContext( sr );
                }
            }

            if( File.Exists( filePath ) )
                File.Replace( tmpPath, filePath, filePath + ".bak", true );
            else
                File.Move( tmpPath, filePath );
        }

        protected override bool LoadSystemConfig()
        {
            if( File.Exists( DefaultSystemConfigPath ) )
            {
                using( Stream str = new FileStream( DefaultSystemConfigPath, FileMode.Open ) )
                {
                    using( IStructuredReader sr = SimpleStructuredReader.CreateReader( str, Context ) )
                    {
                        Context.ConfigManager.Extended.LoadSystemConfig( sr );
                    }
                    Debug.Assert( SystemConfig != null );
                    return true;
                }
            }
            return false;
        }

        protected override bool LoadUserConfig()
        {
            return LoadUserConfigFromFile( EnsureCurrentUserProfile() );
        }

        public virtual bool LoadUserConfigFromFile( string path, bool createNewProfile )
        {
            IUserProfile profile = null;
            if( createNewProfile )
                profile = Context.ConfigManager.SystemConfiguration.UserProfiles.AddOrSet( Path.GetFileName( path ), path, ConfigSupportType.File, false );
            return LoadUserConfigFromFile( path, profile );
        }

        public virtual bool LoadUserConfigFromFile( IUserProfile profile )
        {
            if( profile == null ) throw new ArgumentNullException( "profile" );
            if( profile.Type != ConfigSupportType.File ) throw new CKException( R.ConfigSupportTypeFileOnly );
            
            return LoadUserConfigFromFile( profile.Address, profile );
        }

        private bool LoadUserConfigFromFile( string path, IUserProfile setLastProfile )
        {
            if( File.Exists( path ) )
            {
                using( Stream str = new FileStream( path, FileMode.Open ) )
                {
                    using( IStructuredReader sr = SimpleStructuredReader.CreateReader( str, Context ) )
                    {
                        Context.ConfigManager.Extended.LoadUserConfig( sr, setLastProfile );
                    }
                    return true;
                }
            }
            return false;
        }

        public override void SaveSystemConfig()
        {
            using( Stream wrt = new FileStream( DefaultSystemConfigPath, FileMode.Create ) )
            {
                using( IStructuredWriter sw = SimpleStructuredWriter.CreateWriter( wrt, Context ) )
                {
                    Context.ConfigManager.Extended.SaveSystemConfig( sw );
                }
            }
        }

        public override void SaveUserConfig()
        {
            Debug.Assert( Context.ConfigManager.SystemConfiguration.UserProfiles.LastProfile != null );
            using( Stream wrt = new FileStream( Context.ConfigManager.SystemConfiguration.UserProfiles.LastProfile.Address, FileMode.Create ) )
            {
                using( IStructuredWriter sw = SimpleStructuredWriter.CreateWriter( wrt, Context ) )
                {
                    UserConfig["LastContextPath"] = ContextPath;
                    Context.ConfigManager.Extended.SaveUserConfig( sw );
                }
            }
        }

        public override void SaveContext()
        {
            WriteContext( Context, EnsureContextPath() );
        }

        public virtual bool LoadContext( string filePath )
        {
            if( filePath != null && File.Exists( filePath ) ) 
            {
                using( Stream str = new FileStream( filePath, FileMode.Open ) )
                {
                    using( IStructuredReader rdr = SimpleStructuredReader.CreateReader( str, Context ) )
                    {
                        if( Context.LoadContext( rdr ) )
                        {
                            ContextPath = filePath;
                            return true;
                        }
                    }
                }
            }            
            return false;
        }

        public virtual bool LoadContext( Assembly assembly, string resourcePath )
        {
            if( assembly != null && resourcePath != null && resourcePath != String.Empty )
            {
                using( Stream str = assembly.GetManifestResourceStream( resourcePath ) )
                {
                    using( IStructuredReader rdr = SimpleStructuredReader.CreateReader( str, Context ) )
                    {
                        if( Context.LoadContext( rdr ) )
                        {
                            ContextPath = null;                            
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public virtual bool RestoreLastContext()
        {
            return LoadContext( ContextPath );
        }
    }
}
