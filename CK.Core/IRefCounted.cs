namespace CK.Core;

/// <summary>
/// Basic reference counting interface.
/// </summary>
public interface IRefCounted
{
    /// <summary>
    /// Adds a reference to this object.
    /// </summary>
    void AddRef();

    /// <summary>
    /// Releases a reference to this object.
    /// </summary>
    void Release();
}
