#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\CK.Core.Tests\Monitoring\StupidXmlClient.cs) is part of CiviKey. 
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
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CK.Core;

namespace CK.Core.Tests.Monitoring
{
    public class StupidXmlClient : ActivityMonitorTextHelperClient
    {
        XmlWriter XmlWriter { get; set; }

        public TextWriter InnerWriter { get; private set; }

        public StupidXmlClient( StringWriter s )
        {
            XmlWriter = XmlWriter.Create( s, new XmlWriterSettings() { ConformanceLevel = ConformanceLevel.Fragment, Indent = true } );
            InnerWriter = s;
        }

        public List<XElement> XElements
        {
            get
            {
                string text = InnerWriter.ToString();
                XElement doc = XElement.Parse( "<r>" + text + "</r>" );
                return doc.Elements().ToList();
            }
        }

        protected override void OnEnterLevel( ActivityMonitorLogData data )
        {
            XmlWriter.WriteStartElement( data.MaskedLevel.ToString() );
            XmlWriter.WriteString( data.Text );
        }

        protected override void OnContinueOnSameLevel( ActivityMonitorLogData data )
        {
            XmlWriter.WriteString( data.Text );
        }

        protected override void OnLeaveLevel( LogLevel level )
        {
            XmlWriter.WriteEndElement();
        }

        protected override void OnGroupOpen( IActivityLogGroup g )
        {
            XmlWriter.WriteStartElement( g.MaskedGroupLevel.ToString() + "s" );
            XmlWriter.WriteAttributeString( "Depth", g.Depth.ToString() );
            XmlWriter.WriteAttributeString( "Level", g.GroupLevel.ToString() );
            XmlWriter.WriteAttributeString( "Text", g.GroupText.ToString() );
        }

        protected override void OnGroupClose( IActivityLogGroup g, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            XmlWriter.WriteEndElement();
            XmlWriter.Flush();
        }

        public override string ToString()
        {
            return InnerWriter.ToString();
        }
    }

}
