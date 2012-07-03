#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Storage.Tests\XmlRawObject.cs) is part of CiviKey. 
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
using System.Xml;
using CK.Core;
using CK.Storage;
using System.Xml.Serialization;

namespace Storage
{
    public enum BugRead
    {
        None = 0,
        SkipTag,
        MoveToEndTag,
        ThrowApplicationException,
    }

    public class XmlRawObjectBase
    {
        public int Power;
        public string Name = "Default Name";
        public BugRead BugWhileReading;

        protected void ReadInlineContent( XmlReader r )
        {
            BugWhileReading = r.GetAttributeEnum( "BugWhileReading", BugRead.None );
            switch( BugWhileReading )
            {
                case BugRead.SkipTag:
                    r.Skip();
                    return;
                case BugRead.MoveToEndTag:
                    r.Skip();
                    while( r.ReadState != ReadState.EndOfFile ) r.Read();
                    return;
                case BugRead.ThrowApplicationException:
                    throw new ApplicationException( "BugRead.Throw" );
            }
            Power = r.GetAttributeInt( "Power", 0 );
            r.Read();
            r.ReadStartElement( "Name" );
            Name = r.ReadString();
            r.ReadEndElement();
        }

        protected void WriteInlineContent( XmlWriter w )
        {
            w.WriteAttributeString( "BugWhileReading", BugWhileReading.ToString() );
            w.WriteAttributeString( "Power", Power.ToString() );
            w.WriteStartElement( "Name" );
            w.WriteString( Name );
            w.WriteEndElement(); // Name
        }

        public bool Equals( XmlRawObjectBase other )
        {
            return other != null && Name == other.Name && Power == other.Power;
        }

        public override bool Equals( object obj )
        {
            return Equals( obj as XmlRawObjectBase );
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Power;
        }
    }

    public class XmlRawObjectStructured : XmlRawObjectBase, IStructuredSerializable
    {
        void IStructuredSerializable.ReadContent( IStructuredReader sr )
        {
            ReadInlineContent( sr.Xml );
        }
        void IStructuredSerializable.WriteContent( IStructuredWriter sw )
        {
            WriteInlineContent( sw.Xml );
        }
    }

    public class XmlRawObjectXmlSerialzable : XmlRawObjectBase, IXmlSerializable
    {
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml( XmlReader reader )
        {
            ReadInlineContent( reader );
            if( BugWhileReading == BugRead.None ) reader.ReadEndElement();
        }

        public void WriteXml( XmlWriter writer )
        {
            WriteInlineContent( writer );
        }

    }

}
