using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CK.Core;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization;
using CK.Text;

namespace CK.Core
{
    /// <summary>
    /// Immutable and serializable representation of an exception.
    /// It contains specific data for some exceptions that, based on our experience, are actually interesting.
    /// </summary>
    [Serializable]
    public class CKExceptionData
    {
        readonly string _message;
        readonly string _exceptionTypeName;
        readonly string _exceptionTypeAQName;
        readonly string _stackTrace;
        readonly CKExceptionData _innerException;
        readonly string _fileName;
        readonly string _detailedInfo;
        readonly CKExceptionData[] _loaderExceptions;
        readonly CKExceptionData[] _aggregatedExceptions;
        string _toString;

        /// <summary>
        /// The current stream version.
        /// </summary>
        public static readonly int CurrentStreamVersion = 1;

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
        public CKExceptionData(
            string message,
            string exceptionTypeName,
            string exceptionTypeAssemblyQualifiedName,
            string stackTrace,
            CKExceptionData innerException,
            string fileName,
            string detailedInfo,
            CKExceptionData[] loaderExceptions,
            CKExceptionData[] aggregatedExceptions )
        {
            if( message == null ) throw new ArgumentNullException( nameof( message ) );
            if( String.IsNullOrWhiteSpace( exceptionTypeName ) ) throw new ArgumentNullException( nameof( exceptionTypeName ) );
            if( String.IsNullOrWhiteSpace( exceptionTypeAssemblyQualifiedName ) ) throw new ArgumentNullException( nameof( exceptionTypeAssemblyQualifiedName ) );
            if( aggregatedExceptions != null && aggregatedExceptions.Length == 0 ) throw new ArgumentException( Impl.CoreResources.AggregatedExceptionsMustContainAtLeastOne, nameof( aggregatedExceptions ) );
            if( innerException != null && aggregatedExceptions != null && aggregatedExceptions[0] != innerException ) throw new ArgumentException( Impl.CoreResources.InnerExceptionMustBeTheFirstAggregatedException );
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
        /// Initializes a new <see cref="CKExceptionData"/> from a <see cref="CKBinaryReader"/>. 
        /// See <see cref="Write(CKBinaryWriter,bool)"/>.
        /// </summary>
        /// <param name="r">The reader to read from.</param>
        /// <param name="streamIsCRLF">Whether the strings have CRLF or LF for end-of-lines.</param>
        public CKExceptionData( CKBinaryReader r, bool streamIsCRLF )
            : this( r, streamIsCRLF, r.ReadInt32() )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CKExceptionData"/> from a <see cref="CKBinaryReader"/>
        /// with a known version. 
        /// See <see cref="Write(CKBinaryWriter,bool)"/>.
        /// </summary>
        /// <param name="r">The reader to read from.</param>
        /// <param name="streamIsCRLF">Whether the strings have CRLF or LF for end-of-lines.</param>
        /// <param name="version">Known version.</param>
        public CKExceptionData( CKBinaryReader r, bool streamIsCRLF, int version )
        {
            if( r == null ) throw new ArgumentNullException( "r" );
            _message = r.ReadString( streamIsCRLF );
            _exceptionTypeName = r.ReadString();
            _exceptionTypeAQName = r.ReadString();
            _stackTrace = r.ReadNullableString( streamIsCRLF );
            _fileName = r.ReadNullableString();
            _detailedInfo = r.ReadNullableString( streamIsCRLF );

            int nbAgg = version == 0 ? r.ReadInt32() : r.ReadSmallInt32();
            if( nbAgg > 0 )
            {
                _aggregatedExceptions = new CKExceptionData[nbAgg];
                for( int i = 0; i < nbAgg; ++i ) _aggregatedExceptions[i] = new CKExceptionData( r, streamIsCRLF, version == 0 ? r.ReadInt32() : version );
                _innerException = _aggregatedExceptions[0];
            }
            else
            {
                if( nbAgg == 0 ) _innerException = new CKExceptionData( r, streamIsCRLF, version == 0 ? r.ReadInt32() : version );
            }

            int nbLd = version == 0 ? r.ReadInt32() : r.ReadNonNegativeSmallInt32();
            if( nbLd != 0 )
            {
                _loaderExceptions = new CKExceptionData[nbLd];
                for( int i = 0; i < nbLd; ++i ) _loaderExceptions[i] = new CKExceptionData( r, streamIsCRLF, version == 0 ? r.ReadInt32() : version );
            }
        }

        /// <summary>
        /// Creates a <see cref="CKExceptionData"/> from any <see cref="Exception"/>.
        /// </summary>
        /// <param name="ex">Exception for which data must be created. Can be null: null is returned.</param>
        /// <returns>The data that describes the exception.</returns>
        static public CKExceptionData CreateFrom( Exception ex )
        {
            if( ex == null ) return null;
            CKException ckEx = ex as CKException;
            if( ckEx != null ) return ckEx.EnsureExceptionData();
            Type t = ex.GetType();
            string exceptionTypeName = t.Name;
            string exceptionTypeAssemblyQualifiedName = t.AssemblyQualifiedName;

            CKExceptionData innerException;
            CKExceptionData[] aggregatedExceptions = null;
            var aggEx = ex as AggregateException;
            if( aggEx != null )
            {
                CKExceptionData[] a = new CKExceptionData[aggEx.InnerExceptions.Count];
                for( int i = 0; i < a.Length; ++i ) a[i] = CreateFrom( aggEx.InnerExceptions[i] );
                innerException = a[0];
                aggregatedExceptions = a;
            }
            else innerException = CreateFrom( ex.InnerException );

            string fileName = null;
            string detailedInfo = null;

            CKExceptionData[] loaderExceptions = null;
            var typeLoadEx = ex as ReflectionTypeLoadException;
            if( typeLoadEx != null )
            {
                CKExceptionData[] a = new CKExceptionData[typeLoadEx.LoaderExceptions.Length];
                for( int i = 0; i < a.Length; ++i ) a[i] = CreateFrom( typeLoadEx.LoaderExceptions[i] );
                loaderExceptions = a;
            }
            else
            {
                var fileNFEx = ex as System.IO.FileNotFoundException;
                if( fileNFEx != null )
                {
                    fileName = fileNFEx.FileName;
                    #if NET451 || NET46
                    detailedInfo = fileNFEx.FusionLog.NormalizeEOL();
                    #endif
                }
                else
                {
                    var loadFileEx = ex as System.IO.FileLoadException;
                    if( loadFileEx != null )
                    {
                        fileName = loadFileEx.FileName;
                        #if NET451 || NET46
                        detailedInfo = loadFileEx.FusionLog.NormalizeEOL();
                        #endif
                    }
                    else
                    {
                        #if NET451 || NET46
                        var configEx = ex as System.Configuration.ConfigurationException;
                        if( configEx != null )
                        {
                            fileName = configEx.Filename;
                        }
                        #endif
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
        public string StackTrace => _stackTrace;

        /// <summary>
        /// Gets the inner exception if it exists.
        /// If <see cref="AggregatedExceptions"/> is not null, it is the same as the first aggreated exceptions.
        /// </summary>
        public CKExceptionData InnerException => _innerException;

        /// <summary>
        /// Gets the file name if the exception is referring to a file. 
        /// Null otherwise.
        /// </summary>
        public string FileName => _fileName;

        /// <summary>
        /// Gets more information: this depends on the actual exception type.
        /// For instance, if the exception is a <see cref="System.IO.FileNotFoundException"/> or a <see cref="System.IO.FileLoadException"/> that was raised
        /// while dynamically loading a type or an assembly and we are in DNX, this contains the log from Fusion assembly loading subsystem. 
        /// Null otherwise.
        /// </summary>
        public string DetailedInfo => _detailedInfo;

        /// <summary>
        /// Gets all the the exceptions that occurred while dynamically loading a type or an assembly if the exception is a <see cref="System.Reflection.ReflectionTypeLoadException"/>.
        /// Null otherwise.
        /// </summary>
        public IReadOnlyList<CKExceptionData> LoaderExceptions => _loaderExceptions;

        /// <summary>
        /// Gets all the the aggregated exceptions if the exception is a <see cref="System.AggregateException"/>.
        /// This corresponds to the <see cref="System.AggregateException.InnerExceptions"/> property.
        /// Null if this exception is not a an AggregatedException.
        /// </summary>
        public IReadOnlyList<CKExceptionData> AggregatedExceptions => _aggregatedExceptions;


        /// <summary>
        /// Writes this exception data into a <see cref="BinaryWriter"/>.
        /// </summary>
        /// <param name="w">The writer to use. Can not be null.</param>
        /// <param name="writeVersion">False to not write the <see cref="CurrentStreamVersion"/>.</param>
        public void Write( CKBinaryWriter w, bool writeVersion = true )
        {
            if( writeVersion ) w.Write( CurrentStreamVersion );
            WriteWithoutVersion( w );
        }

        void WriteWithoutVersion( CKBinaryWriter w )
        {
            if( w == null ) throw new ArgumentNullException( "w" );
            w.Write( _message );
            w.Write( _exceptionTypeName );
            w.Write( _exceptionTypeAQName );
            w.WriteNullableString( _stackTrace );
            w.WriteNullableString( _fileName );
            w.WriteNullableString( _detailedInfo );

            if( _aggregatedExceptions != null )
            {
                w.WriteSmallInt32( _aggregatedExceptions.Length );
                foreach( var agg in _aggregatedExceptions ) agg.WriteWithoutVersion( w );
            }
            else
            {
                if( _innerException != null )
                {
                    w.WriteSmallInt32( 0 );
                    _innerException.WriteWithoutVersion( w );
                }
                else w.WriteSmallInt32( -1 );
            }

            if( _loaderExceptions != null )
            {
                w.WriteNonNegativeSmallInt32( _loaderExceptions.Length );
                foreach( var ld in _loaderExceptions ) ld.WriteWithoutVersion( w );
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
            StringWriter sw = w as StringWriter;
            StringBuilder b = sw != null ? sw.GetStringBuilder() : new StringBuilder();
            ToStringBuilder( b, prefix );
            if( sw == null ) w.Write( b.ToString() );
        }

        /// <summary>
        /// Writes the exception data as a readable block of text into a <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="b">The StringBuilder to write to.</param>
        /// <param name="prefix">Prefix that will appear at the start of each line.</param>
        public void ToStringBuilder( StringBuilder b, string prefix )
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
            int lenHeader = _exceptionTypeName.Length + 39 + 28;

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
                .Append( " └─────────────────────────────────────────────────────────────────────────" )
                .AppendLine();
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
                StringBuilder b = new StringBuilder();
                ToStringBuilder( b, string.Empty );
                _toString = b.ToString();
            }
            return _toString;
        }

    }

}
