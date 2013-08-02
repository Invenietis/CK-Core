using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.RouteConfig
{
    public class MetaInsertActionConfiguration : Impl.MetaConfiguration
    {
        string _name;
        string _declaredName;

        public MetaInsertActionConfiguration( string name, string declarationName )
        {
            _name = name;
            _declaredName = declarationName;
        }

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
            return CheckNameValidity( routeName, monitor, _name ) && CheckNameValidity( routeName, monitor, _declaredName );
        }

        protected internal override void Apply( Impl.IRouteConfigurationContext context )
        {
            context.AddDeclaredAction( _name, _declaredName, false );
        }
    }
}
