#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Storage\Impl\PureXml-2.5.5\ReaderBase.cs) is part of CiviKey. 
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
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;
using CK.Core;
using System.Diagnostics;

namespace CK.Storage
{

    internal abstract class ReaderBase : IStructuredReader
    {
        ReaderImpl _root;
        ActionSequence _deserializationActions;
        bool _ignoreUnresolvedType;

        protected ReaderBase( ReaderImpl root )
        {
            _root = root ?? (ReaderImpl)this;
        }

        internal ReaderImpl Root
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
                if( _deserializationActions != null ) _deserializationActions.RunOnce();
                OnDispose();
                _root = null;
            }
        }

        protected abstract void OnDispose();

        public bool IgnoreUnresolvedType
        {
            get { return _ignoreUnresolvedType; }
            set { _ignoreUnresolvedType = value; }
        }

        public abstract Version StorageVersion { get; }

        IStructuredReader IStructuredReader.Current
        {
            get { return Root.Current; }
        }

        public ActionSequence DeserializationActions { get { return _deserializationActions ?? (_deserializationActions = new ActionSequence()); } }

        public event EventHandler<ObjectReadExDataEventArgs> ObjectReadExData;

        public abstract IStructuredReaderBookmark CreateBookmark();

        public abstract XmlReader Xml { get; }

        public ISubStructuredReader OpenSubReader()
        {
            return new ReaderImplSub( Root, this );
        }

        public abstract IServiceProvider BaseServiceProvider { get; }

        public abstract ISimpleServiceContainer ServiceContainer { get; }

        public abstract object GetService( Type serviceType );

        private void AssertCurrentReader()
        {
            if( Root.Current != this )
            {
                throw new InvalidOperationException( "Can be called only on Current reader." );
            }
        }

        Type GetType( string typeName )
        {
            ISimpleTypeFinder f = ComponentModelExtension.GetService<ISimpleTypeFinder>( this );
            if( f == null )
            {
                return Type.GetType( typeName, !_ignoreUnresolvedType );
            }
            return f.ResolveType( typeName, !_ignoreUnresolvedType );
        }

        object IStructuredReader.ReadInlineObject( out StandardReadStatus status )
        {
            AssertCurrentReader();

            XmlReader r = Xml;
            object o = null;
            string type = r.GetAttribute( "type" );
            if( type == null )
            {
                status = StandardReadStatus.ErrorTypeAttributeMissing;
            }
            else
            {
                status = StandardReadStatus.SimpleTypeData;
                switch( type )
                {
                    case "null":
                        status = StandardReadStatus.NullData;
                        r.Skip();
                        break;
                    case "Enum":
                        {
                            status = StandardReadStatus.XmlSerializable;
                            try
                            {
                                Type t = GetType( r.GetAttribute( "typeName" ) );
                                if( t == null )
                                {
                                    
                                    r.Skip();
                                }
                                else
                                {
                                    r.Read();
                                    o = Enum.Parse( t, r.ReadContentAsString() );
                                }
                            }
                            finally
                            {
                                // On error, we can be on the <data key="..." type="Enum" typeName="..." > element or after its content:
                                // by skipping, we either skip the content and the end element or the end element.
                                r.Skip();
                            }
                            break;
                        }
                    case "String":
                        o = r.ReadElementString();
                        break;
                    case "Int32":
                        o = r.ReadElementContentAsInt();
                        break;
                    case "Bool":
                        o = XmlConvert.ToBoolean( r.ReadElementContentAsString() );
                        break;
                    case "DateTimeOffset":
                        o = XmlConvert.ToDateTimeOffset( r.ReadElementContentAsString() );
                        break;
                    case "DateTime":
                        o = XmlConvert.ToDateTime( r.ReadElementContentAsString(), XmlDateTimeSerializationMode.RoundtripKind );
                        break;
                    case "TimeSpan":
                        o = XmlConvert.ToTimeSpan( r.ReadElementContentAsString() );
                        break;
                    case "Guid":
                        o = XmlConvert.ToGuid( r.ReadElementContentAsString() );
                        break;
                    case "Single":
                        o = XmlConvert.ToSingle( r.ReadElementContentAsString() );
                        break;
                    case "Double":
                        o = XmlConvert.ToDouble( r.ReadElementContentAsString() );
                        break;
                    case "Decimal":
                        o = XmlConvert.ToDecimal( r.ReadElementContentAsString() );
                        break;
                    case "Char":
                        o = XmlConvert.ToChar( r.ReadElementContentAsString() );
                        break;
                    case "UInt32":
                        o = XmlConvert.ToUInt32( r.ReadElementContentAsString() );
                        break;
                    case "Int16":
                        o = XmlConvert.ToInt16( r.ReadElementContentAsString() );
                        break;
                    case "UInt16":
                        o = XmlConvert.ToUInt16( r.ReadElementContentAsString() );
                        break;
                    case "Int64":
                        o = XmlConvert.ToInt64( r.ReadElementContentAsString() );
                        break;
                    case "UInt64":
                        o = XmlConvert.ToUInt64( r.ReadElementContentAsString() );
                        break;
                    case "Byte":
                        o = XmlConvert.ToByte( r.ReadElementContentAsString() );
                        break;
                    case "SByte":
                        o = XmlConvert.ToSByte( r.ReadElementContentAsString() );
                        break;
                    case "Structured":
                        {
                            status = StandardReadStatus.Structured;
                            using( ISubStructuredReader rSub = OpenSubReader() )
                            {
                                Type t = GetType( r.GetAttribute( "typeName" ) );
                                if( t != null ) o = ReadInlineObjectStructured( rSub, t, null );
                            }
                            break;
                        }
                    case "XmlSerializable":
                        {
                            status = StandardReadStatus.XmlSerializable;
                            using( Root.CreateJail() )
                            {
                                Type t = GetType( r.GetAttribute( "typeName" ) );
                                if( t != null )
                                {
                                    o = Activator.CreateInstance( t );
                                    IXmlSerializable xmlSer = (IXmlSerializable)o;
                                    xmlSer.ReadXml( Xml );
                                    OnObjectRead( o, (IStructuredReader)this );
                                }
                            }
                            break;
                        }
                    case "Object":
                        {
                            status = StandardReadStatus.BinaryObject;
                            try
                            {
                                int size = int.Parse( r.GetAttribute( "size" ) );
                                // Adds 8 bytes to the size. It allows to detect when the content is bigger than we expect.
                                // ReadContentAsBinHex moves the reader to the next non-content element: this is either
                                // the start element of extra data or the end of the current element.
                                byte[] buffer = new byte[size + 8];
                                int readBytes = 0;
                                // If an error occur while reading content, the reader.ReadState is Error
                                // and this is definitive...
                                r.Read();
                                readBytes = r.ReadContentAsBinHex( buffer, readBytes, buffer.Length );
                                if( readBytes != size )
                                    throw new CKException( R.SizeDifferError, size, readBytes );
                                if( readBytes != -1 )
                                {
                                    BinaryFormatter f = new BinaryFormatter();
                                    using( MemoryStream mem = new MemoryStream( buffer ) )
                                    {
                                        try
                                        {
                                            o = f.Deserialize( mem );
                                        }
                                        catch( Exception ex )
                                        {
                                            throw new CKException( R.DeserializeError, ex );
                                        }
                                    }
                                    OnObjectRead( o, (IStructuredReader)this );
                                }
                            }
                            catch( XmlException ex )
                            {
                                throw new CKException( R.InvalidXml, ex );
                            }
                            finally
                            {
                                // We are on the opening element OR on the closing tag.
                                r.Skip();
                            }
                            break;
                        }
                    default:
                        {
                            status = StandardReadStatus.ErrorUnknownTypeAttribute;
                            break;
                        }
                }
            }
            return o;
        }

        object IStructuredReader.ReadInlineObjectStructured( Type type, object o )
        {
            AssertCurrentReader();

            using( ISubStructuredReader rSub = OpenSubReader() )
            {
                return ReadInlineObjectStructured( rSub, type, o );
            }
        }

        object ReadInlineObjectStructured( ISubStructuredReader rSub, Type type, object o )
        {
            if( type == null && o == null ) throw new ArgumentException( R.AtLeastTypeOrObjectMustBeSpecified );
            if( type == null ) type = o.GetType();
            Type serializerServiceType = typeof( IStructuredSerializer<> ).MakeGenericType( type );
            object serializer = GetService( serializerServiceType );
            if( serializer != null )
            {
                o = serializerServiceType.InvokeMember( "ReadInlineContent", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, serializer, new object[] { rSub, o } );
            }
            else
            {
                // If no serializer is found, the object must implement IStructuredSerializable interface.
                if( o == null ) o = Activator.CreateInstance( type );
                ((IStructuredSerializable)o).ReadContent( rSub );
            }
            OnObjectRead( o, rSub );
            return o;
        }

        void OnObjectRead( object o, IStructuredReader rSub )
        {
            ObjectReadExDataEventArgs e = new ObjectReadExDataEventArgs( rSub, o );
            while( Xml.IsStartElement() )
            {
                e.Handled = false;
                using( Root.CreateJail() )
                {
                    RaiseReadEvent( e );
                }
            }
        }

        internal void RaiseReadEvent( ObjectReadExDataEventArgs e )
        {
            var h = ObjectReadExData;
            if( h != null ) h( this, e );
            if( !e.Handled ) PropagateReadEvent( e );
        }

        protected virtual void PropagateReadEvent( ObjectReadExDataEventArgs e )
        {
        }

    }
}
