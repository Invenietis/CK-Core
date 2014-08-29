using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CK.Core;

namespace CK.Monitoring.Server.UI
{
    public partial class TabContentControl : UserControl, IClientApplicationView
    {
        public TabContentControl()
        {
            InitializeComponent();
        }

        public delegate void InvokeDelegate();

        public void BindClientApplication( ClientApplication model )
        {
            model.Monitors.CollectionChanged += Monitors_CollectionChanged;

            ClientMonitorTreeView.NodeMouseClick += ClientMonitorTreeView_NodeMouseClick;

            foreach( var m in model.Monitors )
            {
                AddMonitorNode( this.ClientMonitorTreeView, m );
            }
        }

        /// <summary>
        /// This is raised when the monitors collection changed for the current client.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Monitors_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        {
            if( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add )
            {
                BeginInvoke( new InvokeDelegate( () =>
                {
                    foreach( ClientMonitor mon in e.NewItems )
                    {
                        AddMonitorNode( this.ClientMonitorTreeView, mon );
                    }
                } ) );
            }
        }

        /// <summary>
        /// Adds a node to the Monitor Tree view.
        /// </summary>
        /// <param name="treeView"></param>
        /// <param name="m"></param>
        void AddMonitorNode( TreeView treeView, ClientMonitor m )
        {
            TreeNode node = new TreeNode( m.MonitorId.ToString() );
            node.Name = m.MonitorId.ToString();
            node.Tag = m;
            treeView.Nodes.Add( node );
        }

        ActivityMonitor _replay;
        ClientMonitor _currentMonitor;

        /// <summary>
        /// When a monitor is selected, display the logs of this monitor. 
        /// This is done by replaying all "memory" logs.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ClientMonitorTreeView_NodeMouseClick( object sender, TreeNodeMouseClickEventArgs e )
        {
            if( _currentMonitor != null ) _currentMonitor.Entries.CollectionChanged -= Entries_CollectionChanged;

            _currentMonitor = e.Node.Tag as ClientMonitor;

            if( _currentMonitor != null )
            {
                // Creates a monitor for re/playing logs
                _replay = new ActivityMonitor( false );
                _replay.Output.RegisterClient( new TreeNodeClient( this.LogView ) );

                LogView.Nodes.Clear();

                _currentMonitor.Entries.CollectionChanged += Entries_CollectionChanged;
                var currentEntries = new List<ClientLogEntry>( _currentMonitor.Entries );
                foreach( ClientLogEntry log in currentEntries ) LogToReplayMonitor( log );
            }
        }


        /// <summary>
        /// When the log entry collection changed, update the log view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Entries_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        {
            if( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add )
            {
                BeginInvoke( new InvokeDelegate( () =>
                {
                    foreach( ClientLogEntry log in e.NewItems )
                    {
                        LogToReplayMonitor( log );
                    }
                } ) );
            }
        }


        /// <summary>
        /// Log a <see cref="IMulticastLogEntry"/> to the replay monitor
        /// </summary>
        /// <param name="replay"></param>
        /// <param name="log"></param>
        private void LogToReplayMonitor( ClientLogEntry clientLog )
        {
            if( _replay != null )
            {
                if( clientLog.IsMissingEntry )
                {
                    _replay.Info().Send( "Missing data" );
                }
                else
                {
                    IMulticastLogEntry log = clientLog.LogEntry;

                    if( log.LogType == LogEntryType.OpenGroup )
                    {
                        _replay.UnfilteredOpenGroup( log.Tags, log.LogLevel, null, log.Text, log.LogTime, CKException.CreateFrom( log.Exception ), log.FileName, log.LineNumber );
                    }
                    if( log.LogType == LogEntryType.Line )
                    {
                        _replay.UnfilteredLog( log.Tags, log.LogLevel, log.Text ?? String.Empty, log.LogTime, CKException.CreateFrom( log.Exception ), log.FileName, log.LineNumber );
                    }
                    if( log.LogType == LogEntryType.CloseGroup )
                    {
                        _replay.CloseGroup( log.LogTime, log.Conclusions );
                    }
                }
            }
        }

        class TreeNodeClient : ActivityMonitorTextHelperClient
        {
            TreeView _treeView;
            TreeNode _currentNode;

            public TreeNodeClient( TreeView treeView )
            {
                _treeView = treeView;
            }

            protected override void OnEnterLevel( ActivityMonitorLogData data )
            {
                var node = CreateNode( data );
                AddNode( node );
            }

            protected override void OnContinueOnSameLevel( ActivityMonitorLogData data )
            {
                OnEnterLevel( data );
            }

            protected override void OnLeaveLevel( LogLevel level )
            {
            }

            protected override void OnGroupOpen( IActivityLogGroup group )
            {
                TreeNode groupNode = CreateNode( group );
                AddNode( groupNode );

                _currentNode = groupNode;
            }

            protected override void OnGroupClose( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
            {
                TreeNode closeGroupNode = CreateNode( group );
                AddNode( closeGroupNode );

                _currentNode = _currentNode.Parent;
            }

            private void AddNode( TreeNode node )
            {
                if( _currentNode == null )
                {
                    _treeView.Nodes.Add( node );
                }
                else
                {
                    _currentNode.Nodes.Add( node );
                }
            }

            private static TreeNode CreateNode( IActivityLogGroup group )
            {
                TreeNode item = new TreeNode();
                item.Name = group.LogTime.ToString();
                item.Text = group.GroupText;
                item.ToolTipText = group.GroupLevel.ToString();
                return item;
            }

            private static TreeNode CreateNode( ActivityMonitorLogData data )
            {
                var node = new TreeNode();
                node.Name = data.LogTime.ToString();
                node.Text = data.Text;
                node.ToolTipText = node.Level.ToString();
                return node;
            }

        }
    }
}
