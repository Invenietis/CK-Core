#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\SimpleSinks.cs) is part of CiviKey. 
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
* Copyright © 2007-2013, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml;
using CK.Core;

namespace CK.Core.Tests
{
    [ExcludeFromCodeCoverage]
    public class StringImpl : IActivityLoggerSink
    {
        public StringWriter Writer { get; private set; }
        public bool WriteTags { get; private set; }
        public bool WriteConclusionTraits { get; private set; }

        public StringImpl( bool writeTags = false, bool writeConclusionTraits = false )
        {
            Writer = new StringWriter();
            WriteTags = writeTags;
            WriteConclusionTraits = writeConclusionTraits;
        }

        public void OnEnterLevel( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
        {
            Writer.WriteLine();
            Writer.Write( level.ToString() + ": " + text );
            if( WriteTags ) Writer.Write( "-[{0}]", tags.ToString() );
        }

        public void OnContinueOnSameLevel( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
        {
            Writer.Write( text );
            if( WriteTags ) Writer.Write( "-[{0}]", tags.ToString() );
        }

        public void OnLeaveLevel( LogLevel level )
        {
            Writer.Flush();
        }

        public void OnGroupOpen( IActivityLogGroup g )
        {
            Writer.WriteLine();
            Writer.Write( new String( '+', g.Depth ) );
            Writer.Write( "{1} ({0})", g.GroupLevel, g.GroupText );
            if( g.Exception != null ) Writer.Write( "Exception: " + g.Exception.Message );
            if( WriteTags ) Writer.Write( "-[{0}]", g.GroupTags.ToString() );
        }

        public void OnGroupClose( IActivityLogGroup g, ICKReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            Writer.WriteLine();
            Writer.Write( new String( '-', g.Depth ) );
            if( WriteConclusionTraits )
            {
                Writer.Write( String.Join( ", ", conclusions.Select( c => c.Text + "-/[/"+ c.Tag.ToString() +"/]/" ) ) );
            }
            else
            {
                Writer.Write( String.Join( ", ", conclusions.Select( c => c.Text ) ) );
            }
        }

        public override string ToString()
        {
            return Writer.ToString();
        }
    }

    [ExcludeFromCodeCoverage]
    public class XmlImpl : IActivityLoggerSink
    {
        XmlWriter XmlWriter { get; set; }

        public TextWriter InnerWriter { get; private set; }

        public XmlImpl( StringWriter s )
        {
            XmlWriter = XmlWriter.Create( s, new XmlWriterSettings() { ConformanceLevel = ConformanceLevel.Fragment, Indent = true } );
            InnerWriter = s;
        }

        public void OnEnterLevel( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
        {
            XmlWriter.WriteStartElement( level.ToString() );
            XmlWriter.WriteString( text );
        }

        public void OnContinueOnSameLevel( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
        {
            XmlWriter.WriteString( text );
        }

        public void OnLeaveLevel( LogLevel level )
        {
            XmlWriter.WriteEndElement();
        }

        public void OnGroupOpen( IActivityLogGroup g )
        {
            XmlWriter.WriteStartElement( g.GroupLevel.ToString() + "s" );
            XmlWriter.WriteAttributeString( "Depth", g.Depth.ToString() );
            XmlWriter.WriteAttributeString( "Level", g.GroupLevel.ToString() );
            XmlWriter.WriteAttributeString( "Text", g.GroupText.ToString() );
        }

        public void OnGroupClose( IActivityLogGroup g, ICKReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            XmlWriter.WriteEndElement();
            XmlWriter.Flush();
        }
    }

}
