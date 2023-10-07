using Microsoft.Extensions.Configuration;
using System.Linq;

namespace CK.Core
{
    /// <summary>
    /// Extends configuration objects.
    /// </summary>
    public static class ConfigurationSectionExtension
    {
        /// <summary>
        /// Handles opt-in or opt-out section that can have "true" or "false" value or children.
        /// <para>
        /// Note that for convenience, this extension method can be called on a null this <paramref name="parent"/>:
        /// the section doesn't obviously exists and <paramref name="optOut"/> value applies.
        /// </para>
        /// </summary>
        /// <param name="parent">This parent configuration. Can be null.</param>
        /// <param name="path">The configuration key or a path to a subordinated key.</param>
        /// <param name="optOut">
        /// <list type="bullet">
        ///   <item>
        ///     True to consider unexisting section to be the default configuration.
        ///     To skip the configuration, the section must have a "false" value.
        ///   </item>
        ///   <item>
        ///     False to ignore an unexisting section.
        ///     To apply the default configuration, the section must have a "true" value.
        ///   </item>
        /// </list>
        /// </param>
        /// <param name="content">Non null if the section has content: configuration applies.</param>
        /// <returns>
        /// True if the configuration applies (if <paramref name="content"/> is null, the default configuration must be applied),
        /// false if the configuration must be skipped.
        /// </returns>
        public static bool ShouldApplyConfiguration( this ImmutableConfigurationSection? parent,
                                                     string path,
                                                     bool optOut,
                                                     out ImmutableConfigurationSection? content )
        {
            content = parent?.TryGetSection( path );
            if( content == null ) return optOut;
            if( bool.TryParse( content.Value, out var b ) )
            {
                content = null;
                return b;
            }
            if( !content.HasChildren )
            {
                content = null;
                return optOut;
            }
            return true;
        }

        /// <summary>
        /// Handles opt-in or opt-out section that can have "true" or "false" value or children.
        /// <para>
        /// Note that for convenience, this extension method can be called on a null this <paramref name="parent"/>:
        /// the section doesn't obviously exists and <paramref name="optOut"/> value applies.
        /// </para>
        /// </summary>
        /// <param name="parent">This parent configuration. Can be null.</param>
        /// <param name="path">The configuration key or a path to a subordinated key.</param>
        /// <param name="optOut">
        /// <list type="bullet">
        ///   <item>
        ///     True to consider unexisting section to be the default configuration.
        ///     To skip the configuration, the section must have a "false" value.
        ///   </item>
        ///   <item>
        ///     False to ignore an unexisting section.
        ///     To apply the default configuration, the section must have a "true" value.
        ///   </item>
        /// </list>
        /// </param>
        /// <param name="content">Non null if the section has content: configuration applies.</param>
        /// <returns>
        /// True if the configuration applies (if <paramref name="content"/> is null, the default configuration must be applied),
        /// false if the configuration must be skipped.
        /// </returns>
        public static bool ShouldApplyConfiguration( this IConfiguration? parent,
                                                     string path,
                                                     bool optOut,
                                                     out IConfigurationSection? content )
        {
            content = parent?.GetSection( path );
            if( content == null || !content.Exists() )
            {
                content = null;
                return optOut;
            }
            if( bool.TryParse( content.Value, out var b ) )
            {
                content = null;
                return b;
            }
            return true;
        }
    }
}
