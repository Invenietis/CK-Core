#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\CKExceptionTests.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
            Assert.That( simpleData.FusionLog, Is.Null );
            
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


        [Test]
        public void SerializeCKException()
        {
            CKException ckEx;
            try { throw new CKException( ThrowExceptionWithInner(), "CK-MostOuter" ); } 
            catch( CKException ex ) { ckEx = ex; }

            BinaryFormatter f = new BinaryFormatter();
            using( var mem = new MemoryStream() )
            {
                f.Serialize( mem, ckEx );
                mem.Position = 0;
                var ckEx2 = (CKException)f.Deserialize( mem );
                Assert.AreEqual( CKExceptionData.CreateFrom( ckEx2 ).ToString(), CKExceptionData.CreateFrom( ckEx ).ToString() );
            }
        }

        [Test]
        public void SerializeCKExceptionData()
        {
            var data = CKExceptionData.CreateFrom( ThrowAggregatedException() );

            BinaryFormatter f = new BinaryFormatter();
            using( var mem = new MemoryStream() )
            {
                f.Serialize( mem, data );
                mem.Position = 0;
                var data2 = (CKExceptionData)f.Deserialize( mem );
                Assert.AreEqual( data2.ToString(), data.ToString() );
            }
        }

        [Test]
        public void BinaryReadWriteCKExceptionData()
        {
            var data = CKExceptionData.CreateFrom( ThrowAggregatedException() );
            using( var mem = new MemoryStream() )
            {
                BinaryWriter w = new BinaryWriter( mem );
                data.Write( w );
                mem.Position = 0;
                var data2 = new CKExceptionData( new BinaryReader( mem ) );
                Assert.AreEqual( data2.ToString(), data.ToString() );
            }
        }

        static AggregateException ThrowAggregatedException()
        {
            AggregateException eAgg = null;
            try
            {
                Parallel.For( 0, 50, i =>
                {
                    if( i % 1 == 0 ) throw new Exception( String.Format( "Ex n°{0}", i ), ThrowExceptionWithInner() );
                    else throw new Exception( String.Format( "Ex n°{0}", i ) );
                } );
            }
            catch( AggregateException ex )
            {
                eAgg = ex;
            }
            return eAgg;
        }

        static Exception ThrowExceptionWithInner()
        {
            Exception e;
            try
            {
                throw new Exception( "Outer", ThrowSimpleException( "Inner" ) );
            }
            catch( Exception ex )
            {
                e = ex;
            }
            return e;
        }

        static Exception ThrowSimpleException( string message )
        {
            Exception e;
            try { throw new Exception( message ); }
            catch( Exception ex ) { e = ex; }
            return e;
        }

    }
}
