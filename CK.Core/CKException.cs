using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace CK.Core
{
    /// <summary>
    /// Basic <see cref="Exception"/> that eases message formatting thanks to its constructors
    /// and provides an Exception wrapper around <see cref="CKExceptionData"/>.
    /// </summary>
    public class CKException : Exception
    {
        CKExceptionData? _exceptionData;

        /// <summary>
        /// Initializes a new <see cref="CKException"/>.
        /// </summary>
        /// <param name="message">Simple message.</param>
        public CKException( string message )
            : base( message )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CKException"/>.
        /// </summary>
        /// <param name="message">Simple message.</param>
        /// <param name="innerException">Exception that caused this one.</param>
        public CKException( string message, Exception? innerException )
            : base( message, innerException )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CKException"/>.
        /// </summary>
        /// <param name="messageFormat">Format string with optional placeholders.</param>
        /// <param name="args">Varying number of arguments to format.</param>
        public CKException( string messageFormat, params object[] args )
            : this( String.Format( messageFormat, args ) )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CKException"/> with an <see cref="Exception.InnerException"/>.
        /// </summary>
        /// <param name="innerException">Exception that caused this one.</param>
        /// <param name="messageFormat">Format string with optional placeholders.</param>
        /// <param name="args">Varying number of arguments to format.</param>
        public CKException( Exception innerException, string messageFormat, params object[] args )
            : this( String.Format( messageFormat, args ), innerException )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CKException"/> with an <see cref="ExceptionData"/>.
        /// The message of this exception is the <see cref="CKExceptionData.Message"/>.
        /// Use the static <see cref="CreateFrom"/> to handle null data (a null CKException will be returned).
        /// </summary>
        /// <param name="data">The exception data. Must not be null.</param>
        public CKException( CKExceptionData data )
            : this( data.Message )
        {
            _exceptionData = data;
        }

        /// <summary>
        /// Creates a <see cref="CKException"/> from a <see cref="CKExceptionData"/>. This method returns null when data is null.
        /// This is the symmetric of <see cref="CKExceptionData.CreateFrom"/>.
        /// </summary>
        /// <param name="data">Data of an exception for which a <see cref="CKException"/> wrapper must be created. Can be null: null is returned.</param>
        /// <returns>The exception that wraps the data.</returns>
        static public CKException? CreateFrom( CKExceptionData? data )
        {
            if( data == null ) return null;
            return new CKException( data );
        }
        
        /// <summary>
        /// Gets the <see cref="CKExceptionData"/> if it exists: use <see cref="EnsureExceptionData"/> to 
        /// create if this is null, a data that describes this exception.
        /// </summary>
        public CKExceptionData? ExceptionData => _exceptionData;

        /// <summary>
        /// If <see cref="ExceptionData"/> is null, this method creates the <see cref="CKExceptionData"/>
        /// with the details from this exception.
        /// </summary>
        /// <returns>The <see cref="CKExceptionData"/> that describes this exception.</returns>
        public CKExceptionData EnsureExceptionData()
        {
            if( _exceptionData == null )
            {
                var inner = CKExceptionData.CreateFrom( InnerException );
                _exceptionData = new CKExceptionData( Message, "CKException", GetType().AssemblyQualifiedName!, StackTrace, inner, null, null, null, null );
            }
            return _exceptionData; 
        }

    }
}
