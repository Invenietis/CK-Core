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
        /// Initializes a new <see cref="CKExceptionData"/> from a <see cref="BinaryReader"/>. 
        /// See <see cref="Write(BinaryWriter)"/>.
        /// </summary>
        /// <param name="r">The reader to read from.</param>
        public CKExceptionData( BinaryReader r )
        {
            if( r == null ) throw new ArgumentNullException( "r" );
            int version = r.ReadInt32();
            _message = r.ReadString();
            _exceptionTypeName = r.ReadString();
            _exceptionTypeAQName = r.ReadString();
            if( r.ReadBoolean() ) _stackTrace = r.ReadString();
            if( r.ReadBoolean() ) _fileName = r.ReadString();
            if( r.ReadBoolean() ) _detailedInfo = r.ReadString();

            int nbAgg = r.ReadInt32();
            if( nbAgg > 0 )
            {
                _aggregatedExceptions = new CKExceptionData[nbAgg];
                for( int i = 0; i < nbAgg; ++i ) _aggregatedExceptions[i] = new CKExceptionData( r );
                _innerException = _aggregatedExceptions[0];
            }
            else
            {
                if( nbAgg == 0 ) _innerException = new CKExceptionData( r );
            }

            int nbLd = r.ReadInt32();
            if( nbLd != 0 )
            {
                _loaderExceptions = new CKExceptionData[nbLd];
                for( int i = 0; i < nbLd; ++i ) _loaderExceptions[i] = new CKExceptionData( r );
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
                    detailedInfo = fileNFEx.FusionLog;
                    #endif
                }
                else
                {
                    var loadFileEx = ex as System.IO.FileLoadException;
                    if( loadFileEx != null )
                    {
                        fileName = loadFileEx.FileName;
                        #if NET451 || NET46
                        detailedInfo = loadFileEx.FusionLog;
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
        public string StackTrace { get { return _stackTrace; } }

        /// <summary>
        /// Gets the inner exception if it exists.
        /// If <see cref="AggregatedExceptions"/> is not null, it is the same as the first aggreated exceptions.
        /// </summary>
        public CKExceptionData InnerException { get { return _innerException; } }

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
        public void Write( BinaryWriter w )
        {
            if( w == null ) throw new ArgumentNullException( "w" );
            w.Write( 0 );
            w.Write( _message );
            w.Write( _exceptionTypeName );
            w.Write( _exceptionTypeAQName );
            WriteNullableString( w, _stackTrace );
            WriteNullableString( w, _fileName );
            WriteNullableString( w, _detailedInfo );

            if( _aggregatedExceptions != null )
            {
                w.Write( _aggregatedExceptions.Length );
                foreach( var agg in _aggregatedExceptions ) agg.Write( w );
            }
            else
            {
                if( _innerException != null )
                {
                    w.Write( 0 );
                    _innerException.Write( w );
                }
                else w.Write( -1 );
            }

            if( _loaderExceptions != null )
            {
                w.Write( _loaderExceptions.Length );
                foreach( var ld in _loaderExceptions ) ld.Write( w );
            }
            else w.Write( 0 );
        }

        static void WriteNullableString( BinaryWriter w, string s )
        {
            if( s != null )
            {
                w.Write( true );
                w.Write( s );
            }
            else w.Write( false );
        }

        /// <summary>
        /// Writes the exception data as a readable block of text into a <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="w">The TextWriter to write to.</param>
        /// <param name="prefix">Prefix that will appear at the start of each line.</param>
        /// <param name="newLine">Defaults to <see cref="Environment.NewLine"/>.</param>
        public void ToTextWriter( TextWriter w, string prefix, string newLine = null )
        {
            WriteText( w.Write, prefix, newLine );
        }

        /// <summary>
        /// Writes the exception data as a readable block of text into a <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="b">The StringBuilder to write to.</param>
        /// <param name="prefix">Prefix that will appear at the start of each line.</param>
        /// <param name="newLine">Defaults to <see cref="Environment.NewLine"/>.</param>
        public void ToStringBuilder( StringBuilder b, string prefix, string newLine = null )
        {
            WriteText( t => b.Append( t ), prefix, newLine );
        }

        /// <summary>
        /// Core function that writes the exception data as a readable block of text.
        /// </summary>
        /// <param name="appender">The function that collects the text fragments.</param>
        /// <param name="prefix">Prefix that will appear at the start of each line.</param>
        /// <param name="newLine">Defaults to <see cref="Environment.NewLine"/>.</param>
        public void WriteText( Action<string> appender, string prefix, string newLine = null )
        {
            if( appender == null ) throw new ArgumentNullException( "appender" );
            if( prefix == null ) prefix = String.Empty;
            if( newLine == null ) newLine = Environment.NewLine;
            if( prefix.Length == 0 && _toString != null && newLine == Environment.NewLine )
            {
                appender( ToString() );
                return;
            }

            appender( prefix );
            appender( " ┌──────────────────────────■ Exception: " );
            appender( _exceptionTypeName );
            appender( " ■──────────────────────────" );
            appender( newLine );
            Debug.Assert( ("──────────────────────────■ Exception: " + " ■──────────────────────────").Length == 39 + 28 );
            int lenHeader = _exceptionTypeName.Length + 39 + 28;

            string locPrefix = prefix + " | ";

            AppendText( appender, locPrefix, "Message", _message, newLine );

            if( _stackTrace != null ) AppendText( appender, locPrefix, "Stack", _stackTrace, newLine );
            
            if( !String.IsNullOrEmpty( _fileName ) ) AppendLine( appender, locPrefix, "FileName", _fileName, newLine );
            if( _detailedInfo != null ) AppendText( appender, locPrefix, "FusionLog", _detailedInfo, newLine );

            if( _loaderExceptions != null )
            {
                appender( locPrefix );
                appender( " ┌──────────────────────────■ [Loader Exceptions] ■──────────────────────────" );
                appender( newLine );
                foreach( var item in _loaderExceptions )
                {
                    item.WriteText( appender, locPrefix + " | ", newLine );
                }
                appender( locPrefix  );
                appender( " └─────────────────────────────────────────────────────────────────────────" );
                appender( newLine );
            }
            // The InnerException of an aggregated exception is the same as the first of it InnerExceptionS.
            // (The InnerExceptionS are the contained/aggregated exceptions of the AggregatedException object.)
            // This is why, if we are on an AggregatedException we do not follow its InnerException.
            if( _aggregatedExceptions != null )
            {
                appender( locPrefix );
                appender( " ┌──────────────────────────■ [Aggregated Exceptions] ■──────────────────────────" );
                appender( newLine );
                foreach( var item in _aggregatedExceptions )
                {
                    item.WriteText( appender, locPrefix + " | ", newLine );
                }
                appender( locPrefix );
                appender( " └─────────────────────────────────────────────────────────────────────────" );
                appender( newLine );
            }
            else if( _innerException != null )
            {
                appender( locPrefix );
                appender( " ┌──────────────────────────■ [Inner Exception] ■──────────────────────────" );
                appender( newLine );
                _innerException.WriteText( appender, locPrefix + " | ", newLine );
                appender( locPrefix );
                appender( " └─────────────────────────────────────────────────────────────────────────" );
                appender( newLine );
            }
            appender( prefix );
            appender( " └─────────────────────────────────────────────────────────────────────────" );
            appender( newLine );
        }

        void AppendText( Action<string> appender, string prefix, string label, string text, string newLine )
        {
            appender( prefix );
            appender( label );
            appender( ": " );
            appender( "         " );
            appender( text.Replace( newLine, newLine + prefix + "         " ) );
            appender( newLine );
        }

        void AppendLine( Action<string> appender, string prefix, string label, string line, string newLine )
        {
            appender( prefix );
            appender( label );
            appender( ": " );
            appender( line );
            appender( newLine );
        }

        /// <summary>
        /// Overridden to return the result of <see cref="WriteText"/> without prefix and a standard <see cref="Environment.NewLine"/>.
        /// This is cached once built.
        /// </summary>
        /// <returns>This exception data as a block of readable text.</returns>
        public override string ToString()
        {
            if( _toString == null )
            {
                StringBuilder b = new StringBuilder();
                ToStringBuilder( b, String.Empty, Environment.NewLine );
                _toString = b.ToString();
            }
            return _toString;
        }

    }

}
