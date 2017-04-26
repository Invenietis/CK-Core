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
* Copyright © 2007-2015, 
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
        /// Manages source filtering.
        /// This default implementation (<see cref="DefaultFilter(ref string, ref int)"/>) handles file scope only.
        /// </summary>
        public static class SourceFilter
        {
            static readonly ConcurrentDictionary<string,SourceLogFilter> _filters;

            static SourceFilter()
            {
                _filters = new ConcurrentDictionary<string, SourceLogFilter>();
                FilterSource = DefaultFilter;
            }

            /// <summary>
            /// Delegate type that can be assigned to <see cref="FilterSource"/> static property to 
            /// enable filter override based on the caller source location. 
            /// </summary>
            /// <param name="fileName">FileName of the source file (that can be changed, typically by removing a common path prefix).</param>
            /// <param name="lineNumber">The line number in the source file.</param>
            /// <returns>The <see cref="SourceLogFilter"/> to apply. Must default to <see cref="LogFilter.Undefined"/>.</returns>
            public delegate SourceLogFilter FilterSourceDelegate( ref string fileName, ref int lineNumber );

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
            public static void ClearAll()
            {
                _filters.Clear(); 
            }

            /// <summary>
            /// Clears all existing Override filters.
            /// </summary>
            public static void ClearOverrides()
            {
                Update( ( file, filter ) => new SourceLogFilter( LogFilter.Undefined, filter.Minimal ) );
            }

            /// <summary>
            /// Clears all existing Minimal filters.
            /// </summary>
            public static void ClearMinimals()
            {
                Update( ( file, filter ) => new SourceLogFilter( filter.Override, LogFilter.Undefined ) );
            }

            /// <summary>
            /// Updates (or simply scans) all existing filters.
            /// </summary>
            /// <param name="mapper">
            /// Function that takes the file name, the existing filter and maps it to a new filter.
            /// </param>
            /// <remarks>
            /// When the mapper returns <see cref="SourceLogFilter.Undefined"/>, the file configuration is removed.
            /// </remarks>
            public static void Update( Func<string, SourceLogFilter, SourceLogFilter> mapper )
            {
                // Keys take a snapshot.
                // Iterating on the Keys is the preferred method for ConcurrentDictionary.
                foreach( var f in _filters.Keys )
                {
                    SourceLogFilter filter;
                    if( _filters.TryGetValue( f, out filter ) )
                    {
                        SetFilter( mapper( f, filter ), f );
                    }
                }
            }

            /// <summary>
            /// Default filter, challenging file names added by <see cref="SetFilter"/> method.
            /// </summary>
            /// <param name="fileName">The file name.</param>
            /// <param name="lineNumber">The line number.</param>
            /// <returns>Defaults to <see cref="LogFilter.Undefined"/>.</returns>
            public static SourceLogFilter DefaultFilter( ref string fileName, ref int lineNumber )
            {
                SourceLogFilter f;
                _filters.TryGetValue( fileName, out f ); 
                return f;
            }

            /// <summary>
            /// Sets a <see cref="SourceLogFilter"/> for a given file. 
            /// Use <see cref="SourceLogFilter.Undefined"/> to clear any existing configuration for the file.
            /// </summary>
            /// <param name="filter">The filter to set for the file.</param>
            /// <param name="fileName">The file name: do not specify it to inject the path of your source file.</param>
            public static void SetFilter( SourceLogFilter filter, [CallerFilePath]string fileName = null )
            {
                if( filter.IsUndefined ) _filters.TryRemove( fileName, out filter );
                else _filters.AddOrUpdate( fileName, filter, ( s, prev ) => filter ); 
            }

            /// <summary>
            /// Sets an override <see cref="LogFilter"/> for a given file: when not <see cref="LogFilter.Undefined"/> this 
            /// takes precedence over <see cref="IActivityMonitor.ActualFilter"/>.
            /// Use <see cref="LogFilter.Undefined"/> to clear it.
            /// </summary>
            /// <param name="overrideFilter">The override filter to set for the file.</param>
            /// <param name="fileName">The file name: do not specify it to inject the path of your source file.</param>
            public static void SetOverrideFilter( LogFilter overrideFilter, [CallerFilePath]string fileName = null )
            {
                SetFilter( new SourceLogFilter( overrideFilter, LogFilter.Undefined ), fileName );
            }

            /// <summary>
            /// Sets a minimal <see cref="LogFilter"/> for a given file.
            /// Use <see cref="LogFilter.Undefined"/> to clear it.
            /// </summary>
            /// <param name="minimalFilter">The minimal filter to set for the file.</param>
            /// <param name="fileName">The file name: do not specify it to inject the path of your source file.</param>
            public static void SetMinimalFilter( LogFilter minimalFilter, [CallerFilePath]string fileName = null )
            {
                SetFilter( new SourceLogFilter( LogFilter.Undefined, minimalFilter ), fileName );
            }

        }

    }
}
