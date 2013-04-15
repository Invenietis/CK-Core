#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityLogger\Impl\ActivityLoggerBridge.cs) is part of CiviKey. 
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
* Copyright © 2007-2013, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace CK.Core
{
    /// <summary>
    /// This class (a <see cref="MarshalByRefObject"/>), used with <see cref="ActivityLoggerBridge"/>, enables <see cref="IActivityLogger"/> to be used across Application Domains.
    /// It can also be used to relay logs inside the same application domain.
    /// This ActivityLoggerBridgeTarget must be created in the original <see cref="AppDomain"/> and bound to the final activity logger (the target).
    /// The ActivityLoggerBridge (that is a <see cref="IActivityLoggerClient"/>) can be created in remote AppDomain (and registered 
    /// in the <see cref="IActivityLogger.Output"/> of a logger in the remote AppDomain) bound to the ActivityLoggerBridgeTarget (one can use <see cref="AppDomain.SetData(string,object)"/> to 
    /// transfer the ActivityLoggerBridgeTarget to the other AppDomain for instance).
    /// </summary>
    public class ActivityLoggerBridgeTarget : MarshalByRefObject
    {
        readonly IActivityLogger _logger;
        bool _honorTargetFilter;

        /// <summary>
        /// Initializes a new <see cref="ActivityLoggerBridgeTarget"/> bound to a <see cref="IActivityLogger"/>.
        /// This object should be transfered to another AppDomain and a <see cref="ActivityLoggerBridge"/> 
        /// should be bound to it.
        /// </summary>
        /// <param name="targetLogger">Logger that will receive the logs.</param>
        /// <param name="honorLoggerFilter">
        /// False to ignore the final filter <see cref="IActivityLogger.Filter"/> value: logs from the remote Application Domain
        /// will always be added to the final logger.
        /// </param>
        public ActivityLoggerBridgeTarget( IActivityLogger targetLogger, bool honorLoggerFilter = true )
        {
            if( targetLogger == null ) throw new ArgumentNullException( "targetLogger" );
            _logger = targetLogger;
            _honorTargetFilter = honorLoggerFilter;
        }

        /// <summary>
        /// For empty object pattern.
        /// </summary>
        internal ActivityLoggerBridgeTarget()
        {
            _logger = DefaultActivityLogger.Empty;
        }

        /// <summary>
        /// Gets or sets whether the <see cref="IActivityLogger.Filter"/> of the target logger should be honored or not.
        /// Defaults to true.
        /// </summary>
        public bool HonorLoggerFilter
        {
            get { return _honorTargetFilter; }
            set { _honorTargetFilter = value; }
        }

        /// <summary>
        /// Gest the final logger directly when used in the same AppDomain.
        /// </summary>
        internal IActivityLogger FinalLogger { get { return _logger; } }

        internal int TargetFilter
        {
            get { return _honorTargetFilter ? (int)_logger.Filter : (int)LogLevelFilter.None; }
        }

        #region Cross AppDomain interface.
        internal void UnfilteredLog( string tags, LogLevel level, string text, DateTime logTimeUtc )
        {
            Debug.Assert( (int)_logger.Filter <= (int)level );
            _logger.UnfilteredLog( ActivityLogger.RegisteredTags.FindOrCreate( tags ), level, text, logTimeUtc );
        }

        internal void OpenGroup( string tags, LogLevel level, Exception exception, string groupText, DateTime logTimeUtc )
        {
            Debug.Assert( (int)_logger.Filter <= (int)level );
            _logger.OpenGroup( ActivityLogger.RegisteredTags.FindOrCreate( tags ), level, exception, groupText, logTimeUtc );
        }

        internal void CloseGroup( string[] taggedConclusions )
        {
            Debug.Assert( taggedConclusions == null || (taggedConclusions.Length >= 2 && taggedConclusions.Length % 2 == 0) );
            List<ActivityLogGroupConclusion> c = null;
            if( taggedConclusions != null )
            {
                c = new List<ActivityLogGroupConclusion>();
                int i = 0;
                while( i < taggedConclusions.Length )
                {
                    CKTrait t = ActivityLogger.RegisteredTags.FindOrCreate( taggedConclusions[i++] );
                    c.Add( new ActivityLogGroupConclusion( t, taggedConclusions[i++] ) );
                }
            }
            _logger.CloseGroup( c );
        } 
        #endregion
    }
}
