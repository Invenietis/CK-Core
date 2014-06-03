#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityMonitor\Impl\ActivityMonitorExtension.DependentActivity.cs) is part of CiviKey. 
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
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace CK.Core
{
    /// <summary>
    /// Provides extension methods for <see cref="IActivityMonitor"/> and other types from the Activity monitor framework.
    /// </summary>
    public static partial class ActivityMonitorExtension
    {

        /// <summary>
        /// Offers dependent token creation, launching and start.
        /// </summary>
        public struct DependentSender
        {
            readonly IActivityMonitor _monitor;
            readonly string _fileName;
            readonly int _lineNumber;

            internal DependentSender( IActivityMonitor m, string f, int n )
            {
                _monitor = m;
                _fileName = f;
                _lineNumber = n;
            }

            /// <summary>
            /// Creates a token for a dependent activity that will use the current monitor's topic.
            /// </summary>
            public ActivityMonitor.DependentToken CreateToken()
            {
                string msg;
                var t = ActivityMonitor.DependentToken.CreateWithMonitorTopic( _monitor, false, out msg );
                _monitor.UnfilteredLog( ActivityMonitor.Tags.CreateDependentActivity, LogLevel.Info, msg, t.CreationDate, null, _fileName, _lineNumber );
                return t;
            }

            /// <summary>
            /// Creates a token for a dependent activity that will be bound to a specified topic (or that will not change the dependent monitor's topic
            /// if null is specified).
            /// </summary>
            /// <param name="dependentTopic">Topic for the dependent activity. Use null to not change the dependent monitor's topic.</param>
            /// <returns>A dependent token.</returns>
            public ActivityMonitor.DependentToken CreateTokenWithTopic( string dependentTopic )
            {
                string msg;
                var t = ActivityMonitor.DependentToken.CreateWithDependentTopic( _monitor, false, dependentTopic, out msg );
                _monitor.UnfilteredLog( ActivityMonitor.Tags.CreateDependentActivity, LogLevel.Info, msg, t.CreationDate, null, _fileName, _lineNumber );
                return t;
            }

            /// <summary>
            /// Launches one or more dependent activities (thanks to a delegate) that will use the current monitor's topic.
            /// </summary>
            /// <param name="dependentLauncher">Must create and launch dependent activities that should use the created token.</param>
            /// <returns>A dependent token.</returns>
            public void Launch( Action<ActivityMonitor.DependentToken> dependentLauncher )
            {
                if( dependentLauncher == null ) throw new ArgumentNullException( "dependentLauncher" );
                string msg;
                var t = ActivityMonitor.DependentToken.CreateWithMonitorTopic( _monitor, true, out msg );
                using( _monitor.UnfilteredOpenGroup( ActivityMonitor.Tags.CreateDependentActivity, LogLevel.Info, null, msg, t.CreationDate, null, _fileName, _lineNumber ) )
                {
                    dependentLauncher( t );
                    _monitor.CloseGroup( _monitor.NextLogTime(), "Success." );
                }
            }

            /// <summary>
            /// Launches one or more dependent activities (thanks to a delegate) that will be bound to a specified topic (or that will not change 
            /// the dependent monitor's topic if null is specified).
            /// </summary>
            /// <param name="dependentLauncher">Must create and launch dependent activities that should use the created token.</param>
            /// <param name="dependentTopic">Topic for the dependent activity. When null, the dependent monitor's topic is not changed.</param>
            public void LaunchWithTopic( Action<ActivityMonitor.DependentToken> dependentLauncher, string dependentTopic )
            {
                if( dependentLauncher == null ) throw new ArgumentNullException( "dependentLauncher" );
                string msg;
                var t = ActivityMonitor.DependentToken.CreateWithDependentTopic( _monitor, true, dependentTopic, out msg );
                using( _monitor.UnfilteredOpenGroup( ActivityMonitor.Tags.CreateDependentActivity, LogLevel.Info, null, msg, t.CreationDate, null, _fileName, _lineNumber ) )
                {
                    dependentLauncher( t );
                    _monitor.CloseGroup( _monitor.NextLogTime(), "Success." );
                }
            }


            /// <summary>
            /// Starts a dependent activity. This sets the <see cref="ActivityMonitor.DependentToken.Topic"/> if it is not null and opens a group
            /// tagged with <see cref="ActivityMonitor.Tags.StartDependentActivity"/> with a message that can be parsed back thanks to <see cref="ActivityMonitor.DependentToken.TryParseStartMessage"/>.
            /// </summary>
            /// <param name="token">Token that describes the origin of the activity.</param>
            /// <returns>A disposable object. It must be disposed at the end of the activity.</returns>
            public IDisposable StartDependentActivity( ActivityMonitor.DependentToken token )
            {
                if( token == null ) throw new ArgumentNullException( "token" );
                return ActivityMonitor.DependentToken.Start( token, _monitor, _fileName, _lineNumber );
            }
        }

        /// <summary>
        /// Enables dependent activities token creation, activities launch and start declaration.
        /// </summary>
        /// <param name="this">This <see cref="IActivityMonitor"/>.</param>
        /// <param name="fileName">Source file name of the emitter (automatically injected by C# compiler but can be explicitly set).</param>
        /// <param name="lineNumber">Line number in the source file (automatically injected by C# compiler but can be explicitly set).</param>
        /// <returns>Sender object.</returns>
        static public DependentSender DependentActivity( this IActivityMonitor @this, [CallerFilePath]string fileName = null, [CallerLineNumber]int lineNumber = 0 )
        {
            return new DependentSender( @this, fileName, lineNumber );
        }
    }
}
