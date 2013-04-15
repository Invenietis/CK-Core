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
using System.Collections.Concurrent;
using System.Threading;

namespace CK.Core
{
    /// <summary>
    /// Implementation of error handling while logging.
    /// </summary>
    public partial class ActivityLogger
    {
        /// <summary>
        /// Encapsulates all exceptions raised by <see cref="IActivityLoggerClient"/> or <see cref="IActivityLoggerSink"/>
        /// or raised by <see cref="LogErrorCollector.OnError"/> event itself. See <see cref="LogErrorCollector"/>.
        /// </summary>
        public struct LoggedError
        {
            internal LoggedError( string c, Exception e )
            {
                Comment = c;
                Exception = e;
            }

            /// <summary>
            /// The origin or a description of the <see cref="P:Exception"/>.
            /// Never null but can be empty if no comment is provided while calling <see cref="LogErrorCollector.Add"/>.
            /// </summary>
            public readonly string Comment;
            
            /// <summary>
            /// The exception.
            /// </summary>
            public readonly Exception Exception;

        }

        /// <summary>
        /// The error collector. See <see cref="LogErrorCollector"/>.
        /// </summary>
        public static readonly LogErrorCollector LoggingError;

        /// <summary>
        /// This collector is thread-safe and keeps up to 50 <see cref="LoggedError"/> (and no more).
        /// It raises <see cref="OnError"/> event on each <see cref="Add"/> (no Add can be lost but one event may 
        /// fire for more than one error).
        /// </summary>
        public class LogErrorCollector
        {
            readonly FIFOBuffer<LoggedError> _exceptions;
            readonly object _raiseLock;
            int _raiseFlag;
            
            /// <summary>
            /// Fires when an error has been <see cref="Add"/>ed (there cannot be more than one thread that raises this event at the same time).
            /// Raising this event is itself protected: if an exception is raised by one of the registered EventHandler, the culprit is removed 
            /// from the OnError list of delegates, the exception is appended in the collector, and a new event will be raised (to the remaining handlers).
            /// Caution: the event always fire on a background thread (adding an error is not a blocking operation).
            /// </summary>
            public event EventHandler OnErrorFromBackgroundThreads;

            internal LogErrorCollector()
            {
                _exceptions = new FIFOBuffer<LoggedError>( 50 );
                _raiseLock = new object();
            }

            /// <summary>
            /// Adds an error that occured during log dispatching. <see cref="ActivityLogger"/> and <see cref="ActivityLoggerTap"/> forward
            /// any exception caused by any of its <see cref="IActivityLoggerClient"/> (resp. <see cref="IActivityLoggerSink"/>) before removing or 
            /// disabling them.
            /// See <see cref="OnError"/> event that may be raised this method.
            /// </summary>
            /// <param name="comment"></param>
            /// <param name="ex"></param>
            public void Add( string comment, Exception ex )
            {
                if( ex == null ) throw new ArgumentNullException( "ex" );
                if( comment == null ) comment = String.Empty;
                lock( _exceptions )
                {
                    _exceptions.Push( new LoggedError( comment, ex ) );
                }
                Interlocked.Exchange( ref _raiseFlag, 1 );
                ThreadPool.QueueUserWorkItem( DoRaiseInBackground );
            }

            private void DoRaiseInBackground( object unusedState )
            {
                // This CompareExchange guaranties that no Add will be lost and
                // that no duplicated event will fire.
                while( Interlocked.CompareExchange( ref _raiseFlag, 0, 1 ) == 1 )
                {
                    // This lock guaranties that no more than one event will fire at the same time.
                    lock( _raiseLock )
                    {
                        // Thread-safe (C# 4.0 compiler use CompareExchange).
                        EventHandler h = OnErrorFromBackgroundThreads;
                        if( h != null )
                        {
                            // h.GetInvocationList() creates an independant copy of Delegate[].
                            foreach( Delegate d in h.GetInvocationList() )
                            {
                                try
                                {
                                    d.DynamicInvoke( this, EventArgs.Empty );
                                }
                                catch( Exception ex2 )
                                {
                                    OnErrorFromBackgroundThreads -= (EventHandler)d;
                                    lock( _exceptions )
                                    {
                                        _exceptions.Push( new LoggedError( R.ErrorWhileRaisingLogError, ex2 ) );
                                    }
                                    Interlocked.Exchange( ref _raiseFlag, 1 );
                                }
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Clears the list. No <see cref="OnError"/> is raised.
            /// </summary>
            public void Clear()
            {
                lock( _exceptions )
                {
                    _exceptions.Clear();
                }
            }

            /// <summary>
            /// Obtains a copy of the last (up to) 50 errors.
            /// </summary>
            /// <returns>An independent array.</returns>
            public LoggedError[] ToArray()
            {
                lock( _exceptions )
                {
                    return _exceptions.ToArray();
                }
            }

        }

    }
}
