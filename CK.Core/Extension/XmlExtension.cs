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

        #region IXmlLineInfo support.

        /// <summary>
        /// Public class that is is used as an annotation on any <see cref="XObject"/> to
        /// associate its line information.
        /// <para>
        /// A similar class is internally defined and used to implement <see cref="System.Xml.IXmlLineInfo"/> on
        /// all <see cref="XObject"/>. This one is publicly assumed and supports <see cref="GetLineColumnString(XObject, string, string)"/>,
        /// <see cref="SetLineColumnInfo{T}(T, int, int)"/>, <see cref="SetLineColumnInfo{T}(T, System.Xml.IXmlLineInfo)"/>, <see cref="SetNoLineColumnInfo{T}(T)"/>
        /// and <see cref="GetLineColumnString(XObject, string, string)"/>.
        /// </para>
        /// </summary>
        public class LineInfoAnnotation : System.Xml.IXmlLineInfo
        {
            class NoInfo : System.Xml.IXmlLineInfo
            {
                public int LineNumber => 0;

                public int LinePosition => 0;

                public bool HasLineInfo() => false;
            }

            /// <summary>
            /// Gets a IXmlLineInfo That has no line/column information (<see cref="System.Xml.IXmlLineInfo.HasLineInfo()"/> is false).
            /// </summary>
            public static readonly System.Xml.IXmlLineInfo None = new NoInfo();

            /// <summary>
            /// Initializes a new information.
            /// </summary>
            /// <param name="lineNumber">The line number.</param>
            /// <param name="linePosition">The position in the line (the column number).</param>
            public LineInfoAnnotation( int lineNumber, int linePosition )
            {
                LineNumber = lineNumber;
                LinePosition = linePosition;
            }

            /// <summary>
            /// The line number.
            /// </summary>
            public int LineNumber { get; }

            /// <summary>
            /// The line position.
            /// </summary>
            public int LinePosition { get; }

            /// <summary>
            /// Always true. Use <see cref="None"/> for an "empty" information.
            /// </summary>
            /// <returns>Always true.</returns>
            public bool HasLineInfo() => true;
        }

        /// <summary>
        /// Sets a line/column information that is the one from another <see cref="System.Xml.IXmlLineInfo"/>
        /// objects (it can be any <see cref="XObject"/> since XObject implements IXmlLineInfo).
        /// </summary>
        /// <typeparam name="T">This element type.</typeparam>
        /// <param name="this">This XObject.</param>
        /// <param name="info">The exisitng line info.</param>
        /// <returns>This object (fluent syntax).</returns>
        public static T SetLineColumnInfo<T>( this T @this, System.Xml.IXmlLineInfo info ) where T : XObject
        {
            @this.RemoveAnnotations<System.Xml.IXmlLineInfo>();
            @this.AddAnnotation( info );
            return @this;
        }

        /// <summary>
        /// Sets a line/column information.
        /// </summary>
        /// <typeparam name="T">This element type.</typeparam>
        /// <param name="this">This XObject.</param>
        /// <param name="line">Line number.</param>
        /// <param name="column">Column number.</param>
        /// <returns>This object (fluent syntax).</returns>
        public static T SetLineColumnInfo<T>( this T @this, int line, int column ) where T : XObject
        {
            @this.RemoveAnnotations<System.Xml.IXmlLineInfo>();
            @this.AddAnnotation( new LineInfoAnnotation( line, column ) );
            return @this;
        }

        /// <summary>
        /// Sets the empty line/column information: <see cref="LineInfoAnnotation.None"/>.
        /// </summary>
        /// <typeparam name="T">This element type.</typeparam>
        /// <param name="this">This XObject.</param>
        /// <returns>This object (fluent syntax).</returns>
        public static T SetNoLineColumnInfo<T>( this T @this ) where T : XObject
        {
            @this.RemoveAnnotations<System.Xml.IXmlLineInfo>();
            @this.AddAnnotation( LineInfoAnnotation.None );
            return @this;
        }


        /// <summary>
        /// Gets a line/column information that may be an empty one (<see cref="System.Xml.IXmlLineInfo.HasLineInfo()"/> is
        /// false) if this information is not known.
        /// </summary>
        /// <typeparam name="T">This element type.</typeparam>
        /// <param name="this">This XObject.</param>
        /// <returns>The associated information.</returns>
        public static System.Xml.IXmlLineInfo GetLineColumnInfo( this XObject @this )
        {
            return @this.Annotation<System.Xml.IXmlLineInfo>() ?? (System.Xml.IXmlLineInfo)@this;
        }

        /// <summary>
        /// Gets line and column information (if it exists) as a string from any <see cref="XObject"/> (such
        /// as <see cref="XAttribute"/> or <see cref="XElement"/>).
        /// </summary>
        /// <param name="this">This <see cref="XObject"/>.</param>
        /// <param name="format">Default format is "- {0},{1}" where {0} is the line and {1} is the column number.</param>
        /// <param name="noLineInformation">Defaults to a null string.</param>
        /// <returns>A string based on <paramref name="format"/> or the <paramref name="noLineInformation"/>.</returns>
        public static string GetLineColumnString( this XObject @this, string format = "- @{0},{1}", string noLineInformation = null )
        {
            return GetLineColumnInfo( @this ).GetLineColumnString( format, noLineInformation );
        }

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

        #endregion



        /// <summary>
        /// Gets the attribute by its name or throws an <see cref="XmlException"/> if it does not exist.
        /// </summary>
        /// <param name="this">This <see cref="XElement"/>.</param>
        /// <param name="name">Name of the attribute.</param>
        static public XAttribute AttributeRequired( this XElement @this, XName name )
        {
            XAttribute a = @this.Attribute( name );
            if( a == null ) throw new XmlException( String.Format( Impl.CoreResources.ExpectedXmlAttribute, name ) + @this.GetLineColumnString() );
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
