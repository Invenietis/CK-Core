using System;
using System.Reflection;


namespace CK.Plugin
{
    /// <summary>
    /// Configures the <see cref="IServiceHost"/>.
    /// </summary>
    public interface IServiceHostConfiguration
    {
        /// <summary>
        /// Returns the <see cref="ServiceLogMethodOptions"/> for the given method.
        /// </summary>
        /// <param name="m">Method for which options should be obtained.</param>
        /// <returns>Configuration for the method.</returns>
        ServiceLogMethodOptions GetOptions( MethodInfo m );

        /// <summary>
        /// Returns the <see cref="ServiceLogEventOptions"/> for the given event.
        /// </summary>
        /// <param name="e">Event for which options should be obtained.</param>
        /// <returns>Configuration for the event.</returns>
        ServiceLogEventOptions GetOptions( EventInfo e );
    }

}
