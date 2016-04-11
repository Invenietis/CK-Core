using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using CK.Core;
using CK.Core.Impl;

namespace CK.Monitoring
{
    /// <summary>
    /// This client writes .ckmon files for one monitor.
    /// To close output file, simply <see cref="IActivityMonitorOutput.UnregisterClient">unregister</see> this client.
    /// </summary>
    public sealed class CKMonWriterClient : IActivityMonitorBoundClient, IMulticastLogInfo
    {
        readonly string _path;
        readonly int _maxCountPerFile;
        readonly LogFilter _minimalFilter;
        readonly bool _useGzipCompression;
        IActivityMonitorImpl _source;
        MonitorBinaryFileOutput _file;
        int _currentGroupDepth;
        LogEntryType _prevLogType;
        DateTimeStamp _prevlogTime;

        /// <summary>
        /// Initializes a new instance of <see cref="CKMonWriterClient"/> that can be registered to write uncompressed .ckmon file for this monitor.
        /// </summary>
        /// <param name="path">The path. Can be absolute. When relative, it will be under <see cref="SystemActivityMonitor.RootLogPath"/> that must be set.</param>
        /// <param name="maxCountPerFile">Maximum number of entries per file. Must be greater than 1.</param>
        public CKMonWriterClient( string path, int maxCountPerFile )
            : this( path, maxCountPerFile, LogFilter.Undefined, false )
        {
        }
        /// <summary>
        /// Initializes a new instance of <see cref="CKMonWriterClient"/> that can be registered to write compressed or uncompressed .ckmon file for this monitor.
        /// </summary>
        /// <param name="path">The path. Can be absolute. When relative, it will be under <see cref="SystemActivityMonitor.RootLogPath"/> that must be set.</param>
        /// <param name="maxCountPerFile">Maximum number of entries per file. Must be greater than 1.</param>
        /// <param name="minimalFilter">Minimal filter for this client.</param>
        /// <param name="useGzipCompression">Whether to output compressed .ckmon files. Defaults to false (do not compress).</param>
        public CKMonWriterClient( string path, int maxCountPerFile, LogFilter minimalFilter, bool useGzipCompression = false )
        {
            _path = path;
            _maxCountPerFile = maxCountPerFile;
            _minimalFilter = minimalFilter;
            _useGzipCompression = useGzipCompression;
        }
        /// <summary>
        /// Gets the minimal filter set by the constructor.
        /// </summary>
        public LogFilter MinimalFilter { get { return _minimalFilter; } }
        void IActivityMonitorBoundClient.SetMonitor( IActivityMonitorImpl source, bool forceBuggyRemove )
        {
            if( source != null && _source != null ) throw ActivityMonitorClient.CreateMultipleRegisterOnBoundClientException( this );
            // Silently ignore null => null or monitor => same monitor.
            if( source != _source )
            {
                _prevLogType = LogEntryType.None;
                _prevlogTime = DateTimeStamp.Unknown;
                Debug.Assert( (source == null) != (_source == null) );
                if( (_source = source) == null )
                {
                    if( _file != null ) _file.Close();
                    _file = null;
                }
                else
                {
                    // If initialization failed, we let the file null: this monitor will not
                    // work (the error will appear in the Critical errors) but this avoids
                    // an exception to be thrown here.
                    var f = new MonitorBinaryFileOutput( _path, ((IUniqueId)_source).UniqueId, _maxCountPerFile, _useGzipCompression );
                    if( f.Initialize( new SystemActivityMonitor( false, null ) ) )
                    {
                        var g = _source.CurrentGroup;
                        _currentGroupDepth = g != null ? g.Depth : 0;
                        _file = f;
                    }
                }
            }
        }
        /// <summary>
        /// Opens this writer if it is not already opened.
        /// </summary>
        /// <returns>True on success, false otherwise.</returns>
        public bool Open()
        {
            using( _source.ReentrancyAndConcurrencyLock() )
            {
                if( _source == null ) throw new InvalidOperationException( "CKMonWriterClient must be registered in an ActivityMonitor." );
                if( _file != null ) return true;
                _file = new MonitorBinaryFileOutput( _path, _source.UniqueId, _maxCountPerFile, _useGzipCompression );
                _prevLogType = LogEntryType.None;
                _prevlogTime = DateTimeStamp.Unknown;
            }
            using( SystemActivityMonitor.EnsureSystemClient( _source ) )
            {
                using( _source.ReentrancyAndConcurrencyLock() )
                {
                    if( _file.Initialize( _source ) )
                    {
                        var g = _source.CurrentGroup;
                        _currentGroupDepth = g != null ? g.Depth : 0;
                    }
                    else _file = null;
                }
            }
            return _file != null;
        }
        /// <summary>
        /// Closes this writer if it <see cref="IsOpened"/>.
        /// It can be re-<see cref="Open"/>ed later.
        /// </summary>
        public void Close()
        {
            using( _source != null ? _source.ReentrancyAndConcurrencyLock() : null )
            {
                if( _file != null ) _file.Close();
                _file = null;
            }
        }
        /// <summary>
        /// Gets whether this writer is opened.
        /// </summary>
        public bool IsOpened
        {
            get { return _file != null; }
        }

        #region Auto implementation of IMulticastLogInfo to call UnicastWrite on file.
        Guid IMulticastLogInfo.MonitorId
        {
            get
            {
                Debug.Assert( _source != null && _file != null );
                return _source.UniqueId;
            }
        }
        int IMulticastLogInfo.GroupDepth
        {
            get
            {
                Debug.Assert( _source != null && _file != null );
                return _currentGroupDepth;
            }
        }
        LogEntryType IMulticastLogInfo.PreviousEntryType
        {
            get
            {
                Debug.Assert( _source != null && _file != null );
                return _prevLogType;
            }
        }
        DateTimeStamp IMulticastLogInfo.PreviousLogTime
        {
            get
            {
                Debug.Assert( _source != null && _file != null );
                return _prevlogTime;
            }
        }
        #endregion
        void IActivityMonitorClient.OnUnfilteredLog( ActivityMonitorLogData data )
        {
            if( _file != null )
            {
                _file.UnicastWrite( data, this );
                _prevlogTime = data.LogTime;
                _prevLogType = LogEntryType.Line;
            }
        }
        void IActivityMonitorClient.OnOpenGroup( IActivityLogGroup group )
        {
            if( _file != null )
            {
                _file.UnicastWriteOpenGroup( group, this );
                ++_currentGroupDepth;
                _prevlogTime = group.LogTime;
                _prevLogType = LogEntryType.OpenGroup;
            }
        }
        void IActivityMonitorClient.OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            if( _file != null )
            {
                _file.UnicastWriteCloseGroup( group, conclusions, this );
                --_currentGroupDepth;
                _prevlogTime = group.CloseLogTime;
                _prevLogType = LogEntryType.CloseGroup;
            }
        }
        void IActivityMonitorClient.OnGroupClosing( IActivityLogGroup group, ref List<ActivityLogGroupConclusion> conclusions )
        {
        }
        void IActivityMonitorClient.OnTopicChanged( string newTopic, string fileName, int lineNumber )
        {
            // Does nothing.
        }
        void IActivityMonitorClient.OnAutoTagsChanged( CKTrait newTrait )
        {
            // Does nothing.
        }
    }
}