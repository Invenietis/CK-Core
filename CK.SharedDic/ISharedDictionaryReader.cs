#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.SharedDic\ISharedDictionaryReader.cs) is part of CiviKey. 
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
    public interface ISharedDictionaryReader : IDisposable
    {
        /// <summary>
        /// Fires whenever a plugin/object data is about to be read and the plugin is registered.
        /// The <see cref="ReadPluginInfo"/> property is available and <see cref="SharedDictionaryReaderEventArgs"/>
        /// exposes the <see cref="IObjectPluginAssociation"/>.
        /// </summary>
        event EventHandler<SharedDictionaryReaderEventArgs> BeforePluginsData;

        /// <summary>
        /// Fires whenever a plugin/object data has been read (the plugin is registered).
        /// The <see cref="ReadPluginInfo"/> property is available and <see cref="SharedDictionaryReaderEventArgs"/>
        /// exposes the <see cref="IObjectPluginAssociation"/>.
        /// </summary>
        event EventHandler<SharedDictionaryReaderEventArgs> AfterPluginsData;

        /// <summary>
        /// Gets the currently read plugin <see cref="INamedVersionedUniqueId"/>: the version
        /// exposed here is the one of the data, it may differ from the actual plugin's version.
        /// Null when the reader is not reading any plugin data.
        /// </summary>
        INamedVersionedUniqueId ReadPluginInfo { get; }

        /// <summary>
        /// Gets the <see cref=MergeMode""/> that this reader use.
        /// </summary>
        MergeMode MergeMode { get; }

        /// <summary>
        /// Gets the <see cref="IStructuredReader"/> from which this reader reads data.
        /// </summary>
        IStructuredReader StructuredReader { get; }

        /// <summary>
        /// Gets the <see cref="ISharedDictionary"/> to which this reader is associated.
        /// </summary>
        ISharedDictionary SharedDictionary { get; }

        /// <summary>
        /// Read object data from the reader. 
        /// If the current element name does not match the name provided, false is returned.
        /// </summary>
        /// <param name="elementName">Name of the element.</param>
        /// <param name="o">The object for which data must be read.</param>
        /// <returns>True if the element has been found and read. Errors (if any) are stored in the <see cref="ErrorCollector"/>.</returns>
        bool ReadPluginsDataElement( string elementName, object o );

        /// <summary>
        /// Reads plugins data for an object: the reader is already positionned on the data. 
        /// Errors (if any) are stored in the <see cref="ErrorCollector"/>.
        /// Use <see cref="ReadPluginsDataElement"/> to read an element.
        /// </summary>
        /// <param name="o">The object for which data must be read.</param>
        void ReadPluginsData( object o );

        /// <summary>
        /// Gets a writeable collection of <see cref="ReadElementObjectInfo"/> that must contains
        /// only read errors.
        /// </summary>
        IList<ReadElementObjectInfo> ErrorCollector { get; }

        /// <summary>
        /// Hook that enables this <see cref="ISharedDictionaryReader"/> implementation
        /// to filter every read information.
        /// </summary>
        /// <param name="o">Object for which information has been read.</param>
        /// <param name="pluginId">Plugin identifier for which information has been read.</param>
        /// <param name="info">The information read from the stream.</param>
        /// <returns>
        /// Simple implementation should return the <paramref name="info"/> unchanged (this is what does the default implementation).
        /// It may also returns null to ignore (skip) this piece of information. 
        /// More complex implementation can use this hook to transform the data (by changing the <see cref="ReadElementObjectInfo.ReadInfo"/> of the <paramref name="info"/>)
        /// or return a brand new <see cref="ReadElementObjectInfo"/>.
        /// </returns>
        ReadElementObjectInfo PreProcessReadInfo( object o, INamedVersionedUniqueId pluginId, ReadElementObjectInfo info );

    }
}
