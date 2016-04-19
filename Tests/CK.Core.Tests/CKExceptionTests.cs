using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if NET451 || NET46
using System.Runtime.Serialization.Formatters.Binary;
#endif
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CK.Core.Tests
{
    [TestFixture]
    public class CKExceptionTests
    {

        [Test]
        public void FromSimplestException()
        {
            CheckSimpleExceptionData( CKExceptionData.CreateFrom( new Exception( "" ) ), s => s == "", false, false );

            var simpleData = CKExceptionData.CreateFrom( ThrowSimpleException( "Test" ) );
            CheckSimpleExceptionData( simpleData, s => s == "Test", false, true );
        }

        private static void CheckSimpleExceptionData( CKExceptionData simpleData, Func<string,bool> message, bool? hasInner = null, bool hasStack = true )
        {
            Assert.That( message(simpleData.Message), "Invalid message." );
            Assert.That( simpleData.ExceptionTypeName, Is.EqualTo( "Exception" ) );
            Assert.That( simpleData.ExceptionTypeAssemblyQualifiedName, Is.EqualTo( typeof( Exception ).AssemblyQualifiedName ) );
            
            if( hasStack ) 
                Assert.That( simpleData.StackTrace, Is.Not.Null, "Stack trace is not null when the exception has actually been thrown." );
            else Assert.That( simpleData.StackTrace, Is.Null );

            if( hasInner.HasValue )
            {
                if( hasInner.Value ) Assert.That( simpleData.InnerException, Is.Not.Null );
                else Assert.That( simpleData.InnerException, Is.Null );
            }
            Assert.That( simpleData.AggregatedExceptions, Is.Null );
            Assert.That( simpleData.LoaderExceptions, Is.Null );
        }

        [Test]
        public void WithInnerExceptions()
        {
            Exception e = ThrowExceptionWithInner();
            var d = CKExceptionData.CreateFrom( e );
            CheckSimpleExceptionData( d, s => s == "Outer", true );
            CheckSimpleExceptionData( d.InnerException, s => s == "Inner", false );
        }

        //#if DNXCORE50
        //        [Test]
        //        public void TestTestTestTestTestTestTestDNXCORE50()
        //        {
        //            Assert.That( false, "Stupid test fail in DNXCORE50" );
        //        }
        //#endif

        //#if DNX46
        //        [Test]
        //        public void TestTestTestTestTestTestTestDNX46()
        //        {
        //            Assert.That( false, "Stupid test fail in DNX46" );
        //        }
        //#endif

        //#if RELEASE
        //        [Test]
        //        public void TestTestTestTestTestTestTestRELEASE()
        //        {
        //            Assert.That( false, "Stupid test fail in RELEASE" );
        //        }
        //#endif

        [Test]
        public void AggregatedExceptions()
        {
            AggregateException eAgg = ThrowAggregatedException();
            var d = CKExceptionData.CreateFrom( eAgg );

            Assert.That( d.ExceptionTypeAssemblyQualifiedName, Is.EqualTo( typeof(AggregateException).AssemblyQualifiedName ) );
            Assert.That( d.ExceptionTypeName, Is.EqualTo( typeof( AggregateException ).Name ) );
            Assert.That( d.AggregatedExceptions.Count, Is.GreaterThanOrEqualTo( 1 ) );
            Assert.That( d.InnerException, Is.SameAs( d.AggregatedExceptions[0] ) );
            for( int i = 0; i < d.AggregatedExceptions.Count; ++i )
            {
                CheckSimpleExceptionData( d.AggregatedExceptions[i], s => s.StartsWith( "Ex n°" ) );
            }
        }

#if NET451 || NET46

        [Test]
        public void reading_and_writing_CKExceptionData_with_Standard_Serialization()
        {
            var dataE0 = CKExceptionData.CreateFrom( ThrowAggregatedException() );
            var dataE1 = CKExceptionData.CreateFrom( ThrowSimpleException( "Test Message" ) );
            var dataE2 = CKExceptionData.CreateFrom( ThrowLoaderException() );
            var dataE3 = CKExceptionData.CreateFrom( ThrowExceptionWithInner() );
            var dataE4 = CKExceptionData.CreateFrom( ThrowTwoInnerExceptions() );

            BinaryFormatter f = new BinaryFormatter();
            using( var mem = new MemoryStream() )
            {
                f.Serialize( mem, dataE0 );
                f.Serialize( mem, dataE1 );
                f.Serialize( mem, dataE2 );
                f.Serialize( mem, dataE3 );
                f.Serialize( mem, dataE4 );
                mem.Position = 0;
                var data0 = (CKExceptionData)f.Deserialize( mem );
                Assert.AreEqual( data0.ToString(), dataE0.ToString() );
                var data1 = (CKExceptionData)f.Deserialize( mem );
                Assert.AreEqual( data1.ToString(), dataE1.ToString() );
                var data2 = (CKExceptionData)f.Deserialize( mem );
                Assert.AreEqual( data2.ToString(), dataE2.ToString() );
                var data3 = (CKExceptionData)f.Deserialize( mem );
                Assert.AreEqual( data3.ToString(), dataE3.ToString() );
                var data4 = (CKExceptionData)f.Deserialize( mem );
                Assert.AreEqual( data4.ToString(), dataE4.ToString() );
            }
        }
#endif

        [Test]
        public void reading_and_writing_CKExceptionData_with_BinaryWriter_and_BinaryReader()
        {
            var dataE0 = CKExceptionData.CreateFrom( ThrowAggregatedException() );
            var dataE1 = CKExceptionData.CreateFrom( ThrowSimpleException( "Test Message" ) );
            var dataE2 = CKExceptionData.CreateFrom( ThrowLoaderException() );
            var dataE3 = CKExceptionData.CreateFrom( ThrowExceptionWithInner() );
            var dataE4 = CKExceptionData.CreateFrom( ThrowTwoInnerExceptions() );
            using( var mem = new MemoryStream() )
            {
                BinaryWriter w = new BinaryWriter( mem );
                dataE0.Write( w );
                dataE1.Write( w );
                dataE2.Write( w );
                dataE3.Write( w );
                dataE4.Write( w );
                mem.Position = 0;
                var r = new BinaryReader( mem );
                var data0 = new CKExceptionData( r );
                Assert.AreEqual( data0.ToString(), dataE0.ToString() );
                var data1 = new CKExceptionData( r );
                Assert.AreEqual( data1.ToString(), dataE1.ToString() );
                var data2 = new CKExceptionData( r );
                Assert.AreEqual( data2.ToString(), dataE2.ToString() );
                var data3 = new CKExceptionData( r );
                Assert.AreEqual( data3.ToString(), dataE3.ToString() );
                var data4 = new CKExceptionData( r );
                Assert.AreEqual( data4.ToString(), dataE4.ToString() );
            }
        }

        static AggregateException ThrowAggregatedException()
        {
            AggregateException eAgg = null;
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
            return eAgg;
        }

        static Exception ThrowExceptionWithInner( bool loaderException = false )
        {
            Exception e;
            try { throw new Exception( "Outer", loaderException ? ThrowLoaderException() : ThrowSimpleException("Inner") ); }
            catch( Exception ex ) { e = ex; }
            return e;
        }

        static CKException ThrowTwoInnerExceptions()
        {
            CKException ckEx;
            try { throw new CKException( ThrowExceptionWithInner( true ), "CK-MostOuter" ); }
            catch( CKException ex ) { ckEx = ex; }
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
            Exception e = null;
            try { Type.GetType( "A.Type, An.Unexisting.Assembly", true ); }
            catch( Exception ex ) { e = ex; }
            return e;
        }

    }
}
