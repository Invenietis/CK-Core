#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Requirements\RequirementLayer.cs) is part of CiviKey. 
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

namespace CK.Plugin
{
    /// <summary>
    /// Combines a <see cref="PluginRequirements"/> and a <see cref="ServiceRequirements"/>.
    /// </summary>
    public class RequirementLayer
    {
        PluginRequirementCollection _pluginRequirements;
        ServiceRequirementCollection _serviceRequirements;

        /// <summary>
        /// Name of these requirements. This is an optional name that identifies this layer.
        /// It is not intended to be used as a unique key.
        /// </summary>
        public string LayerName { get; set; }

        /// <summary>
        /// Gets a <see cref="PluginRequirementCollection"/> that describes plugins requirements.
        /// </summary>
        public IPluginRequirementCollection PluginRequirements { get { return _pluginRequirements; } }

        /// <summary>
        /// Gets a <see cref="ServiceRequirementCollection"/> that decribes service requirements.
        /// </summary>
        public IServiceRequirementCollection ServiceRequirements { get { return _serviceRequirements; } }

        /// <summary>
        /// Initializes a new <see cref="RequirementLayer"/>.
        /// </summary>
        /// <param name="layerName">Optional name for the requirements.</param>
        public RequirementLayer( string layerName )
        {
            LayerName = layerName;
            _pluginRequirements = new PluginRequirementCollection();
            _serviceRequirements = new ServiceRequirementCollection();
        }

        /// <summary>
        /// Reads back an existing <see cref="RequirementLayer"/> or creates a new one 
        /// from xml data previously written by <see cref="WriteInlineToXml"/> method.
        /// </summary>
        /// <param name="r">The xml stream to read: the reader must be on an opened element.</param>
        /// <param name="reqLayer">An existing layer or null to create a new one.</param>
        public static void ReadInlineFromXml( XmlReader r, ref RequirementLayer reqLayer )
        {
            if( reqLayer != null )
            {
                reqLayer.LayerName = r.GetAttribute( "Name" );
            }
            else reqLayer = new RequirementLayer( r.GetAttribute( "Name" ) );

            r.Read();

            if( r.IsStartElement( "PluginRequirements" ) )
            {
                if( r.IsEmptyElement ) r.Read();
                else
                {
                    r.Read();
                    reqLayer.PluginRequirements.Clear();
                    while( r.IsStartElement( "PluginRequirement" ) )
                    {
                        Guid pluginId = new Guid( r.GetAttribute( "PluginId" ) );
                        RunningRequirement runningReq = (RunningRequirement)Enum.Parse( typeof( RunningRequirement ), r.GetAttribute( "Requirement" ) );

                        reqLayer.PluginRequirements.AddOrSet( pluginId, runningReq );

                        if( r.IsEmptyElement ) r.Read();
                        else r.Skip();
                    }
                    r.ReadEndElement();
                }
            }
            if( r.IsStartElement( "ServiceRequirements" ) )
            {
                if( r.IsEmptyElement ) r.Read();
                else
                {
                    r.Read();
                    reqLayer.ServiceRequirements.Clear();
                    while( r.IsStartElement( "ServiceRequirement" ) )
                    {
                        string assemblyQualifiedName = r.GetAttribute( "AssemblyQualifiedName" );
                        RunningRequirement runningReq = (RunningRequirement)Enum.Parse( typeof( RunningRequirement ), r.GetAttribute( "Requirement" ) );

                        reqLayer.ServiceRequirements.AddOrSet( assemblyQualifiedName, runningReq );

                        if( r.IsEmptyElement ) r.Read();
                        else r.Skip();
                    }
                    r.ReadEndElement();
                }
            }
        }

        /// <summary>
        /// Writes a <see cref="RequirementLayer"/> as xml.
        /// </summary>
        /// <param name="w">The xml stream to write to: an element must be opened.</param>
        /// <param name="o">The object to write. Can not be null.</param>
        public static void WriteInlineToXml( XmlWriter w, RequirementLayer o )
        {
            w.WriteAttributeString( "Name", o.LayerName );

            w.WriteStartElement( "PluginRequirements" );

            foreach( PluginRequirement req in o.PluginRequirements )
            {
                w.WriteStartElement( "PluginRequirement" );
                w.WriteAttributeString( "PluginId", req.PluginId.ToString() );
                w.WriteAttributeString( "Requirement", req.Requirement.ToString() );
                w.WriteFullEndElement();
            }

            w.WriteFullEndElement();

            w.WriteStartElement( "ServiceRequirements" );

            foreach( ServiceRequirement req in o.ServiceRequirements )
            {
                w.WriteStartElement( "ServiceRequirement" );
                w.WriteAttributeString( "AssemblyQualifiedName", req.AssemblyQualifiedName );
                w.WriteAttributeString( "Requirement", req.Requirement.ToString() );
                w.WriteFullEndElement();
            }

            w.WriteFullEndElement();
        }

    }
}
