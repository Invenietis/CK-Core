using System;

namespace CK.Core
{
    /// <summary>
    /// Optional attribute for <see cref="IAutoService"/> implementation that
    /// declares that this implementation replaces another one (the replaced implementation
    /// is the single constructor parameter).
    /// <para>
    /// Note that this attribute is useless if this implementation specializes the replaced service since
    /// discovering the most precise implementation is one of the key goal of Auto services handling.
    /// </para>
    /// <para>
    /// It is also useless if the replaced service is used by this implementation: as long as a parameter with the
    /// same type appears in its constructor, this service "covers" (and possibly reuses) the replaced one.
    /// </para>
    /// <para>
    /// This attribute, just like <see cref="IRealObject"/>, <see cref="IAutoService"/>, <see cref="IScopedAutoService"/>
    /// and <see cref="ISingletonAutoService"/> can be created anywhere: the name must be ReplaceAutoServiceAttribute
    /// and a constructor with a Type and/or a constructor with a string must be defined.
    /// </para>
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = false )]
    public class ReplaceAutoServiceAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new <see cref="ReplaceAutoServiceAttribute"/> that specifies the type of the
        /// replaced service.
        /// </summary>
        /// <param name="replaced">The type of the service that this service replaces. Must not be null.</param>
        public ReplaceAutoServiceAttribute( Type replaced )
        {
            ReplacedType = replaced ?? throw new ArgumentNullException( nameof( replaced ) );
        }

        /// <summary>
        /// Initializes a new <see cref="ReplaceAutoServiceAttribute"/> that specifies the assembly
        /// qualified name of the replaced service type.
        /// </summary>
        /// <param name="replacedAssemblyQualifiedName">The type of the service that this service replaces. Must not be null or white space.</param>
        public ReplaceAutoServiceAttribute( string replacedAssemblyQualifiedName )
        {
            if( String.IsNullOrWhiteSpace( replacedAssemblyQualifiedName ) ) throw new ArgumentNullException( nameof( replacedAssemblyQualifiedName ) );
            ReplacedAssemblyQualifiedName = replacedAssemblyQualifiedName;
        }

        /// <summary>
        /// Gets the type of the service that this service replaces.
        /// </summary>
        public Type? ReplacedType { get; private set; }

        /// <summary>
        /// Gets the assembly qualified name of the replaced service type.
        /// </summary>
        public string? ReplacedAssemblyQualifiedName { get; private set; }
    }
}
