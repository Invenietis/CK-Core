
namespace CK.Monitoring
{
    /// <summary>
    /// Defines how the <see cref="GrandOutputConfiguration"/> applies its <see cref="GrandOutputConfiguration.SourceOverrideFilter"/>
    /// to the application domain's global <see cref="ActivityMonitor.SourceFilter.FilterSource"/>.
    /// </summary>
    public enum SourceFilterApplyMode
    {
        /// <summary>
        /// Source filters is ignored.
        /// </summary>
        None = 0,

        /// <summary>
        /// Clears the current <see cref="ActivityMonitor.SourceFilter.FilterSource"/>.
        /// </summary>
        Clear = 1,

        /// <summary>
        /// Clears the current <see cref="ActivityMonitor.SourceFilter.FilterSource"/> and then applies the new ones.
        /// </summary>
        ClearThenApply = 2,

        /// <summary>
        /// Applies the filters.
        /// </summary>
        Apply = 3
    }

}
