#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityLogger\Impl\ActivityLogger.cs) is part of CiviKey. 
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
using System.Diagnostics;

namespace CK.Core
{
    /// <summary>
    /// Concrete implementation of <see cref="IActivityLogger"/>.
    /// </summary>
    public partial class ActivityLogger
    {

        /// <summary>
        /// Groups are bound to an <see cref="ActivityLogger"/> and are linked together from 
        /// the current one to the very first one (stack).
        /// </summary>
        public class Group : IActivityLogGroup, IDisposable
        {
            readonly ActivityLogger _logger;
            readonly int _index;
            string _text;
            CKTrait _tags;
            Exception _exception;
            Func<string> _getConclusion;

            /// <summary>
            /// Initializes a new <see cref="Group"/> object.
            /// </summary>
            /// <param name="logger">The logger.</param>
            internal protected Group( ActivityLogger logger, int index )
            {
                _logger = logger;
                _index = index;
            }

            /// <summary>
            /// Initializes or reinitializes this group. 
            /// </summary>
            /// <param name="level">The <see cref="GroupLevel"/>.</param>
            /// <param name="text">The <see cref="GroupText"/>.</param>
            /// <param name="defaultConclusionText">
            /// Optional delegate to call on close to obtain a conclusion text if no 
            /// explicit conclusion is provided through <see cref="IActivityLogger.CloseGroup"/>.
            /// </param>
            /// <param name="ex">Optional exception associated to the group.</param>
            internal protected virtual void Initialize( CKTrait tags, LogLevel level, string text, Func<string> defaultConclusionText, Exception ex )
            {
                Filter = _logger.Filter;
                // Logs everything when a Group is an error: we then have full details without
                // logging all with Error or Fatal.
                if( level >= LogLevel.Error ) _logger.Filter = LogLevelFilter.Trace;
                GroupLevel = level;
                _text = text ?? String.Empty;
                _tags = tags ?? ActivityLogger.EmptyTag;
                _getConclusion = defaultConclusionText;
                _exception = ex;
            }

            /// <summary>
            /// Gets the origin <see cref="IActivityLogger"/> for the log group.
            /// </summary>
            public IActivityLogger OriginLogger { get { return _logger; } }

            /// <summary>
            /// Gets the tags for the log group.
            /// </summary>
            public CKTrait Tags { get { return _tags; } }

            /// <summary>
            /// Get the previous group in its <see cref="OriginLogger"/>. Null if this is a top level group.
            /// </summary>
            public IActivityLogGroup Parent { get { return _index > 0 ? _logger._groups[_index - 1] : null; } }
            
            /// <summary>
            /// Gets or sets the <see cref="LogLevelFilter"/> for this group.
            /// Initialized with the <see cref="IActivityLogger.Filter"/> when the group has been opened.
            /// </summary>
            public LogLevelFilter Filter { get; protected set; }

            /// <summary>
            /// Gets the depth of this group in its <see cref="OriginLogger"/> (1 for top level groups).
            /// </summary>
            public int Depth { get { return _index+1; } }

            /// <summary>
            /// Gets the level of this group.
            /// </summary>
            public LogLevel GroupLevel { get; private set; }
            
            /// <summary>
            /// Gets the text with which this group has been opened. Null if and only if the group is closed.
            /// </summary>
            public string GroupText { get { return _text; } }

            /// <summary>
            /// Gets the associated <see cref="Exception"/> if it exists.
            /// </summary>
            public Exception Exception { get { return _exception; } }

            /// <summary>
            /// Gets whether the <see cref="GroupText"/> is actually the <see cref="Exception"/> message.
            /// </summary>
            public bool IsGroupTextTheExceptionMessage 
            {
                get { return _exception != null && ReferenceEquals( _exception.Message, GroupText ); } 
            }

            /// <summary>
            /// Gets or sets an optional function that will be called on group closing. 
            /// </summary>
            protected Func<string> GetConclusionText 
            { 
                get { return _getConclusion; } 
                set { _getConclusion = value; } 
            }
      
            /// <summary>
            /// Ensures that any groups opened after this one are closed before closing this one.
            /// </summary>
            void IDisposable.Dispose()
            {
                if( _text != null )
                {
                    while( _logger._current != this ) ((IDisposable)_logger._current).Dispose();
                    _logger.CloseGroup( null );
                }
            }           

            internal void GroupClose( List<ActivityLogGroupConclusion> conclusions )
            {
                string auto = ConsumeConclusionText();
                if( auto != null ) conclusions.Add( new ActivityLogGroupConclusion( TagGetTextConclusion, auto ) );
                _text = null;
                _exception = null;
                Debug.Assert( _getConclusion == null, "Has been consumed." );
            }

            /// <summary>
            /// Calls <see cref="GetConclusionText"/> and sets it to null.
            /// </summary>
            string ConsumeConclusionText()
            {
                string autoText = null;
                if( _getConclusion != null )
                {
                    try
                    {
                        autoText = _getConclusion();
                    }
                    catch( Exception ex )
                    {
                        autoText = "Unexpected Error while getting conclusion text: " + ex.Message;
                    }
                    _getConclusion = null;
                }
                return autoText;
            }
        }

        /// <summary>
        /// Factory method for <see cref="Group"/> (or any specialized class).
        /// This is may be overriden in advanced scenario where groups may offer different 
        /// behaviors than the default ones.
        /// </summary>
        /// <param name="index">The index (zero based depth) of the group.</param>
        /// <returns>A new group.</returns>
        protected virtual Group CreateGroup( int index )
        {
            return new Group( this, index );
        }

    }
}
