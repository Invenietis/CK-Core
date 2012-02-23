using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CK.Plugin.Config;

namespace CK.Context
{
    
    /// <summary>
    /// Exposes standard host information.
    /// </summary>
    public interface IHostInformation
    {
        /// <summary>
        /// Gets the host configuration associated to the current user.
        /// </summary>
        IObjectPluginConfig UserConfig { get; }

        /// <summary>
        /// Gets the host configuration associated to the system.
        /// <remarks>
        IObjectPluginConfig SystemConfig { get; }

        /// <summary>
        /// Gets the System configuration's file path.
        /// </summary>
        /// <returns></returns>
        Uri GetSystemConfigAddress();

        /// <summary>
        /// Gets the name of the application. Civikey-Standard for instance for the Civikey Standard application. 
        /// It is an identifier (no /, \ or other special characters in it: see <see cref="Path.GetInvalidPathChars"/>).
        /// </summary>
        string AppName { get; }

        /// <summary>
        /// Gets an optional second name (can be null).
        /// When not null, it is an identifier just like <see cref="AppName"/>.
        /// </summary>
        string SubAppName { get; }

        /// <summary>
        /// Gets the current application version.
        /// </summary>
        Version AppVersion { get; }

        /// <summary>
        /// Gets the full path of application-specific data repository for the current user if 
        /// the host handles it. Null otherwise.
        /// When not null, it ends with <see cref="Path.DirectorySeparatorChar"/> and the directory exists.
        /// </summary>
        string ApplicationDataPath { get; }

        /// <summary>
        /// Gets the full path of application-specific data repository for all users if 
        /// the host handles it. Null otherwise.
        /// When not null, it ends with <see cref="Path.DirectorySeparatorChar"/> and the directory exists.
        /// </summary>
        string CommonApplicationDataPath { get; }


    }
}
