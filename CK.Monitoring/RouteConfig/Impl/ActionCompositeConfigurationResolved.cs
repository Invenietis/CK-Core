using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.RouteConfig.Impl
{
    class ActionCompositeConfigurationResolved : ActionConfigurationResolved
    {
        readonly List<ActionConfigurationResolved> _children;
        readonly bool _isParallel;
        const string _seqDisplayName = "Sequence";
        const string _parDisplayName = "Parallel";

        internal ActionCompositeConfigurationResolved( IActivityMonitor monitor, int index, IReadOnlyList<string> path, ActionCompositeConfiguration a, bool flattenUselessComposite )
            : base( index, path, a )
        {
            _isParallel = a.IsParallel;
            _children = new List<ActionConfigurationResolved>();
            AppendChildren( monitor, a, path.Append( a.Name ).ToReadOnlyList(), flattenUselessComposite );
        }

        void AppendChildren( IActivityMonitor monitor, ActionCompositeConfiguration a, IReadOnlyList<string> childPath, bool flattenUselessComposite )
        {
            foreach( var child in a.Children )
            {
                ActionCompositeConfiguration composite = child as ActionCompositeConfiguration;
                if( flattenUselessComposite && composite != null && composite.IsParallel == a.IsParallel )
                {
                    AppendChildren( monitor, composite, childPath = childPath.Append( composite.Name ).ToReadOnlyList(), true );
                }
                else _children.Add( ActionConfigurationResolved.Create( monitor, child, flattenUselessComposite, _children.Count, childPath ) );
            }
        }

        public new ActionCompositeConfiguration ActionConfiguration { get { return (ActionCompositeConfiguration)base.ActionConfiguration; } }

        public IReadOnlyList<ActionConfigurationResolved> Children { get { return _children; } }


     }
}
