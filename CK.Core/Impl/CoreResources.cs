namespace CK.Core.Impl;

/// <summary>
/// Resource class, for looking up strings.
/// </summary>
public class CoreResources
{
#pragma warning disable 1591

    public static readonly string AggregatedExceptionsMustContainAtLeastOne = "AggregatedExceptions must contain at least one exception.";

    public static readonly string FactoryTesterMismatch = "The 'factory' function must create an item that satisfies the 'tester' function.";

    public static readonly string FileUtilUnableToCreateUniqueTimedFileOrFolder = "Unable to create a unique timed file or folder.";

    public static readonly string InnerExceptionMustBeTheFirstAggregatedException = "The InnerException must be the first AggregatedExceptions.";

    public static readonly string TagsMustBelongToTheSameContext = "Tags must belong to the same context.";

    public static readonly string ServiceAlreadyDirectlySupported = "Service {0} is directly supported by the container.";

    public static readonly string DirectServicesCanNotBeDisabled = "Service {0} is directly supported by the container. It can not be disabled.";

    public static readonly string ServiceImplCallbackTypeMismatch = "Service {0} is not implemented by object {1} returned by the callback.";

    public static readonly string ServiceImplTypeMismatch = "Service {0} is not implemented by object {1}.";

    public static readonly string ServiceAlreadyRegistered = "Service {0} is already registered by the container.";

    public static readonly string UnregisteredServiceInServiceProvider = "Unable to find service '{0}'.";

    public static readonly string TagsMustNotBeMultiLineString = "Tag must not be multi line (CR and LF characters are forbidden).";
}
