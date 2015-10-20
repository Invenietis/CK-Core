using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Globalization;

namespace CK.Core
{
    /// <summary>
    /// Extension methods for <see cref="XmlReader"/> and <see cref="XElement"/>
    /// classes.
    /// </summary>
    public static class XmlExtension
    {
        /// <summary>
        /// Gets line and column information (if it exists) as a string from any <see cref="XObject"/> (such as <see cref="XAttribute"/> or <see cref="XElement"/>).
        /// </summary>
        /// <param name="this">This <see cref="IXmlLineInfo"/>.</param>
        /// <param name="format">Default format is "- @Line,Column".</param>
        /// <param name="noLineInformation">Defaults to a null string when <see cref="IXmlLineInfo.HasLineInfo()"/> is false.</param>
        /// <returns>A string based on <paramref name="format"/> or <paramref name="noLineInformation"/>.</returns>
        static public string GetLineColumnString( this IXmlLineInfo @this, string format = "- @{0},{1}", string noLineInformation = null )
        {
            if( @this.HasLineInfo() ) return String.Format( format, @this.LineNumber, @this.LinePosition );
            return noLineInformation;
        }

        /// <summary>
        /// Gets the attribute by its name or throws an <see cref="XmlException"/> if it does not exist.
        /// </summary>
        /// <param name="this">This <see cref="XElement"/>.</param>
        /// <param name="name">Name of the attribute.</param>
        static public XAttribute AttributeRequired( this XElement @this, XName name )
        {
            XAttribute a = @this.Attribute( name );
            if( a == null ) throw new XmlException( String.Format( Resources.ExpectedXmlAttribute, name ) + @this.GetLineColumnString() );
            return a;
        }

        /// <summary>
        /// Gets an enum value.
        /// </summary>
        /// <typeparam name="T">Type of the enum. There is no way (in c#) to constraint the type to Enum - nor to Delegate, this is why 
        /// the constraint restricts only the type to be a value type.</typeparam>
        /// <param name="this">This <see cref="XElement"/>.</param>
        /// <param name="name">Name of the attribute.</param>
        /// <param name="defaultValue">Default value if the attribute does not exist or can not be parsed.</param>
        /// <returns>The parsed value or the default value.</returns>
        static public T AttributeEnum<T>( this XElement @this, XName name, T defaultValue ) where T : struct
        {
            T result;
            XAttribute a = @this.Attribute( name );
            if( a == null || !Enum.TryParse( a.Value, out result ) ) result = defaultValue;
            return result;
        }

    }
}
