#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Storage\Impl\PureXml-2.5.5\WriterBase.cs) is part of CiviKey. 
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
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;
using CK.Core;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace CK.Storage
{



    /// <summary>
    /// Simple implementation of <see cref="IStructuredWriter"/>.
    /// </summary>
    internal abstract class WriterBase : IStructuredWriter
    {
        WriterImpl _root;

        protected WriterBase( WriterImpl root )
        {
            _root = root ?? (WriterImpl)this;
        }

        internal WriterImpl Root
        {
            get { return _root; }
        }

        public bool IsDisposed
        {
            get { return _root == null; }
        }

        public void Dispose()
        {
            if( _root != null )
            {
                OnDispose();
                _root = null;
            }
        }

        public event EventHandler<ObjectWriteExDataEventArgs> ObjectWriteExData;

        protected abstract void OnDispose();

        IStructuredWriter IStructuredWriter.Current
        {
            get { return Root.Current; }
        }

        public abstract XmlWriter Xml { get; }

        public abstract IServiceProvider BaseServiceProvider { get; }

        public abstract ISimpleServiceContainer ServiceContainer { get; }
        
        public abstract object GetService( Type serviceType );

        public ISubStructuredWriter OpenSubWriter()
        {
            return new WriterImplSub( Root, Root.Current );
        }

        void IStructuredWriter.WriteInlineObject( object o )
        {
            AssertCurrentWriter();
            
            if( o == null )
            {
                Xml.WriteAttributeString( "type", "null" );
                Xml.WriteEndElement();
                return;
            }

            Type t = o.GetType();
            if( DoWriteInlineObjectStructured( o, t, true ) ) return;
            if( t.IsValueType )
            {
                if( t.IsEnum )
                {
                    Xml.WriteAttributeString( "type", "Enum" );
                    Xml.WriteAttributeString( "typeName", GetTypeName( t ) );
                    Xml.WriteString( Enum.Format( t, o, "g" ) );
                }
                else if( t == typeof( Int32 ) ) WriteTypedObjectContent( Xml, (Int32)o );
                else if( t == typeof( bool ) ) WriteTypedObjectContent( Xml, (bool)o );
                else if( t == typeof( Guid ) ) WriteTypedObjectContent( Xml, (Guid)o );
                else if( t == typeof( char ) ) WriteTypedObjectContent( Xml, (char)o );
                else if( t == typeof( Decimal ) ) WriteTypedObjectContent( Xml, (Decimal)o );
                else if( t == typeof( Single ) ) WriteTypedObjectContent( Xml, (Single)o );
                else if( t == typeof( Double ) ) WriteTypedObjectContent( Xml, (Double)o );
                else if( t == typeof( UInt32 ) ) WriteTypedObjectContent( Xml, (UInt32)o );
                else if( t == typeof( Int64 ) ) WriteTypedObjectContent( Xml, (Int64)o );
                else if( t == typeof( UInt64 ) ) WriteTypedObject( Xml, (UInt64)o );
                else if( t == typeof( Int16 ) ) WriteTypedObjectContent( Xml, (Int16)o );
                else if( t == typeof( UInt16 ) ) WriteTypedObjectContent( Xml, (UInt16)o );
                else if( t == typeof( SByte ) ) WriteTypedObjectContent( Xml, (SByte)o );
                else if( t == typeof( Byte ) ) WriteTypedObjectContent( Xml, (Byte)o );
                else if( t == typeof( DateTime ) ) WriteTypedObjectContent( Xml, (DateTime)o );
                else if( t == typeof( TimeSpan ) ) WriteTypedObject( Xml, (TimeSpan)o );
                else if( t.IsSerializable ) WriteSerializable( Xml, o );
                else
                {
                    throw new CKException( "Unable to Serialize object of Type {0} from assembly {1}.", t.FullName, t.Assembly.FullName );
                }
                Xml.WriteEndElement();
                return;
            }
            if( t == typeof( string ) )
            {
                WriteTypedObjectContent( Xml, (string)o );
                Xml.WriteEndElement();
                return;
            }
            if( t == typeof( CultureInfo ) )
            {
                WriteTypedObjectContent( Xml, (CultureInfo)o );
                Xml.WriteEndElement();
                return;
            }
            using( ISubStructuredWriter wSub = OpenSubWriter() )
            {
                Debug.Assert( Root.Current == wSub );
                // IXmlSerializable or IsSerializable (reference type).
                IXmlSerializable xmlSer = o as IXmlSerializable;
                if( xmlSer != null )
                {
                    Xml.WriteAttributeString( "type", "XmlSerializable" );
                    Xml.WriteAttributeString( "typeName", GetTypeName( t ) );
                    xmlSer.WriteXml( Xml );
                }
                else if( t.IsSerializable )
                {
                    WriteSerializable( Xml, o );
                }
                else throw new CKException( "Unable to Serialize object of Type {0} from assembly {1}.", t.FullName, t.Assembly.FullName );
                DoWriteEnd( wSub, o );
            }
        }

        void IStructuredWriter.WriteInlineObjectStructured( object o )
        {
            AssertCurrentWriter();
            if( o == null )
            {
                Xml.WriteAttributeString( "type", "null" );
                Xml.WriteEndElement();
                return;
            }
            Type t = o.GetType();
            if( !DoWriteInlineObjectStructured( o, t, false ) )
                throw new CKException( R.NotWritableStructuredObject, t.AssemblyQualifiedName );
        }

        private void AssertCurrentWriter()
        {
            if( Root.Current != this )
            {
                throw new InvalidOperationException( "Can be called only on Current writer." );
            }
        }

        string GetTypeName( Type t )
        {
            ISimpleTypeNaming s = (ISimpleTypeNaming)GetService( typeof( ISimpleTypeNaming ) );
            return s != null ? s.GetTypeName( t ) : t.AssemblyQualifiedName;
        }

        private void DoWriteEnd( IStructuredWriter wSub, object o )
        {
            ObjectWriteExDataEventArgs e = new ObjectWriteExDataEventArgs( wSub, o );
            RaiseWriteEvent( e );
            Xml.WriteEndElement();
        }

        internal void RaiseWriteEvent( ObjectWriteExDataEventArgs e )
        {
            var h = ObjectWriteExData;
            if( h != null ) h( this, e );
            PropagateWriteEvent( e );
        }

        protected virtual void PropagateWriteEvent( ObjectWriteExDataEventArgs e )
        {
        }

        private bool DoWriteInlineObjectStructured( object o, Type t, bool writeTypeInfo )
        {
            Debug.Assert( o != null );
            Type serializerServiceType = typeof( IStructuredSerializer<> ).MakeGenericType( t );
            object serializer = GetService( serializerServiceType );
            if( serializer != null )
            {
                using( ISubStructuredWriter wSub = OpenSubWriter() )
                {
                    BindingFlags fInvoke = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod;
                    object[] parameters = new object[] { wSub, o };
                    if( writeTypeInfo )
                    {
                        Xml.WriteAttributeString( "type", "Structured" );
                        Xml.WriteAttributeString( "typeName", GetTypeName( t ) );
                    }
                    serializerServiceType.InvokeMember( "WriteInlineContent", fInvoke, null, serializer, parameters );
                    DoWriteEnd( wSub, o );
                }
                return true;
            }
            IStructuredSerializable structured = o as IStructuredSerializable;
            if( structured != null )
            {
                using( ISubStructuredWriter wSub = OpenSubWriter() )
                {
                    if( writeTypeInfo )
                    {
                        Xml.WriteAttributeString( "type", "Structured" );
                        Xml.WriteAttributeString( "typeName", GetTypeName( t ) );
                    }
                    structured.WriteContent( wSub );
                    DoWriteEnd( wSub, o );
                }
                return true;
            }
            return false;
        }

        #region WriteTypedOjectContent & WriteSerializable

        private static void WriteTypedObjectContent( XmlWriter w, CultureInfo p )
        {
            w.WriteAttributeString( "type", "CultureInfo" );
            w.WriteString( XmlConvert.ToString( p.LCID ) );
        }

        private static void WriteTypedObjectContent( XmlWriter w, Double p )
        {
            w.WriteAttributeString( "type", "Double" );
            w.WriteString( XmlConvert.ToString( p ) );
        }

        private static void WriteTypedObjectContent( XmlWriter w, Single p )
        {
            w.WriteAttributeString( "type", "Single" );
            w.WriteString( XmlConvert.ToString( p ) );
        }

        private static void WriteTypedObjectContent( XmlWriter w, Decimal p )
        {
            w.WriteAttributeString( "type", "Decimal" );
            w.WriteString( XmlConvert.ToString( p ) );
        }

        private static void WriteTypedObjectContent( XmlWriter w, Char p )
        {
            w.WriteAttributeString( "type", "Char" );
            w.WriteString( XmlConvert.ToString( p ) );
        }

        private static void WriteTypedObjectContent( XmlWriter w, Guid p )
        {
            w.WriteAttributeString( "type", "Guid" );
            w.WriteString( XmlConvert.ToString( p ) );
        }

        private static void WriteTypedObject( XmlWriter w, TimeSpan p )
        {
            w.WriteAttributeString( "type", "TimeSpan" );
            w.WriteString( XmlConvert.ToString( p ) );
        }

        private static void WriteTypedObjectContent( XmlWriter w, DateTime p )
        {
            w.WriteAttributeString( "type", "DateTime" );
            w.WriteString( XmlConvert.ToString( p, XmlDateTimeSerializationMode.RoundtripKind ) );
        }

        private static void WriteTypedObjectContent( XmlWriter w, bool p )
        {
            w.WriteAttributeString( "type", "Bool" );
            w.WriteString( XmlConvert.ToString( p ) );
        }

        private static void WriteTypedObjectContent( XmlWriter w, string p )
        {
            w.WriteAttributeString( "type", "String" );
            w.WriteString( p );
        }

        private static void WriteTypedObjectContent( XmlWriter w, Int64 p )
        {
            w.WriteAttributeString( "type", "Int64" );
            w.WriteString( XmlConvert.ToString( p ) );
        }

        private static void WriteTypedObject( XmlWriter w, UInt64 p )
        {
            w.WriteAttributeString( "type", "UInt64" );
            w.WriteString( XmlConvert.ToString( p ) );
        }

        private static void WriteTypedObjectContent( XmlWriter w, Int32 p )
        {
            w.WriteAttributeString( "type", "Int32" );
            w.WriteString( XmlConvert.ToString( p ) );
        }

        private static void WriteTypedObjectContent( XmlWriter w, UInt32 p )
        {
            w.WriteAttributeString( "type", "UInt32" );
            w.WriteString( XmlConvert.ToString( p ) );
        }

        private static void WriteTypedObjectContent( XmlWriter w, Int16 p )
        {
            w.WriteAttributeString( "type", "Int16" );
            w.WriteString( XmlConvert.ToString( p ) );
        }

        private static void WriteTypedObjectContent( XmlWriter w, UInt16 p )
        {
            w.WriteAttributeString( "type", "UInt16" );
            w.WriteString( XmlConvert.ToString( p ) );
        }

        private static void WriteTypedObjectContent( XmlWriter w, SByte p )
        {
            w.WriteAttributeString( "type", "SByte" );
            w.WriteString( XmlConvert.ToString( p ) );
        }

        private static void WriteTypedObjectContent( XmlWriter w, Byte p )
        {
            w.WriteAttributeString( "type", "Byte" );
            w.WriteString( XmlConvert.ToString( p ) );
        }

        static private void WriteSerializable( XmlWriter w, object o )
        {
            w.WriteAttributeString( "type", "Object" );
            using( MemoryStream mem = new MemoryStream() )
            {
                BinaryFormatter f = new BinaryFormatter();
                f.Serialize( mem, o );
                byte[] buffer = mem.ToArray();
                w.WriteAttributeString( "size", buffer.Length.ToString() );
                w.WriteBinHex( buffer, 0, buffer.Length );
            }
        }
        #endregion


    }
}
