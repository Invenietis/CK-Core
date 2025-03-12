using System;

namespace CK.Core;

/// <summary>
/// Disposable <see cref="IUtf8JsonReaderContext"/>.
/// </summary>
public interface IDisposableUtf8JsonReaderContext : IUtf8JsonReaderContext, IDisposable
{
}
