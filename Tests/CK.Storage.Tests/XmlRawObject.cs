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
        void IStructuredSerializable.ReadInlineContent( IStructuredReader sr )
        {
            ReadInlineContent( sr.Xml );
        }
        void IStructuredSerializable.WriteInlineContent( IStructuredWriter sw )
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
