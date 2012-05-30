#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.SharedDic\ISharedDictionaryWriter.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using CK.SharedDic;
using CK.Storage;
using CK.Core;

namespace CK.Plugin.Config
{
    public interface ISharedDictionaryWriter : IDisposable
    {
        /// <summary>
        /// Fires whenever a plugin/object data is about to be written.
        /// </summary>
        event EventHandler<SharedDictionaryWriterEventArgs> BeforePluginsData;

        /// <summary>
        /// Fires whenever a plugin/object data has been written.
        /// </summary>
        event EventHandler<SharedDictionaryWriterEventArgs> AfterPluginsData;

        /// <summary>
        /// Gets the <see cref="IStructuredWriter"/> into which this writer writes data.
        /// </summary>
        IStructuredWriter StructuredWriter { get; }

        /// <summary>
        /// Gets the <see cref="ISharedDictionary"/> to which this writer is associated.
        /// </summary>
        ISharedDictionary SharedDictionary { get; }

        /// <summary>
        /// Writes plugins data for an object in a named element. 
        /// Element is written only if required: if there is no data to write, nothing is written.
        /// </summary>
        /// <param name="elementName">Name of the element that will contain the configuration.</param>
        /// <param name="o">Object for which configuration must be written.</param>
        /// <param name="writeEmptyElement">True to force an empty &lt;<paramref name="elemenName"/> /&gt; element to be written even 
        /// if there is no data. Defaults to false.
        /// </param>
        /// <returns>The number of plugins for which data has been written.</returns>
        int WritePluginsDataElement( string elementName, object o, bool writeEmptyElement );

        /// <summary>
        /// Writes plugins data for an object: data element is already opened.
        /// Use <see cref="WritePluginsDataElement"/> to write a named element.
        /// </summary>
        /// <param name="o">The object for which data must be written.</param>
        /// <returns>The number of plugins for which data has been written.</returns>
        int WritePluginsData( object o );

    }
}
