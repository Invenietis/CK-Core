using System.Collections.Generic;

namespace CK.RouteConfig.Impl
{
    /// <summary>
    /// Intermediate objects that captures the first step of configuration resolution.
    /// At this step we manipulate <see cref="MetaConfiguration"/> objects.
    /// </summary>
    public interface IProtoRoute
    {
        /// <summary>
        /// Gets the associated <see cref="RouteConfiguration"/> object.
        /// </summary>
        RouteConfiguration Configuration { get; }

        /// <summary>
        /// Gets the namespace of this route.
        /// </summary>
        string Namespace { get; }

        /// <summary>
        /// Gets the full name of this route.
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Gets the list of <see cref="MetaConfiguration"/> objects such as <see cref="MetaAddActionConfiguration"/> or <see cref="MetaRemoveActionConfiguration"/>.
        /// </summary>
        IReadOnlyList<MetaConfiguration> MetaConfigurations { get; }

        /// <summary>
        /// Finds a previously declared action.
        /// The action can exist in the parent routes if <see cref="SubRouteConfiguration.ImportParentDeclaredActionsAbove"/> is true (which is the default).
        /// </summary>
        /// <param name="name">Name of an existing action.</param>
        /// <returns>Null or the action with the name.</returns>
        ActionConfiguration FindDeclaredAction( string name );

        /// <summary>
        /// Gets the list of subordinated route.
        /// </summary>
        IReadOnlyList<IProtoSubRoute> SubRoutes { get; }
    }

}
