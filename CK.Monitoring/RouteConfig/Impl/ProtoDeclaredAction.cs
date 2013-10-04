using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.RouteConfig.Impl
{
    class ProtoDeclaredAction
    {
        ActionConfiguration _action;
        bool _isCloned;

        public ProtoDeclaredAction( ActionConfiguration a )
        {
            _action = a;
        }

        public ActionConfiguration Action
        {
            get { return _action; }
        }

        internal bool Override( IActivityMonitor monitor, IReadOnlyList<string> fullPath, ActionConfiguration a )
        {
            var e = fullPath.GetEnumerator();
            if( !e.MoveNext() ) throw new ArgumentException( "Must not be empty.", "fullPath" );
            if( e.Current != _action.Name ) throw new ArgumentException( "Must start with the action name.", "fullPath" );
            if( !e.MoveNext() )
            {
                _action = a;
                _isCloned = false;
                monitor.Info( "Action '{0}' has been overridden.", a.Name );
                return true;
            }
            ActionCompositeConfiguration parent;
            int idx = FindInComposite( e, out parent );
            if( idx >= 0 )
            {
                Debug.Assert( _action is ActionCompositeConfiguration, "It is a composite." );
                Debug.Assert( _action.IsCloneable, "A composite is cloneable." );
                if( !_isCloned )
                {
                    _action = ((ActionCompositeConfiguration)_action).CloneComposite( true );
                    monitor.Info( "Action '{0}' has been cloned in order to override an inner action.", String.Join( "/", fullPath ) );
                    _isCloned = true;
                    idx = FindInComposite( e, out parent );
                }
                Debug.Assert( parent.Children[idx].Name == fullPath.Last() );
                parent.Override( idx, a );
                monitor.Info( "Inner action '{0}' has been overridden.", String.Join( "/", fullPath ) );
                return true;
            }
            monitor.Error( "Action '{0}' not found. Unable to override it.", String.Join( "/", fullPath ) );
            return false;
        }

        int FindInComposite( IEnumerator<string> path, out ActionCompositeConfiguration parent )
        {
            parent = null;
            ActionCompositeConfiguration composite = _action as ActionCompositeConfiguration;
            if( composite == null ) return -1;
            return FindInComposite( composite, path, ref parent );
        }

        static int FindInComposite( ActionCompositeConfiguration start, IEnumerator<string> path, ref ActionCompositeConfiguration parent )
        {
            string current = path.Current;
            for( int i = 0; i < start.Children.Count; ++i )
            {
                if( start.Children[i].Name == current )
                {
                    if( !path.MoveNext() )
                    {
                        parent = start;
                        return i;
                    }
                    ActionCompositeConfiguration newStart = start.Children[i] as ActionCompositeConfiguration;
                    if( newStart == null ) return -1;
                    return FindInComposite( newStart, path, ref parent );
                }
            }
            return -1;
        }

    }
}
