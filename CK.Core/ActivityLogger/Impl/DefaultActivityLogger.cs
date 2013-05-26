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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Simple implementation of <see cref="IDefaultActivityLogger"/> with a <see cref="Tap"/> to register <see cref="IActivityLoggerSink"/>, 
    /// an <see cref="ErrorCounter"/> and a <see cref="PathCatcher"/>.
    /// </summary>
    public class DefaultActivityLogger : ActivityLogger, IDefaultActivityLogger
    {
        [ExcludeFromCodeCoverage]
        class EmptyDefault : ActivityLoggerEmpty, IDefaultActivityLogger
        {
            public ActivityLoggerTap Tap
            {
                get { return ActivityLoggerTap.Empty; }
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

        readonly ActivityLoggerTap _tap;
        readonly ActivityLoggerErrorCounter _errorCounter;
        readonly ActivityLoggerPathCatcher _pathCatcher;

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultActivityLogger"/>, that will optionally generate error counter group conclusions ("2 erros, 1 warning").
        /// </summary>
        /// <param name="generateErrorCounterConlusion">False to not generate error counter group conclusions.</param>
        public DefaultActivityLogger( bool generateErrorCounterConlusion = true )
        {
            // Order does not really matter matters here thanks to Closing/Closed pattern, but
            // we order them in the "logical" sense.

            // Will be the last one as beeing called: it is the final sink.
            _tap = new ActivityLoggerTap( this );
            // Will be called AFTER the ErrorCounter.
            _pathCatcher = new ActivityLoggerPathCatcher( this );
            // Will be called first.
            _errorCounter = new ActivityLoggerErrorCounter( this, generateErrorCounterConlusion );
        }

        /// <summary>
        /// Gets the <see cref="ActivityLoggerTap"/> that manages <see cref="IActivityLoggerSink"/>
        /// for this <see cref="DefaultActivityLogger"/>.
        /// </summary>
        public ActivityLoggerTap Tap 
        { 
            get { return _tap; } 
        }

        /// <summary>
        /// Gets the <see cref="ActivityLoggerErrorCounter"/> that manages fatal errors, errors and warnings
        /// and automatic conclusion of groups with such information.
        /// </summary>
        public ActivityLoggerErrorCounter ErrorCounter
        {
            get { return _errorCounter; }
        }

        /// <summary>
        /// Gets the <see cref="ActivityLoggerPathCatcher"/> that exposes and maintains <see cref="ActivityLoggerPathCatcher.DynamicPath">DynamicPath</see>,
        /// <see cref="ActivityLoggerPathCatcher.LastErrorPath">LastErrorPath</see> and <see cref="ActivityLoggerPathCatcher.LastWarnOrErrorPath">LastWarnOrErrorPath</see>.
        /// </summary>
        public ActivityLoggerPathCatcher PathCatcher
        {
            get { return _pathCatcher; }
        }

    }
}
