using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace CK.Core
{
    /// <summary>
    /// Immutable and serializable representation of an exception.
    /// It contains specific data for some exceptions that, based on our experience, are actually interesting.
    /// </summary>
    [SerializationVersion(1)]
    public sealed class CKExceptionData : ICKVersionedBinarySerializable, ICKSimpleBinarySerializable
    {
        readonly string _message;
        readonly string _exceptionTypeName;
        readonly string _exceptionTypeAQName;
        readonly string? _stackTrace;
        readonly CKExceptionData? _innerException;
        readonly string? _fileName;
        readonly string? _detailedInfo;
        readonly CKExceptionData[]? _loaderExceptions;
        readonly CKExceptionData[]? _aggregatedExceptions;
        string? _toString;

        /// <summary>
        /// Initializes a new <see cref="CKExceptionData"/> with all its fields.
        /// Use the factory method <see cref="CreateFrom"/> to create a data from any exception.
        /// </summary>
        /// <param name="message">Message of the exception. Must not be null.</param>
        /// <param name="exceptionTypeName">Type name of the exception (no namespace nor assembly). Must not be null nor empty..</param>
        /// <param name="exceptionTypeAssemblyQualifiedName">Full type name of the exception. Must not be null nor empty.</param>
        /// <param name="stackTrace">Stack trace. Can be null.</param>
        /// <param name="innerException">Inner exception. If <paramref name="aggregatedExceptions"/> is not null, it must be the same as the first aggregated exceptions.</param>
        /// <param name="fileName">File name related to the exception (if it makes sense). Can be null.</param>
        /// <param name="detailedInfo">More detailed information if any.</param>
        /// <param name="loaderExceptions">Loader exceptions. <see cref="LoaderExceptions"/>.</param>
        /// <param name="aggregatedExceptions">Aggregated exceptions can be null. Otherwise, it must contain at least one exception.</param>
        public CKExceptionData( string message,
                                string exceptionTypeName,
                                string exceptionTypeAssemblyQualifiedName,
                                string? stackTrace,
                                CKExceptionData? innerException,
                                string? fileName,
                                string? detailedInfo,
                                CKExceptionData[]? loaderExceptions,
                                CKExceptionData[]? aggregatedExceptions )
        {
            Throw.CheckNotNullArgument( message );
            Throw.CheckNotNullOrEmptyArgument( exceptionTypeName );
            if( aggregatedExceptions != null && aggregatedExceptions.Length == 0 )
            {
                Throw.ArgumentException( nameof( aggregatedExceptions ), Impl.CoreResources.AggregatedExceptionsMustContainAtLeastOne );
            }
            if( innerException != null && aggregatedExceptions != null && aggregatedExceptions[0] != innerException )
            {
                Throw.ArgumentException( nameof( aggregatedExceptions ), Impl.CoreResources.InnerExceptionMustBeTheFirstAggregatedException );
            }
            // No empty array for loaderExceptions: null or at least one inside.
            if( loaderExceptions != null && loaderExceptions.Length == 0 ) loaderExceptions = null;
            _message = message;
            _exceptionTypeName = exceptionTypeName;
            _exceptionTypeAQName = exceptionTypeAssemblyQualifiedName;
            _stackTrace = String.IsNullOrWhiteSpace( stackTrace ) ? null : stackTrace;
            _innerException = innerException;
            _fileName = fileName;
            _detailedInfo = detailedInfo;
            _loaderExceptions = loaderExceptions;
            _aggregatedExceptions = aggregatedExceptions;
        }

        /// <summary>
        /// Initializes a new <see cref="CKExceptionData"/> from a <see cref="ICKBinaryReader"/>. 
        /// See <see cref="Write(ICKBinaryWriter)"/>.
        /// </summary>
        /// <param name="r">The reader to read from.</param>
        public CKExceptionData( CKBinaryReader r )
            : this( r, r.ReadInt32() )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CKExceptionData"/> from a <see cref="ICKBinaryReader"/>
        /// with a known version. 
        /// See <see cref="WriteData(ICKBinaryWriter)"/>.
        /// </summary>
        /// <param name="r">The reader to read from.</param>
        /// <param name="version">Known version.</param>
        public CKExceptionData( ICKBinaryReader r, int version )
        {
            Throw.CheckNotNullArgument( r );
            Throw.CheckOutOfRangeArgument( version >= 0 && version <= 1 );
            _message = r.ReadString();
            _exceptionTypeName = r.ReadString();
            _exceptionTypeAQName = r.ReadString();
            _stackTrace = r.ReadNullableString();
            _fileName = r.ReadNullableString();
            _detailedInfo = r.ReadNullableString();

            int nbAgg = version == 0 ? r.ReadInt32() : r.ReadSmallInt32();
            if( nbAgg > 0 )
            {
                _aggregatedExceptions = new CKExceptionData[nbAgg];
                for( int i = 0; i < nbAgg; ++i ) _aggregatedExceptions[i] = new CKExceptionData( r, version == 0 ? r.ReadInt32() : version );
                _innerException = _aggregatedExceptions[0];
            }
            else
            {
                if( nbAgg == 0 ) _innerException = new CKExceptionData( r, version == 0 ? r.ReadInt32() : version );
            }

            int nbLd = version == 0 ? r.ReadInt32() : r.ReadNonNegativeSmallInt32();
            if( nbLd != 0 )
            {
                _loaderExceptions = new CKExceptionData[nbLd];
                for( int i = 0; i < nbLd; ++i ) _loaderExceptions[i] = new CKExceptionData( r, version == 0 ? r.ReadInt32() : version );
            }
        }


        /// <summary>
        /// Creates a new "fake" <see cref="CKExceptionData"/> that can be used whenever no actual exception is available.
        /// </summary>
        /// <param name="message">Message of the exception. Must not be null.</param>
        /// <param name="exceptionTypeName">Type name of the exception (no namespace nor assembly). Must not be null nor empty..</param>
        /// <param name="exceptionTypeAssemblyQualifiedName">Full type name of the exception. Must not be null nor empty.</param>
        /// <returns>The data that describes the exception.</returns>
        static public CKExceptionData Create( string message, string exceptionTypeName = "Not.An.Exception", string exceptionTypeAssemblyQualifiedName = "Not.An.Exception, No.Assembly" )
        {
            return new CKExceptionData( message, exceptionTypeName, exceptionTypeAssemblyQualifiedName, null, null, null, null, null, null );
        }

        /// <summary>
        /// Creates a <see cref="CKExceptionData"/> from any <see cref="Exception"/>.
        /// </summary>
        /// <param name="ex">Exception for which data must be created. Can be null: null is returned.</param>
        /// <returns>The data that describes the exception.</returns>
        [return: NotNullIfNotNull( "ex" )]
        static public CKExceptionData? CreateFrom( Exception? ex )
        {
            if( ex == null ) return null;
            if( ex is CKException ckEx ) return ckEx.EnsureExceptionData();
            Type t = ex.GetType();
            string exceptionTypeName = t.Name;
            string exceptionTypeAssemblyQualifiedName = t.AssemblyQualifiedName!;

            CKExceptionData? innerException;
            CKExceptionData[]? aggregatedExceptions = null;
            if( ex is AggregateException aggEx )
            {
                CKExceptionData[] a = new CKExceptionData[aggEx.InnerExceptions.Count];
                for( int i = 0; i < a.Length; ++i ) a[i] = CreateFrom( aggEx.InnerExceptions[i] )!;
                innerException = a[0];
                aggregatedExceptions = a;
            }
            else innerException = CreateFrom( ex.InnerException );

            string? fileName = null;
            string? detailedInfo = null;

            CKExceptionData[]? loaderExceptions = null;
            if( ex is ReflectionTypeLoadException typeLoadEx && typeLoadEx.LoaderExceptions != null )
            {
                CKExceptionData[] a = new CKExceptionData[typeLoadEx.LoaderExceptions.Length];
                for( int i = 0; i < a.Length; ++i ) a[i] = CreateFrom( typeLoadEx.LoaderExceptions[i] )!;
                loaderExceptions = a;
            }
            else
            {
                if( ex is FileNotFoundException fileNFEx )
                {
                    fileName = fileNFEx.FileName;
                    detailedInfo = fileNFEx.FusionLog?.ReplaceLineEndings();
                }
                else
                {
                    if( ex is FileLoadException loadFileEx )
                    {
                        fileName = loadFileEx.FileName;
                        detailedInfo = loadFileEx.FusionLog?.ReplaceLineEndings();
                    }
                }
            }
            return new CKExceptionData( ex.Message, exceptionTypeName, exceptionTypeAssemblyQualifiedName, ex.StackTrace, innerException, fileName, detailedInfo, loaderExceptions, aggregatedExceptions );
        }

        /// <summary>
        /// Gets the message of the exception. Never null but can be empty.
        /// </summary>
        public string Message => _message; 

        /// <summary>
        /// Gets the assembly qualified exception type name. Never null nor empty.
        /// </summary>
        public string ExceptionTypeAssemblyQualifiedName => _exceptionTypeAQName;

        /// <summary>
        /// Gets the exception type name. Never null nor empty.
        /// </summary>
        public string ExceptionTypeName => _exceptionTypeName;

        /// <summary>
        /// Gets the stack trace. Can be null.
        /// </summary>
        public string? StackTrace => _stackTrace;

        /// <summary>
        /// Gets the inner exception if it exists.
        /// If <see cref="AggregatedExceptions"/> is not null, it is the same as the first aggregated exceptions.
        /// </summary>
        public CKExceptionData? InnerException => _innerException;

        /// <summary>
        /// Gets the file name if the exception is referring to a file. 
        /// Null otherwise.
        /// </summary>
        public string? FileName => _fileName;

        /// <summary>
        /// Gets more information: this depends on the actual exception type.
        /// For instance, if the exception is a <see cref="System.IO.FileNotFoundException"/> or a <see cref="System.IO.FileLoadException"/> that was raised
        /// while dynamically loading a type or an assembly and we are in DNX, this contains the log from Fusion assembly loading subsystem. 
        /// Null otherwise.
        /// </summary>
        public string? DetailedInfo => _detailedInfo;

        /// <summary>
        /// Gets all the exceptions that occurred while dynamically loading a type or an assembly if the exception is a <see cref="System.Reflection.ReflectionTypeLoadException"/>.
        /// Null otherwise.
        /// </summary>
        public IReadOnlyList<CKExceptionData>? LoaderExceptions => _loaderExceptions;

        /// <summary>
        /// Gets all the aggregated exceptions if the exception is a <see cref="System.AggregateException"/>.
        /// This corresponds to the <see cref="System.AggregateException.InnerExceptions"/> property.
        /// Null if this exception is not a an AggregatedException.
        /// </summary>
        public IReadOnlyList<CKExceptionData>? AggregatedExceptions => _aggregatedExceptions;


        /// <summary>
        /// Writes this exception data into a <see cref="ICKBinaryWriter"/>, embedding the
        /// current version.
        /// </summary>
        /// <param name="w">The writer to use.</param>
        public void Write( ICKBinaryWriter w )
        {
            Debug.Assert( SerializationVersionAttribute.GetRequiredVersion( GetType() ) == 1 );
            w.Write( 1 );
            WriteData( w );
        }

        /// <summary>
        /// Writes this exception data into a <see cref="ICKBinaryWriter"/>, without the
        /// current version.
        /// </summary>
        /// <param name="w">The writer to use.</param>
        public void WriteData( ICKBinaryWriter w )
        {
            Throw.CheckNotNullArgument( w );
            w.Write( _message );
            w.Write( _exceptionTypeName );
            w.Write( _exceptionTypeAQName );
            w.WriteNullableString( _stackTrace );
            w.WriteNullableString( _fileName );
            w.WriteNullableString( _detailedInfo );

            if( _aggregatedExceptions != null )
            {
                w.WriteSmallInt32( _aggregatedExceptions.Length );
                foreach( var agg in _aggregatedExceptions ) agg.WriteData( w );
            }
            else
            {
                if( _innerException != null )
                {
                    w.WriteSmallInt32( 0 );
                    _innerException.WriteData( w );
                }
                else w.WriteSmallInt32( -1 );
            }

            if( _loaderExceptions != null )
            {
                w.WriteNonNegativeSmallInt32( _loaderExceptions.Length );
                foreach( var ld in _loaderExceptions ) ld.WriteData( w );
            }
            else w.WriteNonNegativeSmallInt32( 0 );
        }


        /// <summary>
        /// Writes the exception data as a readable block of text into a <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="w">The TextWriter to write to.</param>
        /// <param name="prefix">Prefix that will appear at the start of each line.</param>
        public void ToTextWriter( TextWriter w, string prefix )
        {
            StringWriter? sw = w as StringWriter;
            StringBuilder b = sw != null ? sw.GetStringBuilder() : new StringBuilder();
            ToStringBuilder( b, prefix );
            if( sw == null ) w.Write( b.ToString() );
        }

        /// <summary>
        /// Writes the exception data as a readable block of text into a <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="b">The StringBuilder to write to.</param>
        /// <param name="prefix">Prefix that will appear at the start of each line.</param>
        /// <param name="endWithNewLine">Whether a new line is appended to the end of the text or not.</param>
        public void ToStringBuilder( StringBuilder b, string prefix, bool endWithNewLine = true )
        {
            if( prefix == null ) prefix = string.Empty;
            if( prefix.Length == 0 && _toString != null )
            {
                b.Append( _toString );
                return;
            }

            b.Append( prefix );
            b.Append( " ┌──────────────────────────■ Exception: " );
            b.Append( _exceptionTypeName );
            b.Append( " ■──────────────────────────" );
            b.AppendLine();
            Debug.Assert( ("──────────────────────────■ Exception: " + " ■──────────────────────────").Length == 39 + 28 );

            string locPrefix = prefix + " | ";

            AppendField( b, locPrefix, "Message", _message );

            if( _stackTrace != null ) AppendField( b, locPrefix, "Stack", _stackTrace );
            
            if( !string.IsNullOrEmpty( _fileName ) ) AppendField( b, locPrefix, "FileName", _fileName );

            if( _detailedInfo != null ) AppendField( b, locPrefix, "Details", _detailedInfo );

            if( _loaderExceptions != null )
            {
                b.Append( locPrefix )
                    .Append( " ┌──────────────────────────■ [Loader Exceptions] ■──────────────────────────" )
                    .AppendLine();
                foreach( var item in _loaderExceptions )
                {
                    item.ToStringBuilder( b, locPrefix + " | " );
                }
                b.Append( locPrefix )
                    .Append( " └─────────────────────────────────────────────────────────────────────────" )
                    .AppendLine();
            }
            // The InnerException of an aggregated exception is the same as the first of it InnerExceptionS.
            // (The InnerExceptionS are the contained/aggregated exceptions of the AggregatedException object.)
            // This is why, if we are on an AggregatedException we do not follow its InnerException.
            if( _aggregatedExceptions != null )
            {
                b.Append( locPrefix )
                    .Append( " ┌──────────────────────────■ [Aggregated Exceptions] ■──────────────────────────" )
                    .AppendLine();
                foreach( var item in _aggregatedExceptions )
                {
                    item.ToStringBuilder( b, locPrefix + " | " );
                }
                b.Append( locPrefix )
                    .Append( " └─────────────────────────────────────────────────────────────────────────" )
                    .AppendLine();
            }
            else if( _innerException != null )
            {
                b.Append( locPrefix )
                    .Append( " ┌──────────────────────────■ [Inner Exception] ■──────────────────────────" )
                    .AppendLine();
                _innerException.ToStringBuilder( b, locPrefix + " | " );
                b.Append( locPrefix )
                    .Append( " └─────────────────────────────────────────────────────────────────────────" )
                    .AppendLine();
            }
            b.Append( prefix )
                .Append( " └─────────────────────────────────────────────────────────────────────────" );
            if( endWithNewLine )
            {
                b.AppendLine();
            }
        }

        static StringBuilder AppendField( StringBuilder b, string prefix, string label, string text )
        {
            b.Append( prefix ).Append( label ).Append( ": " ).Append( ' ', 10 - label.Length );
            prefix += new string( ' ', 12 );
            b.AppendMultiLine( prefix, text, false ).AppendLine();
            return b;
        }

        /// <summary>
        /// Overridden to return a detailed text. This string is cached once built.
        /// </summary>
        /// <returns>This exception data as a readable text.</returns>
        public override string ToString()
        {
            if( _toString == null )
            {
                StringBuilder b = new();
                ToStringBuilder( b, string.Empty );
                _toString = b.ToString();
            }
            return _toString;
        }

    }

}
