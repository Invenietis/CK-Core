using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Core;

/// <summary>
/// Yet another system clock. See https://github.com/dotnet/extensions/issues/151.
/// This is mainly for testing purposes: a basic implementation and default is available here: <see cref="SystemClock"/>.
/// </summary>
public interface ISystemClock
{
    /// <summary>
    /// Gets the <see cref="DateTime.UtcNow"/> or a modified time depending
    /// on the actual implementation.
    /// </summary>
    DateTime UtcNow { get; }
}
