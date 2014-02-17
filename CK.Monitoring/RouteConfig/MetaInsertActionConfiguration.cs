using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.RouteConfig
{
    /// <summary>
    /// Enables inserting in the route an already declared action.
    /// </summary>
    public class MetaInsertActionConfiguration : Impl.MetaConfiguration
    {
        string _name;
        string _declaredName;

        /// <summary>
        /// Initializes a new <see cref="MetaInsertActionConfiguration"/> with the name of the action and the name of the 
        /// previously declared action.
        /// </summary>
        /// <param name="name">Name of the action to insert.</param>
        /// <param name="declarationName">Declared action's name.</param>
        public MetaInsertActionConfiguration( string name, string declarationName )
        {
            _name = name;
            _declaredName = declarationName;
        }

        /// <summary>
        /// Gets or sets the name of the declared action. Never null: defaults to <see cref="String.Empty"/>.
        /// </summary>
        public string DeclaredName
        {
            get { return _declaredName; }
            set { _declaredName = value ?? String.Empty; }
        }

        protected internal override bool CheckValidity( string routeName, IActivityMonitor monitor )
        {
            // If we support insertion in a composite we will allow name with / inside/
            // If we allow / in DeclaredName, it means that an action inside a Composite can be inserted
            // somewhere else... This is possible technically, but does it make sense?
            return CheckActionNameValidity( routeName, monitor, _name ) && CheckActionNameValidity( routeName, monitor, _declaredName );
        }

        protected internal override void Apply( Impl.IRouteConfigurationContext context )
        {
            context.AddDeclaredAction( _name, _declaredName, false );
        }
    }
}
