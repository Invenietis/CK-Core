#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityLogger\Impl\DefaultActivityLogger.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Gives access to concrete implementation of <see cref="IDefaultActivityLogger"/> thanks to <see cref="Create"/> factory method.
    /// </summary>
    public class DefaultActivityLogger : ActivityLogger, IDefaultActivityLogger
    {
        /// <summary>
        /// Factory method for <see cref="IDefaultActivityLogger"/> implementation.
        /// </summary>
        /// <returns>A new <see cref="IDefaultActivityLogger"/> implementation.</returns>
        static public IDefaultActivityLogger Create()
        {
            return new DefaultActivityLogger();
        }

        class EmptyDefault : ActivityLoggerEmpty, IDefaultActivityLogger
        {
            public ActivityLoggerTap Tap
            {
                get { return ActivityLoggerTap.Empty; }
            }

            public IDefaultActivityLogger Register( IActivityLoggerSink sink )
            {
                return this;
            }

            public IDefaultActivityLogger Unregister( IActivityLoggerSink sink )
            {
                return this;
            }

            public IReadOnlyList<IActivityLoggerSink> RegisteredSinks
            {
                get { return ReadOnlyListEmpty<IActivityLoggerSink>.Empty; }
            }

            public ActivityLoggerErrorCounter ErrorCounter
            {
                get { return ActivityLoggerErrorCounter.Empty; }
            }

            public ActivityLoggerPathCatcher PathCatcher
            {
                get { return ActivityLoggerPathCatcher.Empty; }
            }

        }

        /// <summary>
        /// Empty <see cref="IDefaultActivityLogger"/> (null object design pattern).
        /// </summary>
        static public readonly IDefaultActivityLogger Empty = new EmptyDefault();

        ActivityLoggerTap _tap;
        ActivityLoggerErrorCounter _errorCounter;
        ActivityLoggerPathCatcher _pathCatcher;

        DefaultActivityLogger()
        {
            _tap = new ActivityLoggerTap();
            _errorCounter = new ActivityLoggerErrorCounter();
            _pathCatcher = new ActivityLoggerPathCatcher();
            
            // Order does not really matter matters here thankd to Closing/Closed pattern, but
            // we order them in the "logical" sense.
            //
            // Registered as a Multiplexed client: will be the last one as beeing called: it is the final sink.
            Output.RegisterMuxClient( _tap );

            // Registered as a normal client: they will not receive
            // external outputs.
            // Will be called AFTER the ErrorCounter.
            Output.RegisterClient( _pathCatcher );
            // Will be called first.
            Output.RegisterClient( _errorCounter );
            
            Output.NonRemoveableClients.AddRangeArray( _tap, _pathCatcher, _errorCounter );
        }

        ActivityLoggerTap IDefaultActivityLogger.Tap 
        { 
            get { return _tap; } 
        }

        ActivityLoggerErrorCounter IDefaultActivityLogger.ErrorCounter
        {
            get { return _errorCounter; }
        }

        ActivityLoggerPathCatcher IDefaultActivityLogger.PathCatcher
        {
            get { return _pathCatcher; }
        }

        IDefaultActivityLogger IDefaultActivityLogger.Register( IActivityLoggerSink sink )
        {
            _tap.Register( sink );
            return this;
        }

        IDefaultActivityLogger IDefaultActivityLogger.Unregister( IActivityLoggerSink sink )
        {
            _tap.Unregister( sink );
            return this;
        }

        IReadOnlyList<IActivityLoggerSink> IDefaultActivityLogger.RegisteredSinks
        {
            get { return _tap.RegisteredSinks; }
        }

    }
}
