using System;
using CK.Core;

namespace CK.Plugin
{

    /// <summary>
    /// Qualifies the type of error during plugin management.
    /// </summary>
    public enum ExecutionPlanResultStatus
    {
        /// <summary>
        /// No error.
        /// </summary>
        Success = 0,
        
        /// <summary>
        /// An error occured while loading (activating) the plugin.
        /// </summary>
        LoadError = 1,
        
        /// <summary>
        /// An error occured during the call to <see cref="IPlugin.Setup"/>.
        /// </summary>
        SetupError = 2,

        /// <summary>
        /// An error occured during the call to <see cref="IPlugin.Start"/>.
        /// </summary>
        StartError = 3

    }

    /// <summary>
    /// Defines the return of the <see cref="IPluginHost.Execute"/> method.
    /// </summary>
    public interface IExecutionPlanResult
    {
        /// <summary>
        /// Kind of error.
        /// </summary>
        ExecutionPlanResultStatus Status { get; }
        
        /// <summary>
        /// The plugin that raised the error.
        /// </summary>
        IPluginInfo Culprit { get; }

        /// <summary>
        /// Detailed error information specific to the <see cref="IPlugin.Setup"/> phasis.
        /// </summary>
        IPluginSetupInfo SetupInfo { get; }

        /// <summary>
        /// Gets the exception if it exists (note that a <see cref="IPlugin.Setup"/> may not throw exception but simply 
        /// returns false).
        /// </summary>
        Exception Error { get; }
    }
}
