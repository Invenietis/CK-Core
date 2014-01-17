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
    /// <summary>
    /// Defines configuration for <see cref="GrandOutput"/>.
    /// This object is not thread safe. New GrandOutputConfiguration can be created when needed (and 
    /// method <see cref="LoadFromFile"/> or <see cref="Load"/> called).
    /// </summary>
    public class GrandOutputConfiguration
    {
        RouteConfiguration _routeConfig;
        Dictionary<string, LogFilter> _sourceFilter;
        SourceFilterApplyMode _sourceFilterApplyMode;
        LogFilter? _appDomainDefaultFilter;

        /// <summary>
        /// Initializes a new <see cref="GrandOutputConfiguration"/>.
        /// </summary>
        public GrandOutputConfiguration()
        {
        }

        /// <summary>
        /// Loads this configuration from a <see cref="XElement"/>.
        /// </summary>
        /// <param name="path">Path to the configuration xml file.</param>
        /// <param name="monitor">Monitor that will be used.</param>
        /// <returns>True on success, false if the configuration can not be read.</returns>
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

        /// <summary>
        /// Loads this configuration from a <see cref="XElement"/>.
        /// </summary>
        /// <param name="xmlGrandOutputConfiguration">The xml element.</param>
        /// <param name="monitor">Monitor that will be used.</param>
        /// <returns>True on success, false if the configuration can not be read.</returns>
        public bool Load( XElement xmlGrandOutputConfiguration, IActivityMonitor monitor )
        {
            if( xmlGrandOutputConfiguration == null ) throw new ArgumentNullException( "xmlGrandOutputConfiguration" );
            if( monitor == null ) throw new ArgumentNullException( "monitor" );
            try
            {
                if( xmlGrandOutputConfiguration.Name != "GrandOutputConfiguration" ) throw new XmlException( "Element name must be <GrandOutputConfiguration>." );
                LogFilter? appDomainFilter = xmlGrandOutputConfiguration.GetAttributeLogFilter( "AppDomainDefaultFilter", false );

                XElement channelElement = xmlGrandOutputConfiguration.Element( "Channel" );
                if( channelElement == null )
                {
                    monitor.Error().Send( "Missing <Channel /> element." );
                    return false;
                }
                SourceFilterApplyMode applyMode;
                Dictionary<string, LogFilter> sourceFilter = ReadSourceFilter( xmlGrandOutputConfiguration, out applyMode, monitor );
                if( sourceFilter == null ) return false;

                RouteConfiguration routeConfig = FillRoute( monitor, channelElement, new RouteConfiguration() );

                // No error: set the new values.
                _routeConfig = routeConfig;
                _sourceFilter = sourceFilter;
                _sourceFilterApplyMode = applyMode;
                _appDomainDefaultFilter = appDomainFilter;
                return true;
            }
            catch( Exception ex )
            {
                monitor.Error().Send( ex );
            }
            return false;
        }

        /// <summary>
        /// Gets or sets the default filter for the application domain. 
        /// This value is set on the static <see cref="ActivityMonitor.DefaultFilter"/> by <see cref="GrandOutput.SetConfiguration"/>
        /// if and only if the configured GrandOutput is the <see cref="GrandOutput.Default"/>.
        /// </summary>
        public LogFilter? AppDomainDefaultFilter 
        {
            get { return _appDomainDefaultFilter; }
            set { _appDomainDefaultFilter = value; } 
        }

        /// <summary>
        /// Gets the source file filter mapping: if and only if the configured GrandOutput is the <see cref="GrandOutput.Default"/>, 
        /// the <see cref="GrandOutput.SetConfiguration"/> method clears any existing source file filters and sets this ones.
        /// </summary>
        public Dictionary<string, LogFilter> SourceFilter
        {
            get { return _sourceFilter; }
        }

        /// <summary>
        /// Gets or sets the
        /// </summary>
        public SourceFilterApplyMode SourceFilterApplicationMode
        {
            get { return _sourceFilterApplyMode; }
            set { _sourceFilterApplyMode = value; }
        }

        /// <summary>
        /// Gets the channels configuration. 
        /// This is not specific to GrandOutput channels: care must be taken when working directly with this object. 
        /// Loading from xml should be used.
        /// </summary>
        public RouteConfiguration ChannelsConfiguration
        {
            get { return _routeConfig; }
        }

        static Dictionary<string, LogFilter> ReadSourceFilter( XElement e, out SourceFilterApplyMode apply, IActivityMonitor monitor )
        {
            apply = SourceFilterApplyMode.None;
            try
            {
                var s = e.Element( "SourceFilter" );
                if( s == null ) return new Dictionary<string, LogFilter>();
                apply = s.GetAttributeEnum( "ApplyMode", SourceFilterApplyMode.ClearThenApply );
                return e.Elements( "SourceFilter" )
                        .Elements( "File" )
                        .Select( f => new { File = f.Attribute( "File" ), Filter = f.GetAttributeLogFilter( "Filter", true ) } )
                        .Where( f => f.File != null && !String.IsNullOrWhiteSpace( f.File.Value ) && f.Filter != LogFilter.Undefined )
                        .ToDictionary( f => f.File.Value, f => f.Filter.Value );
            }
            catch( Exception ex )
            {
                monitor.Error().Send( ex, "Unable to read SourceFilter element." );
                return null;
            }
        }

        RouteConfiguration FillRoute( IActivityMonitor monitor, XElement xml, RouteConfiguration route )
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
            return route;
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

        static Func<string, bool> CreatePredicate( string pattern )
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
