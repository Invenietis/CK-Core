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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml;
using CK.Core;

namespace CK.Core.Tests
{
    [ExcludeFromCodeCoverage]
    public class StupidXmlClient : ActivityMonitorTextHelperClient
    {
        XmlWriter XmlWriter { get; set; }

        public TextWriter InnerWriter { get; private set; }

        public StupidXmlClient( StringWriter s )
        {
            XmlWriter = XmlWriter.Create( s, new XmlWriterSettings() { ConformanceLevel = ConformanceLevel.Fragment, Indent = true } );
            InnerWriter = s;
        }

        protected override void OnEnterLevel( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
        {
            XmlWriter.WriteStartElement( level.ToString() );
            XmlWriter.WriteString( text );
        }

        protected override void OnContinueOnSameLevel( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
        {
            XmlWriter.WriteString( text );
        }

        protected override void OnLeaveLevel( LogLevel level )
        {
            XmlWriter.WriteEndElement();
        }

        protected override void OnGroupOpen( IActivityLogGroup g )
        {
            XmlWriter.WriteStartElement( g.GroupLevel.ToString() + "s" );
            XmlWriter.WriteAttributeString( "Depth", g.Depth.ToString() );
            XmlWriter.WriteAttributeString( "Level", g.GroupLevel.ToString() );
            XmlWriter.WriteAttributeString( "Text", g.GroupText.ToString() );
        }

        protected override void OnGroupClose( IActivityLogGroup g, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            XmlWriter.WriteEndElement();
            XmlWriter.Flush();
        }
    }

}
