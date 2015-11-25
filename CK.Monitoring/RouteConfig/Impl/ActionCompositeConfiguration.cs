using System;
using System.Collections.Generic;
using System.Linq;
using CK.Core;

namespace CK.RouteConfig.Impl
{
    /// <summary>
    /// Defines a composite action that can be a <see cref="ActionParallelConfiguration"/> or a <see cref="ActionSequenceConfiguration"/>.
    /// </summary>
    public class ActionCompositeConfiguration : ActionConfiguration
    {
        readonly List<ActionConfiguration> _children;
        readonly bool _isParallel;
        const string _seqDisplayName = "Sequence";
        const string _parDisplayName = "Parallel";

        /// <summary>
        /// Protected constructor.
        /// </summary>
        /// <param name="name">Action's name.</param>
        /// <param name="isParallel">Whether this composite is a parallel.</param>
        protected ActionCompositeConfiguration( string name, bool isParallel )
            : base( name )
        {
            _children = new List<ActionConfiguration>();
            _isParallel = isParallel;
        }

        ActionCompositeConfiguration( ActionCompositeConfiguration toCopy, bool copyCompositeOnly )
            : base( toCopy.Name )
        {
            _isParallel = toCopy.IsParallel;
            if( copyCompositeOnly )
            {
                _children = new List<ActionConfiguration>( toCopy.Children );
                for( int i = 0; i < _children.Count; ++i )
                {
                    ActionCompositeConfiguration composite = _children[i] as ActionCompositeConfiguration;
                    if( composite != null ) _children[i] = new ActionCompositeConfiguration( composite, true );
                }
            }
            else
            {
                _children = new List<ActionConfiguration>( toCopy.Children.Select( c => c.IsCloneable ? c.Clone() : c ) );
            }
        }

        /// <summary>
        /// Gets whether this is a parallel composite.
        /// </summary>
        public bool IsParallel { get { return _isParallel; } }

        /// <summary>
        /// Gets children (items of this composite) actions.
        /// </summary>
        public IReadOnlyList<ActionConfiguration> Children { get { return _children; } }

        /// <summary>
        /// Checks that children are valid (action's name must be unique).
        /// </summary>
        /// <param name="routeName">Name of the route that references this action.</param>
        /// <param name="monitor">Monitor to report errors.</param>
        /// <returns>True if valid, false otherwise.</returns>
        public override bool CheckValidity( string routeName, IActivityMonitor monitor )
        {
            if( monitor == null ) throw new ArgumentNullException( "monitor" );
            bool result = true;
            for( int i = 0; i < _children.Count; ++i )
            {
                var child = _children[i];
                bool validChild = true;
                for( int j = ++i; j < _children.Count; ++j )
                {
                    if( _children[j].Name == child.Name )
                    {
                        monitor.Error().Send( "Duplicate action name '{0}' in {1} '{2}', route '{3}'.", child.Name, TypeDisplayName, Name, routeName );
                        validChild = false;
                    }
                }
                validChild &= child.CheckValidity( routeName, monitor );
                // Remove child to continue the process but avoid cascading errors.
                if( !validChild ) _children.RemoveAt( i-- );
                result &= validChild;
            }
            return result;
        }

        /// <summary>
        /// Adds an <see cref="ActionConfiguration"/> to this composite.
        /// </summary>
        /// <param name="action">The action to add.</param>
        protected void Add( ActionConfiguration action )
        {
            if( action == null ) throw new ArgumentNullException( "action" );
            _children.Add( action );
        }

        /// <summary>
        /// Overrides (replaces) an <see cref="ActionConfiguration"/> at a specified index.
        /// </summary>
        /// <param name="idx">Index to replace.</param>
        /// <param name="action">The new action to inject.</param>
        internal void Override( int idx, ActionConfiguration action )
        {
            if( action == null ) throw new ArgumentNullException( "action" );
            _children[idx] = action;
        }

        internal bool CheckAndOptimize( IActivityMonitor monitor )
        {
            bool formalError = false;
            HashSet<string> formalNames = new HashSet<string>();
            formalNames.Add( Name );
            for( int i = 0; i < _children.Count; ++i )
            {
                ActionConfiguration a = _children[i];
                if( !formalNames.Add( a.Name ) )
                {
                    if( a.Name == Name )
                    {
                        monitor.Warn().Send( "Action n°{2} '{0}' has the name of its parent {1}.", a.Name, TypeDisplayName, i+1 );
                    }
                    else
                    {
                        if( _isParallel )
                        {
                            monitor.Warn().Send( "Duplicate name '{0}' found in {1} (action n°{2}).", a.Name, _parDisplayName, i + 1 );
                        }
                        else
                        {
                            monitor.Error().Send( "Action n°{2}'s name '{0}' alreay appears in {1}. In a {1}, names must be unique.", a.Name, _seqDisplayName );
                            formalError = true;
                        }
                    }
                }
                ActionCompositeConfiguration composite = a as ActionCompositeConfiguration;
                if( composite != null )
                {
                    formalError |= composite.CheckAndOptimize( monitor );
                    if( composite.IsParallel == _isParallel )
                    {
                        _children.RemoveAt( i );
                        _children.InsertRange( i, composite.Children );
                        i += composite.Children.Count;
                    }
                }
            }
            return !formalError;
        }

        private string TypeDisplayName
        {
            get { return _isParallel ? _parDisplayName : _seqDisplayName; }
        }

        /// <summary>
        /// Always true since one can always clone a composite.
        /// </summary>
        public override bool IsCloneable
        {
            get { return true; }
        }

        /// <summary>
        /// Clones this composite.
        /// </summary>
        /// <returns>A clone of this composite.</returns>
        public override ActionConfiguration Clone()
        {
            return CloneComposite( false );
        }

        internal ActionCompositeConfiguration CloneComposite( bool cloneCompositeOnly )
        {
            return new ActionCompositeConfiguration( this, cloneCompositeOnly );
        }

    }
}
