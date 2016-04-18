using System;

namespace CK.Core.Impl
{
    /// <summary>
    /// Resource class, for looking up strings.
    /// </summary>
    public class CoreResources
    {
#pragma warning disable 1591

        public static readonly string AggregatedExceptionsMustContainAtLeastOne = "AggregatedExceptions must contain at least one exception.";

        public static readonly string AppSettingsAlreadyInitialized = "ApplicationSettings have already been initialized. It can be initialized only once.";

        public static readonly string AppSettingsDefaultInitializationFailed = "Unable to initialize AppSettings, the default fallback to System.Configuration.ConfigurationManager.AppSettings can not be generated since System.Configuration assembly is not available.";

        public static readonly string AppSettingsNoDefaultInitializationOnNetCore = "Unable to automatically initialize AppSettings on .Net standard platform. It must be explicitly initialized.";        

        public static readonly string AppSettingsRequiredConfigurationBadType = "Required AppSettings configuration named '{0}' is missing: it must be a '{1}'.";

        public static readonly string AppSettingsRequiredConfigurationMissing = "Required AppSettings configuration named '{0}' is missing.";

        public static readonly string ArgumentCountNegative = "Argument count can not be negative.";

        public static readonly string ArgumentMustNotBeNullOrWhiteSpace = "Argument must not be null or whitespace.";

        public static readonly string CapacityMustBeGreaterThanOrEqualToZero = "Capacity must be greater than or equal to zero.";

        public static readonly string DateTimeMustBeUtc = "DateTime must be Utc. Use DateTime.UtcNow to obtain it for instance.";

        public static readonly string ErrorWhileCollectorRaiseError = "An error handler raised the error. It has been removed from the CriticalErrorCollector.OnErrorFromBackgroundThreads event.";

        public static readonly string ExceptionWhileResolvingType = "An exception occured while resolving type: {0}.";

        public static readonly string ExpectedXmlAttribute = "Expected attribute '{0}'.";

        public static readonly string FactoryTesterMismatch = "The 'factory' function must create an item that satisfies the 'tester' function.";

        public static readonly string FIFOBufferEmpty = "FIFOBuffer is empty.";

        public static readonly string FileMustExist = "File must exist.";

        public static readonly string FileUtilNoReadOnlyWhenCreateFile = "Access set to FileAccess.Read is stupid when creating a file.";

        public static readonly string FileUtilUnableToCreateUniqueTimedFile = "Unable to create a unique timed file.";

        public static readonly string InnerExceptionMustBeTheFirstAggregatedException = "The InnerException must be the first AggregatedExceptions.";

        public static readonly string InvalidAssemblyQualifiedName = "'{0}' is not a valid assembly qualified name.";

        public static readonly string StringMatcherForwardPastEnd = "Unable to forward past the end of the string.";

        public static readonly string TraitsMustBelongToTheSameContext = "Traits must belong to the same context.";
        
        public static readonly string ServiceAlreadyDirectlySupported = "Service {0} is directly supported by the container.";

        public static readonly string DirectServicesCanNotBeDisabled = "Service {0} is direcly supported by the container. It can not be disabled.";

        public static readonly string ServiceImplCallbackTypeMismatch = "Service {0} is not implemented by object {1} returned by the callback.";

        public static readonly string ServiceImplTypeMismatch = "Service {0} is not implemented by object {1}.";

        public static readonly string ServiceAlreadyRegistered = "Service {0} is already registered by the container.";

        public static readonly string UnregisteredServiceInServiceProvider = "Unable to find service '{0}'.";
    }
}
