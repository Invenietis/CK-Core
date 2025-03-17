namespace CK.Core;

/// <summary>
/// Characterizes the root of a <see cref="NormalizedPath"/>.
/// </summary>
public enum NormalizedPathRootKind : byte
{
    /// <summary>
    /// Relative path.
    /// </summary>
    None = 0,

    /// <summary>
    /// Marks a path that is rooted because of its <see cref="NormalizedPath.FirstPart"/>.
    /// A path that starts with a tilde (~/) is rooted as well as a path whose first ends with a colon (:).
    /// <para>
    /// When the first part ends with a colon, there's 2 cases:
    /// <list type="bullet">
    ///     <item>The part is ":" alone (length = 1) or "X:" (length = 2): this RootedByFirstPart applies.</item>
    ///     <item>The part is longer than 3 characters (like "ni:"): this denotes a <see cref="RootedByURIScheme"/> kind.</item>
    /// </list>
    /// "C:" is RootedByFirstPart whereas "ni:" is <see cref="RootedByURIScheme"/>.
    /// </para>
    /// </summary>
    RootedByFirstPart = 1,

    /// <summary>
    /// When '/' or '\' starts the path, it is rooted.
    /// The <see cref="NormalizedPath.FirstPart"/> does not contain the separator (there can even be
    /// no parts at all), but the <see cref="NormalizedPath.Path"/> (and <see cref="NormalizedPath.ToString()"/>)
    /// starts with it (normalized to <see cref="NormalizedPath.DirectorySeparatorChar"/>).
    /// </summary>
    RootedBySeparator = 2,

    /// <summary>
    /// When double separators ("//" or "\\") starts the path, it is rooted.
    /// The <see cref="NormalizedPath.FirstPart"/> does not contain the separators (there can even be
    /// no parts at all), but the <see cref="NormalizedPath.Path"/> (and <see cref="NormalizedPath.ToString()"/>)
    /// starts with them (normalized to <see cref="NormalizedPath.DirectorySeparatorChar"/>).
    /// </summary>
    RootedByDoubleSeparator = 3,

    /// <summary>
    /// When a path starts with "XX:" (at least 3 characters long, including the ending colon), it is considered
    /// to be a "URI scheme" ("X:" is <see cref="RootedByFirstPart"/>).
    /// <para>
    /// With this kind of root, <see cref="NormalizedPath.FirstPart"/> ends with ":/" (this is the only case where a part
    /// contains the <see cref="NormalizedPath.DirectorySeparatorChar"/>).
    /// </para>
    /// </summary>
    RootedByURIScheme = 4
}
