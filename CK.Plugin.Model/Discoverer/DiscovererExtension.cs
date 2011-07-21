
using System.Text;

namespace CK.Plugin
{
    public static class DiscovererExtension
    {
        /// <summary>
        /// Gets the method's signature.
        /// </summary>
        /// <param name="m">This <see cref="ISimpleMethodInfo"/>.</param>
        /// <returns>The signature (return type, name and parameter types, types are ).</returns>
        public static string GetSimpleSignature( this ISimpleMethodInfo m )
        {
            return AppendSimpleSignature( m, new StringBuilder() ).ToString();
        }

        /// <summary>
        /// Writes the method's signature into a <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="m">This <see cref="ISimpleMethodInfo"/>.</param>
        /// <returns>The string builder (to allow fluent syntax).</returns>
        public static StringBuilder AppendSimpleSignature( this ISimpleMethodInfo m, StringBuilder b )
        {
            b.Append( m.ReturnType ).Append( ' ' ).Append( m.Name ).Append( '(' );
            foreach( var p in m.Parameters ) b.Append( p.ParameterType ).Append( ',' );
            b.Length = b.Length - 1;
            b.Append( ')' );
            return b;
        }
    }
}
