#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\ActivityLoggerTests.cs) is part of CiviKey. 
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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;
using CK.Core;
using NUnit.Framework;

namespace CK.Core.Tests
{

    class SafeClient : IActivityLoggerClient
    {
        readonly StringBuilder _buffer = new StringBuilder();
        string _prefix = "";

        public void OnFilterChanged( LogLevelFilter current, LogLevelFilter newValue )
        {
            lock( _buffer ) _buffer.Append( _prefix ).AppendLine( "[OnFilterChanged]" );
        }

        public void OnUnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
        {
            lock( _buffer ) _buffer.Append( _prefix ).AppendFormat( "[{0}]{1}", level, text ).AppendLine();
        }

        public void OnOpenGroup( IActivityLogGroup group )
        {
            lock( _buffer )
            {
                _buffer.Append( _prefix ).AppendFormat( ">[{0}]{1}", group.GroupLevel, group.GroupText ).AppendLine();
                _prefix += " ";
            }
        }

        public void OnGroupClosing( IActivityLogGroup group, ref System.Collections.Generic.List<ActivityLogGroupConclusion> conclusions )
        {
            lock( _buffer ) _buffer.Append( _prefix ).AppendFormat( "[Closing]{0}", group.GroupText ).AppendLine();
        }

        public void OnGroupClosed( IActivityLogGroup group, ICKReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            lock( _buffer )
            {
                _prefix = _prefix.Substring( 0, _prefix.Length - 1 );
                _buffer.Append( _prefix ).AppendFormat( "<[Closed]{0}", group.GroupText ).AppendLine();
            }
        }

        public override string ToString()
        {
            lock( _buffer ) return _buffer.ToString();
        }
    }

    class SafeSink : IActivityLoggerSink
    {
        readonly StringBuilder _buffer = new StringBuilder();
        string _prefix = "";

        public void OnEnterLevel( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
        {
            lock( _buffer ) _buffer.Append( _prefix ).AppendFormat( "[{0}]{1}", level, text ).AppendLine();
        }

        public void OnContinueOnSameLevel( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
        {
            lock( _buffer ) _buffer.Append( _prefix ).AppendFormat( "[{0}]{1}", level, text ).AppendLine();
        }

        public void OnLeaveLevel( LogLevel level )
        {
        }

        public void OnGroupOpen( IActivityLogGroup group )
        {
            lock( _buffer )
            {
                _buffer.Append( _prefix ).AppendFormat( ">[{0}]{1}", group.GroupLevel, group.GroupText ).AppendLine();
                _prefix += " ";
            }
        }

        public void OnGroupClose( IActivityLogGroup group, ICKReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            lock( _buffer )
            {
                _prefix = _prefix.Substring( 0, _prefix.Length - 1 );
                _buffer.Append( _prefix ).AppendFormat( "<[Closed]{0}", group.GroupText ).AppendLine();
            }
        }

        public override string ToString()
        {
            lock( _buffer ) return _buffer.ToString();
        }

    }

    class BuggyClient : IActivityLoggerClient
    {
        readonly ThreadContext _c;
        readonly double _probFailPerCall;
        public readonly int NumClient;
        public bool Failed;
        public bool FailureHasBeenReceivedThroughEvent;

        public BuggyClient( ThreadContext c, int numClient, double probFailPerCall = 0.2 )
        {
            NumClient = numClient;
            _c = c;
            _probFailPerCall = probFailPerCall;
        }

        void MayFail()
        {
            if( _c.Rand.NextDouble() < _probFailPerCall )
            {
                Failed = true;
                throw new CKException( "BuggyClient{0} failed.", NumClient );
            }
        }

        #region IActivityLoggerClient Members

        public void OnFilterChanged( LogLevelFilter current, LogLevelFilter newValue )
        {
            MayFail();
        }

        public void OnUnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
        {
            MayFail();
        }

        public void OnOpenGroup( IActivityLogGroup group )
        {
            MayFail();
        }

        public void OnGroupClosing( IActivityLogGroup group, ref System.Collections.Generic.List<ActivityLogGroupConclusion> conclusions )
        {
            MayFail();
        }

        public void OnGroupClosed( IActivityLogGroup group, ICKReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            MayFail();
        }

        #endregion
    }

    class BuggySink : IActivityLoggerSink
    {
        readonly ThreadContext _c;
        readonly double _probFailPerCall;
        public readonly int NumSink;
        public bool Failed;
        public bool FailureHasBeenReceivedThroughEvent;

        public BuggySink( ThreadContext c, int numSink, double probFailPerCall = 0.2 )
        {
            _c = c;
            NumSink = numSink;
            _probFailPerCall = probFailPerCall;
        }

        void MayFail()
        {
            if( _c.Rand.NextDouble() < _probFailPerCall )
            {
                Failed = true;
                throw new CKException( "BuggySink{0} failed.", NumSink );
            }
        }

        #region IActivityLoggerSink Members

        public void OnEnterLevel( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
        {
            MayFail();
        }

        public void OnContinueOnSameLevel( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
        {
            MayFail();
        }

        public void OnLeaveLevel( LogLevel level )
        {
            MayFail();
        }

        public void OnGroupOpen( IActivityLogGroup group )
        {
            MayFail();
        }

        public void OnGroupClose( IActivityLogGroup group, ICKReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            MayFail();
        }

        #endregion
    }

    class ThreadContext
    {
        readonly ActivityLoggerErrorLogs _context;
        readonly IActivityLogger _logger;
        readonly public int NumLogger;
        readonly public int OperationCount;
        readonly public Random Rand;

        public ThreadContext( ActivityLoggerErrorLogs context, int numLogger, int buggyClientCount, int buggySinkCount, int operationCount )
        {
            _context = context;
            NumLogger = numLogger;
            OperationCount = operationCount;
            Rand = new Random();
            _logger = CreateLoggerWithBuggyListeners( buggyClientCount, buggySinkCount );
        }

        public void Run()
        {
            _logger.Info( "ThreadContext{0}Begin", NumLogger );
            for( int i = 0; i < OperationCount; ++i )
            {
                double op = Rand.NextDouble();
                if( op < 1.0/60 ) _logger.Filter = _logger.Filter == LogLevelFilter.Trace ? LogLevelFilter.Info : LogLevelFilter.Trace;
                
                if( op < 1.0/3 ) _logger.Info( "OP-{0}-{1}", NumLogger, i );
                else if( op < 2.0/3 ) _logger.OpenGroup( LogLevel.Info, "G-OP-{0}-{1}", NumLogger, i );
                else _logger.CloseGroup();
            }
            _logger.Info( "ThreadContext{0}End", NumLogger );
        }

        IDefaultActivityLogger CreateLoggerWithBuggyListeners( int buggyClientCount, int buggySinkCount )
        {
            IDefaultActivityLogger logger = new DefaultActivityLogger();
            for( int i = 0; i < buggyClientCount; ++i ) logger.Output.RegisterClient( _context.NewBuggyClient( this ) );
            for( int i = 0; i < buggySinkCount; ++i ) logger.Tap.Register( _context.NewBuggySink( this ) );
            logger.Output.Register( _context.SafeClient );
            logger.Tap.Register( _context.SafeSink );
            return logger;
        }

        internal void CheckResult( string clientText, string sinkText )
        {
            Assert.That( clientText, Is.StringContaining( String.Format( "ThreadContext{0}Begin", NumLogger ) ) );
            Assert.That( sinkText, Is.StringContaining( String.Format( "ThreadContext{0}Begin", NumLogger ) ) );

            Assert.That( clientText, Is.StringContaining( String.Format( "ThreadContext{0}End", NumLogger ) ) );
            Assert.That( sinkText, Is.StringContaining( String.Format( "ThreadContext{0}End", NumLogger ) ) );
        }
    }

    [TestFixture]
    [ExcludeFromCodeCoverage]
    [Category( "ActivityLogger" )]
    public class ActivityLoggerErrorLogs
    {
        internal SafeClient SafeClient;
        internal SafeSink SafeSink;
        Random _random;
        List<BuggyClient> _buggyClients;
        List<BuggySink> _buggySinks;
        List<ThreadContext> _contexts;
        double _probFailurePerOperation;

        [Test]
        public void LoggerErrorTrap()
        {
            // Run this test for at least X second.
            int nbSecond = 2;
            Stopwatch w = new Stopwatch();
            w.Start();
            do
            {
                OneRun( threadCount: 30, operationCount: 225 );
            }
            while( w.ElapsedMilliseconds < nbSecond*1000 );
        }


        void InitializeEnv( int threadCount, int operationCount, int buggyClientCount, int buggySinkCount, double probFailurePerOperation, double probBuggyOnErrorHandlerFailure )
        {
            SafeClient = new SafeClient();
            SafeSink = new SafeSink();
            _random = new Random();
            _buggyClients = new List<BuggyClient>();
            _buggySinks = new List<BuggySink>();
            _contexts = new List<ThreadContext>();
            _probFailurePerOperation = probFailurePerOperation;
            for( int i = 0; i < threadCount; ++i ) _contexts.Add( new ThreadContext( this, _contexts.Count, buggyClientCount, buggySinkCount, operationCount ) );
            _inSafeErrorHandler = false;
            _maxNumberOfErrorReceivedAtOnce = 0;
            _lastSequenceNumberReceived = ActivityLogger.LoggingError.NextSequenceNumber - 1;
            _errorsFromBackground = new ConcurrentBag<string>();
            _probBuggyOnErrorHandlerFailure = probBuggyOnErrorHandlerFailure;
            _buggyOnErrorHandlerFailCount = 0;
            _buggyOnErrorHandlerReceivedCount = 0;
            _nbNotCleared = 0;
        }

        internal BuggyClient NewBuggyClient( ThreadContext c ) 
        {
            var r = new BuggyClient( c, _buggyClients.Count, _random.NextDouble() / 5 );
            _buggyClients.Add( r );
            return r;
        }

        internal BuggySink NewBuggySink( ThreadContext c ) 
        {
            var r = new BuggySink( c, _buggySinks.Count, _random.NextDouble() / 5 );
            _buggySinks.Add( r );
            return r;
        }
        
        void OneRun( int threadCount, int operationCount )
        {
            ActivityLogger.LoggingError.Capacity = 300;

            InitializeEnv( 
                threadCount: threadCount, 
                buggyClientCount: 10, 
                buggySinkCount: 10, 
                operationCount: 250,
                probFailurePerOperation: 0.9 / operationCount,
                probBuggyOnErrorHandlerFailure: 0.1 );

            ActivityLogger.LoggingError.OnErrorFromBackgroundThreads += SafeOnErrorHandler;
            ActivityLogger.LoggingError.OnErrorFromBackgroundThreads += BuggyOnErrorHandler;

            int nextSeq = ActivityLogger.LoggingError.NextSequenceNumber;
            RunAllAndWaitForTermination();

            Console.WriteLine( @"ActivityLogger.LoggingError Test:
            ThreadCount: {0}
            DispatchQueuedWorkItemCount: {1}
            OptimizedDispatchQueuedWorkItemCount: {2}
            Errors handled: {3}
            Errors from Error handler: {4}
            Error not Cleared while raised: {5}", 
                threadCount, 
                ActivityLogger.LoggingError.DispatchQueuedWorkItemCount, 
                ActivityLogger.LoggingError.OptimizedDispatchQueuedWorkItemCount,
                ActivityLogger.LoggingError.NextSequenceNumber - nextSeq,
                _buggyOnErrorHandlerReceivedCount,
                _nbNotCleared );

            Assert.That( _nbNotCleared, Is.GreaterThan( 0 ), "Clear is called from SafeOnErrorHandler each 10 errors." );
            Assert.That( ActivityLogger.LoggingError.Capacity, Is.EqualTo( 500 ), "Changed in SafeOnErrorHandler." );
            CollectionAssert.IsEmpty( _errorsFromBackground );
            var buggyClientMismatch = _buggyClients.Where( c => c.Failed != c.FailureHasBeenReceivedThroughEvent ).ToArray();
            var buggySinkMismatch = _buggySinks.Where( s => s.Failed != s.FailureHasBeenReceivedThroughEvent ).ToArray();
            CollectionAssert.IsEmpty( buggyClientMismatch );
            CollectionAssert.IsEmpty( buggySinkMismatch );
            string clientText = SafeClient.ToString();
            string sinkText = SafeSink.ToString();
            for( int i = 0; i < _contexts.Count; ++i ) _contexts[i].CheckResult( clientText, sinkText );
            Assert.That( _buggyOnErrorHandlerReceivedCount, Is.GreaterThan( 0 ), "There must be at least one error from the buggy handler." );
            Assert.That( _buggyOnErrorHandlerReceivedCount, Is.EqualTo( _buggyOnErrorHandlerFailCount ) );

            ActivityLogger.LoggingError.OnErrorFromBackgroundThreads -= SafeOnErrorHandler;
            ActivityLogger.LoggingError.OnErrorFromBackgroundThreads -= BuggyOnErrorHandler;

            Assert.That( ActivityLogger.LoggingError.DispatchQueuedWorkItemCount, Is.GreaterThan( 0 ), "Of course, events have been raised..." );
            Assert.That( ActivityLogger.LoggingError.OptimizedDispatchQueuedWorkItemCount, Is.GreaterThan( 0 ), "Optimizations must have saved us some works." );
            Assert.That( _nbNotCleared, Is.GreaterThan( 0 ), "Clear is called from SafeOnErrorHandler each 20 errors." );

        }


        bool _inSafeErrorHandler;
        int _maxNumberOfErrorReceivedAtOnce;
        int _lastSequenceNumberReceived;
        int _nbNotCleared;
        // One can not use Assert.That from a background thread since it throws... an exception that we handle :-).
        // Instead we collect strings.
        ConcurrentBag<string> _errorsFromBackground;

        void SafeOnErrorHandler( object source, CriticalErrorCollector.ErrorEventArgs e )
        {
            if( _inSafeErrorHandler )
            {
                _errorsFromBackground.Add( "Error events are not raised simultaneously." );
                return;
            }
            
            // Ass soon as the first error, we increase the capacity to avoid losing any error.
            // This tests the tread-safety of the operation and shows that no deadlock occur (we are 
            // receiving an error event and can safely change the internal buffer capacity).
            ActivityLogger.LoggingError.Capacity = 500;

            _inSafeErrorHandler = true;
            _maxNumberOfErrorReceivedAtOnce = Math.Max( _maxNumberOfErrorReceivedAtOnce, e.LoggingErrors.Count );
            foreach( var error in e.LoggingErrors )
            {
                if( error.SequenceNumber % 10 == 0 ) _nbNotCleared += ActivityLogger.LoggingError.Clear();
                if( _lastSequenceNumberReceived != error.SequenceNumber - 1 )
                {
                    _errorsFromBackground.Add( String.Format( "Received {0}, expected {1}.", error.SequenceNumber - 1, _lastSequenceNumberReceived ) );
                    _inSafeErrorHandler = false;
                    return;
                }
                _lastSequenceNumberReceived = error.SequenceNumber;
                string msg = error.Exception.Message;
                if( msg.StartsWith( "BuggySink" ) )
                {
                    int idx = Int32.Parse( Regex.Match( msg, "\\d+" ).Value );
                    _buggySinks[idx].FailureHasBeenReceivedThroughEvent = true;
                }
                else if( msg.StartsWith( "BuggyClient" ) )
                {
                    int idx = Int32.Parse( Regex.Match( msg, "\\d+" ).Value );
                    _buggyClients[idx].FailureHasBeenReceivedThroughEvent = true;
                }
                else if( msg.StartsWith( "BuggyErrorHandler" ) )
                {
                    ++_buggyOnErrorHandlerReceivedCount;
                    if( !error.ToString().StartsWith( R.ErrorWhileCollectorRaiseError ) )
                    {
                        _errorsFromBackground.Add( "Bad comment for error handling." );
                        _inSafeErrorHandler = false;
                        return;
                    }
                }
                else
                {
                    _errorsFromBackground.Add( "Unexpected error: " + error.Exception.Message );
                    _inSafeErrorHandler = false;
                    return;
                }
            }
            _inSafeErrorHandler = false;
        }

        double _probBuggyOnErrorHandlerFailure;
        int _buggyOnErrorHandlerFailCount;
        int _buggyOnErrorHandlerReceivedCount;

        void BuggyOnErrorHandler( object source, CriticalErrorCollector.ErrorEventArgs e )
        {
            // Force at least one error regardless of the probablity.
            if( _buggyOnErrorHandlerFailCount == 0 || _random.NextDouble() < _probBuggyOnErrorHandlerFailure )
            {
                // Subscribe again to this buggy event.
                ActivityLogger.LoggingError.OnErrorFromBackgroundThreads += BuggyOnErrorHandler;
                throw new CKException( "BuggyErrorHandler{0}", _buggyOnErrorHandlerFailCount++ );
            }
        }

        private void RunAllAndWaitForTermination()
        {
            int loggerCount = _contexts.Count;
            Assert.That( loggerCount % 2, Is.EqualTo( 0 ) );
            Task[] tasks = new Task[loggerCount / 2];
            Thread[] threads = new Thread[loggerCount / 2];

            for( int i = 0; i < tasks.Length; i++ ) tasks[i] = new Task( _contexts[i].Run );
            for( int i = 0; i < threads.Length; i++ ) threads[i] = new Thread( _contexts[tasks.Length + i].Run );

            for( int i = 0; i < loggerCount; i++ )
                if( i % 2 == 0 ) threads[i / 2].Start();
                else tasks[i / 2].Start();

            Task.WaitAll( tasks );
            for( int i = 0; i < threads.Length; i++ ) threads[i].Join();

            // Note: 
            // Thread.Sleep(0): yields to any thread of same or higher priority on any processor.
            // Thread.Sleep(1): yields to any thread on any processor.
            // Here we want to let any thread run.
            //
            // When using Sleep(0), this pooling sometimes never terminates (the background thread did 
            // not have the opportunity to decrease the ActivityLogger._waitingRaiseCount internal counter).
            // This indicates that the background thread had a lower priority than this Test thread.
            //
            // while( ActivityLogger.LoggingError.OnErrorFromBackgroundThreadsPending ) Thread.Sleep( 1 );
            //
            // The right way to wait for something to happen is to block a thread until a signal unblocks it.
            // This is what the following function is doing.
            ActivityLogger.LoggingError.WaitOnErrorFromBackgroundThreadsPending();
            Assert.That( ActivityLogger.LoggingError.OnErrorFromBackgroundThreadsPending, Is.False, "Since nobody calls ActivityLogger.Add. In real situations, this would not necessarily be true." );
        }

    }
}
