#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Storage\IStructuredReader.cs) is part of CiviKey. 
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

namespace CK.Storage
{

    /// <summary>
    /// The interface to read information from a simple, potentially hybrid, structured storage. 
    /// The heart of the storage is an Xml stream exposed here as the <see cref="P:XmlReader"/> property. 
    /// It can be easily extended since it implements <see cref="IServiceProvider"/> and contains a <see cref="ISimpleServiceContainer"/>.
    /// </summary>
    /// <remarks>
    /// Since <see cref="IDisposable"/> is implemented, you must call <see cref="IDisposable.Dispose"/> when
    /// you are finished reading.
    /// </remarks>
    public interface IStructuredReader : IServiceProvider, IDisposable
    {

        /// <summary>
        /// Fires whenever an object has been read for each extra 
        /// element found after the object data.
        /// </summary>
        event EventHandler<ObjectReadExDataEventArgs> ObjectReadExData;

        /// <summary>
        /// Gets the current <see cref="IStructuredReader"/>. 
        /// It may be this reader or any subordinated reader created by <see cref="OpenSubReader"/>.
        /// </summary>
        /// <returns>The current reader.</returns>
        IStructuredReader Current { get; }

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> to which this
        /// reader is ultimately bound.
        /// </summary>
        /// <remarks>
        /// This is the root provider: any <see cref="ISubStructuredReader"/> subordinated
        /// to a <see cref="IStructuredReader"/> are bound to the same base service provider.
        /// </remarks>
        IServiceProvider BaseServiceProvider { get; }

        /// <summary>
        /// Gets the global version defined at the root level (the 'document' or 'file' notion).
        /// </summary>
        Version StorageVersion { get; }

        /// <summary>
        /// Offers post deserialization (deferred) actions.
        /// Actions should be explicitely executed at the end of a read session, otherwise remaining 
        /// actions are invoked during <see cref="IDisposable.Dispose"/> execution.
        /// </summary>
        ActionSequence DeserializationActions { get; }

        /// <summary>
        /// Gets the <see cref="ISimpleServiceContainer"/> for this reader.
        /// </summary>
        ISimpleServiceContainer ServiceContainer { get; }

        /// <summary>
        /// Creates a bookmark for this reader. A bookmark will be able to restore a <see cref="IStructuredReader"/>
        /// similar to this one.
        /// </summary>
        /// <returns>A bookmark.</returns>
        IStructuredReaderBookmark CreateBookmark();

        /// <summary>
        /// Gets the main <see cref="XmlReader"/> object.
        /// </summary>
        XmlReader Xml { get; }

        /// <summary>
        /// Returns a <see cref="ISubStructuredReader"/> that can be used to read the current node,
        /// and all its descendants from the <see cref="P:XmlReader"/>. 
        /// <see cref="M:XmlReader.ReadSubtree()"/> is called on the reader and the subordinate reader
        /// is positionned on the current element. The <see cref="IDisposable.Dispose">Dispose</see>
        /// method must be called before reading again on this reader.
        /// </summary>
        /// <returns>The reader bound to the subordinated elements.</returns>
        ISubStructuredReader OpenSubReader();

        /// <summary>
        /// Reads an object from the storage.
        /// </summary>
        /// <param name="status">A <see cref="StandardReadStatus"/> that describes the result of the read (type and/or error occurence).</param>
        /// <returns>Deserialized object (can be null).</returns>
        /// <exception cref="Exception">Any exceptions related to read operations may be thrown by this method. BUT in any case, 
        /// the Xml reader head is forwarded allowing the caller to safely continue reading the stream, ignoring the exceptions.</exception>
        object ReadInlineObject( out StandardReadStatus status );

        /// <summary>
        /// Reads an object that has been written with <see cref="IStructuredWriter.WriteInlineObjectStructured"/> by providing 
        /// a <see cref="Type"/> or an existing object instance (both can not be null at the same time).
        /// <para>
        /// If both are provided, the type is used to lookup for a <see cref="IStructuredSerializer{T}"/> in the services and to delegate
        /// it the read if it exists. Otherwise, the object must implement <see cref="IStructuredSerializable"/>.
        /// </para>
        /// <para>
        /// If only the type is provided, see <see cref="StructuredReaderAndWriterExtension.ReadInlineObjectStructured(IStructuredReader, Type)"/>.
        /// </para>
        /// <para>
        /// If only the object is provided, see <see cref="StructuredReaderAndWriterExtension.ReadInlineObjectStructured(IStructuredReader, object)"/>.
        /// </para>
        /// </summary>
        /// <param name="type">Type of the object to read. When null, the type of the object is used. 
        /// If a <see cref="IStructuredSerializer{T}"/> is available in the services, it is used, 
        /// otherwise, the type offer a default construtor and implement <see cref="IStructuredSerializable"/>.</param>
        /// <param name="o">The object to read. When null, it will be created by the <see cref="IStructuredSerializer{T}"/> if it exists in te services 
        /// or by a call to <see cref="Activator.CreateInstance(Type)"/>.</param>
        /// <returns>Deserialized object (can be null).</returns>
        object ReadInlineObjectStructured( Type type, object o );

    }

}
