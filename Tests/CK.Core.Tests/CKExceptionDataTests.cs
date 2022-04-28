using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using FluentAssertions;
using System.Diagnostics;

namespace CK.Core.Tests
{

    public class CKExceptionDataTests
    {

        [Test]
        public void FromSimplestException()
        {
            CheckSimpleExceptionData( CKExceptionData.CreateFrom( new Exception( "" ) ), s => s == "", false, false );

            var simpleData = CKExceptionData.CreateFrom( ThrowSimpleException( "Test" ) );
            CheckSimpleExceptionData( simpleData, s => s == "Test", false, true );
        }

        static void CheckSimpleExceptionData( CKExceptionData? simpleData, Func<string, bool> message, bool? hasInner = null, bool hasStack = true )
        {
            Debug.Assert( simpleData != null );
            message( simpleData.Message ).Should().BeTrue( "Invalid message." );
            simpleData.ExceptionTypeName.Should().Be( "Exception" );
            simpleData.ExceptionTypeAssemblyQualifiedName.Should().Be( typeof( Exception ).AssemblyQualifiedName );

            if( hasStack )
                simpleData.StackTrace.Should().NotBeNull( "Stack trace is not null when the exception has actually been thrown." );
            else simpleData.StackTrace.Should().BeNull();

            if( hasInner.HasValue )
            {
                if( hasInner.Value ) simpleData.InnerException.Should().NotBeNull();
                else simpleData.InnerException.Should().BeNull();
            }
            simpleData.AggregatedExceptions.Should().BeNull();
            simpleData.LoaderExceptions.Should().BeNull();
        }

        [Test]
        public void WithInnerExceptions()
        {
            Exception e = ThrowExceptionWithInner();
            var d = CKExceptionData.CreateFrom( e );
            CheckSimpleExceptionData( d, s => s == "Outer", true );
            CheckSimpleExceptionData( d.InnerException, s => s == "Inner", false );
            var backToEx = new CKException( d );
            var backToData = CKExceptionData.CreateFrom( backToEx );
            CheckSimpleExceptionData( backToData, s => s == "Outer", true );
            CheckSimpleExceptionData( backToData.InnerException, s => s == "Inner", false );
        }

        [Test]
        public void AggregatedExceptions()
        {
            AggregateException eAgg = ThrowAggregatedException();
            var d = CKExceptionData.CreateFrom( eAgg );

            d.ExceptionTypeAssemblyQualifiedName.Should().Be( typeof( AggregateException ).AssemblyQualifiedName );
            d.ExceptionTypeName.Should().Be( typeof( AggregateException ).Name );
            Debug.Assert( d.AggregatedExceptions != null );
            d.AggregatedExceptions.Count.Should().BeGreaterOrEqualTo( 1 );
            d.InnerException.Should().BeSameAs( d.AggregatedExceptions[0] );
            for( int i = 0; i < d.AggregatedExceptions.Count; ++i )
            {
                CheckSimpleExceptionData( d.AggregatedExceptions[i], s => s.StartsWith( "Ex n°" ) );
            }
        }

        [Test]
        public void CKExceptionData_is_both_Simple_and_Versioned_serializable()
        {
            var dataE0 = CKExceptionData.CreateFrom( ThrowAggregatedException() );
            var dataE1 = CKExceptionData.CreateFrom( ThrowSimpleException( "Test Message" ) );
            var dataE2 = CKExceptionData.CreateFrom( ThrowLoaderException() );
            var dataE3 = CKExceptionData.CreateFrom( ThrowExceptionWithInner() );
            var dataE4 = CKExceptionData.CreateFrom( ThrowTwoInnerExceptions() );
            SerializationVersionAttribute.GetRequiredVersion( typeof( CKExceptionData ) ).Should().Be( 1 );
            using( var mem = Util.RecyclableStreamManager.GetStream() )
            {
                CKBinaryWriter w = new CKBinaryWriter( mem );
                dataE0.Write( w );
                dataE1.WriteData( w );
                dataE2.Write( w );
                dataE3.WriteData( w );
                dataE4.Write( w );
                mem.Position = 0;
                var r = new CKBinaryReader( mem );
                var data0 = new CKExceptionData( r );
                data0.ToString().Should().Be( dataE0.ToString() );
                var data1 = new CKExceptionData( r, 1 );
                data1.ToString().Should().Be( dataE1.ToString() );
                var data2 = new CKExceptionData( r );
                data2.ToString().Should().Be( dataE2.ToString() );
                var data3 = new CKExceptionData( r, 1 );
                data3.ToString().Should().Be( dataE3.ToString() );
                var data4 = new CKExceptionData( r );
                data4.ToString().Should().Be( dataE4.ToString() );
            }
        }

        static AggregateException ThrowAggregatedException()
        {
            AggregateException? eAgg = null;
            try
            {
                Parallel.For( 0, 50, i =>
                {
                    System.Threading.Thread.Sleep( 10 );
                    if( i % 2 == 0 ) throw new Exception( String.Format( "Ex n°{0}", i ), ThrowExceptionWithInner() );
                    else throw new Exception( String.Format( "Ex n°{0}", i ) );
                } );
            }
            catch( AggregateException ex )
            {
                eAgg = ex;
            }
            return eAgg!;
        }

        static Exception ThrowExceptionWithInner( bool loaderException = false )
        {
            Exception e;
            try { throw new Exception( "Outer", loaderException ? ThrowLoaderException() : ThrowSimpleException( "Inner" ) ); }
            catch( Exception ex ) { e = ex; }
            return e;
        }

        static Exception ThrowTwoInnerExceptions()
        {
            Exception ckEx;
            try { throw new Exception( "CK-MostOuter", ThrowExceptionWithInner( true ) ); }
            catch( Exception ex ) { ckEx = ex; }
            return ckEx;
        }

        static Exception ThrowSimpleException( string message )
        {
            Exception e;
            try { throw new Exception( message ); }
            catch( Exception ex ) { e = ex; }
            return e;
        }

        static Exception ThrowLoaderException()
        {
            Exception? e = null;
            try { Type.GetType( "A.Type, An.Unexisting.Assembly", true ); }
            catch( Exception ex ) { e = ex; }
            return e!;
        }

    }
}
