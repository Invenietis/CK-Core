using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using System.IO;
using System.Xml;
using System.Diagnostics;

namespace CK.Storage
{
    /// <summary>
    /// Factory for <see cref="IStructuredReader"/> implementation.
    /// </summary>
    public static class SimpleStructuredReader
    {

        /// <summary>
        /// Creates a simple (full xml based) <see cref="IStructuredReader"/> instance.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to use.</param>
        /// <returns>A reader bound to the <paramref name="stream"/>.</returns>
        static public IStructuredReader CreateReader( Stream stream, IServiceProvider serviceProvider )
        {
            return CreateReader( stream, serviceProvider, true );
        }

        /// <summary>
        /// Creates a simple (full xml based) <see cref="IStructuredReader"/> instance.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to use.</param>
        /// <param name="throwErrorOnMissingFile">True to throw an exception when the <paramref name="stream"/> parameter is null.</param>
        /// <returns>A reader bound to the <paramref name="stream"/> or null.</returns>
        static public IStructuredReader CreateReader( Stream stream, IServiceProvider serviceProvider, bool throwErrorOnMissingFile )
        {
            MissingDisposeCallSentinel.DebugCheckMissing( s => Debug.Fail( s ) );

            if( stream == null )
            {
                if( throwErrorOnMissingFile )
                    throw new CKException( R.FileNotFound );
                else
                    return null;
            }
            XmlReader r = null;
            try
            {
                r = XmlReader.Create( stream, new XmlReaderSettings()
                {
                    IgnoreComments = true,
                    IgnoreProcessingInstructions = true,
                    IgnoreWhitespace = true,
                    ProhibitDtd = true,
                    ValidationType = ValidationType.None
                } );
            }
            catch( Exception ex )
            {
                if( r != null ) r.Close();
                throw new CKException( R.InvalidFileManifest, ex );
            }
            ReaderImpl rw = new ReaderImpl( r, serviceProvider, true );
            if( rw.StorageVersion == null )
            {
                rw.Dispose();
                throw new CKException( R.InvalidFileManifestVersion );
            }
            return rw;
        }


    }
}
