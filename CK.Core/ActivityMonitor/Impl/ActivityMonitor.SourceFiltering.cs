#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityMonitor\Impl\ActivityMonitor.SourceFiltering.cs) is part of CiviKey. 
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
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core
{
    public partial class ActivityMonitor
    {
        /// <summary>
        /// Manages source filters.
        /// </summary>
        public static class SourceFilter
        {
            static readonly ConcurrentDictionary<string,LogFilter> _filters;

            static SourceFilter()
            {
                _filters = new ConcurrentDictionary<string, LogFilter>();
                FilterSource = DefaultFilter;
            }

            /// <summary>
            /// Delegate type that can be assigned to <see cref="FilterSource"/> static property to 
            /// enable filter override based on the caller source location. 
            /// </summary>
            /// <param name="fileName">FileName of the source file (that can be changed, typically by removing a common path prefix).</param>
            /// <param name="lineNumber">The line number in the source file.</param>
            /// <returns>The LogFilter to apply. Must default to <see cref="LogFilter.Undefined"/>.</returns>
            public delegate LogFilter FilterSourceDelegate( ref string fileName, int lineNumber );

            /// <summary>
            /// Holds a <see cref="FilterSourceDelegate"/> that can override filter configuration and/or alter 
            /// source file name.
            /// It can be changed at any time and application is immediate. 
            /// It is set by default to <see cref="DefaultFilter"/>.
            /// </summary>
            public static FilterSourceDelegate FilterSource;

            /// <summary>
            /// Clears all existing filters.
            /// </summary>
            public static void Clear()
            {
                _filters.Clear(); 
            }

            /// <summary>
            /// Default filter, challenging file names added by <see cref="SetFileFilter"/> method.
            /// </summary>
            /// <param name="fileName">The file name.</param>
            /// <param name="lineNumber">The line number.</param>
            /// <returns>Defaults to <see cref="LogFilter.Undefined"/>.</returns>
            public static LogFilter DefaultFilter( ref string fileName, int lineNumber )
            {
                LogFilter f;
                _filters.TryGetValue( fileName, out f ); 
                return f;
            }

            /// <summary>
            /// Sets a filter for a given file. 
            /// Use <see cref="LogFilter.Undefined"/> to clear any existing configuration for the file.
            /// </summary>
            /// <param name="filter">The filter to set for the file.</param>
            /// <param name="fileName">The file name: do not specify it to inject the path of your source file.</param>
            public static void SetFileFilter( LogFilter filter, [CallerFilePath]string fileName = null )
            {
                if( filter == LogFilter.Undefined ) _filters.TryRemove( fileName, out filter );
                else _filters.AddOrUpdate( fileName, filter, ( s, prev ) => filter ); 
            }

            internal static int SourceFilterLine( ref string fileName, int lineNumber )
            {
                var h = FilterSource;
                return h == null ? 0 : (int)h( ref fileName, lineNumber ).Line;
            }

            internal static int SourceFilterGroup( ref string fileName, int lineNumber )
            {
                var h = FilterSource;
                return h == null ? 0 : (int)h( ref fileName, lineNumber ).Group;
            }

        }

    }
}
