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
using System.Collections.Generic;

namespace CK.Context
{

    public class StandardContextHost : AbstractContextHost, IHostInformation
    {
        Version _appVersion;
        string _commonAppData;
        string _appDataPath;

        /// <summary>
        /// Fired whenever a an address (and a display name) is required for the context.
        /// </summary>
        public event EventHandler<ContextProfileRequiredEventArgs> ContextAddressRequired;

        /// <summary>
        /// Initializes a new <see cref="StandardContextHost"/> with an application name and an optional subordinated name. 
        /// These are used to build the <see cref="ApplicationDataPath"/> and <see cref="CommonApplicationDataPath"/>.
        /// This constructor does no more than validating its parameters, and since the base <see cref="AbstractContextHost"/> constructor 
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

        protected virtual string GetFilePath( bool commonPath )
        {
            Environment.SpecialFolder f = commonPath ? Environment.SpecialFolder.CommonApplicationData : Environment.SpecialFolder.ApplicationData;
            string p = Environment.GetFolderPath( f )
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
            get { return _appDataPath ?? (_appDataPath = GetFilePath( false )); }
        }

        /// <summary>
        /// Gets the full path of application-specific data repository, for all users.
        /// Ends with <see cref="Path.DirectorySeparatorChar"/>.
        /// The directory is created if it does not exist.
        /// </summary>
        public string CommonApplicationDataPath
        {
            get { return _commonAppData ?? (_commonAppData = GetFilePath( true )); }
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
        public virtual string DefaultUserConfigPath
        {
            get { return ApplicationDataPath + "User.config.ck"; }
        }

        protected override Uri GetSystemConfigAddress()
        {
            return new Uri( DefaultSystemConfigPath );
        }

        protected override Uri GetDefaultUserConfigAddress( bool saving )
        {
            return new Uri( DefaultUserConfigPath );
        }

        protected override KeyValuePair<string, Uri> GetDefaultContextProfile( bool saving )
        {
            var e = new ContextProfileRequiredEventArgs( Context, saving )
            {
                Address = new Uri( Path.Combine( _appDataPath, "Context.xml" ) ),
                DisplayName = String.Format( Res.R.NewContextDisplayName, DateTime.Now )
            };
            var h = ContextAddressRequired;
            if( h != null )
            {
                h( this, e );
                if( e.Address == null ) throw new CKException( Res.R.ContextAddressRequired );
            }
            return new KeyValuePair<string, Uri>( e.DisplayName, e.Address );
        }

        #endregion

        public override IContext CreateContext()
        {
            IContext c = base.CreateContext();
            c.ServiceContainer.Add<IHostInformation>( this );
            return c;
        }

    }
}
