#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Config\Impl\RequirementLayerSerializer.cs) is part of CiviKey. 
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

using CK.Plugin;
using CK.Storage;

namespace CK.Plugin.Config
{
    /// <summary>
    /// Implements a <see cref="IStructuredSerializer{T}"/> for <see cref="RequirementLayer"/>.
    /// </summary>
    public class RequirementLayerSerializer : IStructuredSerializer<RequirementLayer>
    {
        /// <summary>
        /// Gets the singleton instance for this <see cref="RequirementLayerSerializer"/>.
        /// </summary>
        public static readonly IStructuredSerializer<RequirementLayer> Instance = new RequirementLayerSerializer();

        private RequirementLayerSerializer()
        {
        }

        object IStructuredSerializer<RequirementLayer>.ReadInlineContent( IStructuredReader sr, RequirementLayer reqLayer )
        {
            RequirementLayer.ReadInlineFromXml( sr.Xml, ref reqLayer );
            return reqLayer;
        }

        void IStructuredSerializer<RequirementLayer>.WriteInlineContent( IStructuredWriter sw, RequirementLayer o )
        {
            RequirementLayer.WriteInlineToXml( sw.Xml, o );
        }

    }
}
