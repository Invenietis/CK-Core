#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core.PowershellExtensions\IPowershellActivityMonitor.cs) is part of CiviKey. 
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
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core.PowershellExtensions
{
    /// <summary>
    /// Public interface for a powershell useable activity monitor. 
    /// It exposes an internal storage of logs and methods without having to import extension methods (that is an issue in powershell)
    /// </summary>
    public interface IPowershellActivityMonitor : IActivityMonitor
    {
        /// <summary>
        /// Empty the internal storage of logs.
        /// </summary>
        void Clear();

        /// <summary>
        /// Read all log lines in the internal storage.
        /// </summary>
        /// <returns>All logs available in the internal storage</returns>
        IEnumerable<string> ReadAllLines();

        /// <summary>
        /// Writes an error log.
        /// </summary>
        /// <param name="log">The log to store</param>
        void WriteError( string log );

        /// <summary>
        /// Writes a fatal log.
        /// </summary>
        /// <param name="log">The log to store</param>
        void WriteFatal( string log );

        /// <summary>
        /// Writes an info log.
        /// </summary>
        /// <param name="log">The log to store</param>
        void WriteInfo( string log );

        /// <summary>
        /// Writes a trace log.
        /// </summary>
        /// <param name="log">The log to store</param>
        void WriteTrace( string log );

        /// <summary>
        /// Writes a warn log.
        /// </summary>
        /// <param name="log">The log to store</param>
        void WriteWarn( string log );
    }
}