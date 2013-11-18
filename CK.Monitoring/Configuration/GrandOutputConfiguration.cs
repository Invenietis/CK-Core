using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CK.Core;
using CK.Monitoring.GrandOutputHandlers;
using CK.RouteConfig;

namespace CK.Monitoring
{
    public class GrandOutputConfiguration
    {
        RouteConfiguration _routeConfig;
        LogFilter? _appDomainDefaultFilter;

        public GrandOutputConfiguration()
        {
        }

        public bool LoadFromFile( string path, IActivityMonitor monitor )
        {
            if( path == null ) throw new ArgumentNullException( "path" );
            if( monitor == null ) throw new ArgumentNullException( "monitor" );
            try
            {
                var doc = XDocument.Load( path, LoadOptions.SetBaseUri|LoadOptions.SetLineInfo );
                return Load( doc.Root, monitor );
            }
            catch( Exception ex )
            {
                monitor.Error().Send( ex );
                return false;
            }
        }

        public bool Load( XElement xmlGrandOutputConfiguration, IActivityMonitor monitor )
        {
            if( xmlGrandOutputConfiguration == null ) throw new ArgumentNullException( "xml" );
            if( monitor == null ) throw new ArgumentNullException( "monitor" );
            try
            {
                if( xmlGrandOutputConfiguration.Name != "GrandOutputConfiguration" ) throw new XmlException( "Element name must be <GrandOutputConfiguration>." );
                _appDomainDefaultFilter = xmlGrandOutputConfiguration.GetAttributeLogFilter( "AppDomainDefaultFilter", false );
                _routeConfig = new RouteConfiguration();
                FillRoute( monitor, xmlGrandOutputConfiguration, _routeConfig );
                return true;
            }
            catch( Exception ex )
            {
                monitor.Error().Send( ex );
            }
            return false;
        }

        /// <summary>
        /// Gets the default filter for the application domain. 
        /// This value is set on the static <see cref="ActivityMonitor.DefaultFilter"/> by <see cref="GrandOutput.SetConfiguration"/>
        /// if and only if the configured GrandOutput is the <see cref="GrandOutput.Default"/>.
        /// </summary>
        public LogFilter? AppDomainDefaultFilter { get { return _appDomainDefaultFilter; } }

        internal RouteConfiguration RouteConfiguration
        {
            get { return _routeConfig; }
        }

        void FillRoute( IActivityMonitor monitor, XElement xml, RouteConfiguration route )
        {
            route.ConfigData = new GrandOutputChannelConfigData( xml );
            foreach( var e in xml.Elements() )
            {
                switch( e.Name.LocalName )
                {
                    case "Channel":
                        route.DeclareRoute( FillSubRoute( monitor, e, new SubRouteConfiguration( e.AttributeRequired( "Name" ).Value, null ) ) );
                        break;
                    case "Parallel": 
                    case "Sequence":
                    case "Add": DoSequenceOrParallelOrAdd( monitor, a => route.AddAction( a ), e );
                        break;
                    default: throw new XmlException( "Element name must be <Add>, <Parallel>, <Sequence> or <Channel>." );
                }
            }
        }

        SubRouteConfiguration FillSubRoute( IActivityMonitor monitor, XElement xml, SubRouteConfiguration sub )
        {
            FillRoute( monitor, xml, sub );
            sub.RoutePredicate = CreatePredicate( xml.AttributeRequired( "TopicFilter" ).Value );
            return sub;
        }

        void DoSequenceOrParallelOrAdd( IActivityMonitor monitor, Action<ActionConfiguration> collector, XElement xml )
        {
            if( xml.Name == "Parallel" || xml.Name == "Sequence" )
            {
                Action<ActionConfiguration> elementCollector;
                if( xml.Name == "Parallel" )
                {
                    var p = new ActionParallelConfiguration( xml.AttributeRequired( "Name" ).Value );
                    elementCollector = a => p.AddAction( a );
                    collector( p );
                }
                else
                {
                    var s = new ActionSequenceConfiguration( xml.AttributeRequired( "Name" ).Value );
                    elementCollector = a => s.AddAction( a );
                    collector( s );
                }
                foreach( var action in xml.Elements() ) DoSequenceOrParallelOrAdd( monitor, collector, action );
            }
            else
            {
                if( xml.Name != "Add" ) throw new XmlException( String.Format( "Unknown element '{0}': only <Add>, <Parallel> or <Sequence>.", xml.Name ) );
                string type = xml.AttributeRequired( "Type" ).Value;
                Type t = FindConfigurationType( type );
                HandlerConfiguration hC = (HandlerConfiguration)Activator.CreateInstance( t, xml.AttributeRequired( "Name" ).Value );
                hC.DoInitialize( monitor, xml );
                collector( hC );
            }
        }

        Func<string, bool> CreatePredicate( string pattern )
        {
            string r = "^" + Regex.Escape( pattern ).Replace( @"\*", ".*" ).Replace( @"\?", "." ) + "$";
            Regex re = new Regex( r, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline );
            return re.IsMatch;
        }

        static Type FindConfigurationType( string type )
        {
            Type t = SimpleTypeFinder.WeakDefault.ResolveType( type, false );
            if( t == null )
            {
                string fullTypeName, assemblyFullName;
                if( !SimpleTypeFinder.SplitAssemblyQualifiedName( type, out fullTypeName, out assemblyFullName ) )
                {
                    fullTypeName = type;
                    assemblyFullName = "CK.Monitoring";
                }
                if( !fullTypeName.EndsWith( "Configuration" ) ) fullTypeName += "Configuration";
                t = SimpleTypeFinder.WeakDefault.ResolveType( fullTypeName + ", " + assemblyFullName, false );
                if( t == null )
                {
                    t = SimpleTypeFinder.WeakDefault.ResolveType( "CK.Monitoring.GrandOutputHandlers." + fullTypeName + ", " + assemblyFullName, true );
                }
            }
            return t;
        }


    }
}
