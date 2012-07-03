#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Storage\IStructuredWriter.cs) is part of CiviKey. 
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
    /// The interface to write information into a very simple, potentially hybrid, structured storage. 
    /// The heart of the storage is an Xml stream exposed here as the <see cref="P:XmlWriter"/> property. 
    /// It can be easily extended since it implements <see cref="IServiceProvider"/> and contains a <see cref="P:ServiceContainer"/>.
    /// </summary>
    /// <remarks>
    /// Since <see cref="IDisposable"/> is implemented, you must call <see cref="IDisposable.Dispose"/> when
    /// you are finished writing.
    /// </remarks>
    public interface IStructuredWriter : IServiceProvider, IDisposable
    {
        /// <summary>
        /// Fires each time an object has been written.
        /// </summary>
        event EventHandler<ObjectWriteExDataEventArgs> ObjectWriteExData;

        /// <summary>
        /// Gets the current <see cref="IStructuredWriter"/>. 
        /// It may be this writer or any subordinated writer created by <see cref="OpenSubWriter"/>.
        /// </summary>
        /// <returns>The current writer.</returns>
        IStructuredWriter Current { get; }

        /// <summary>
        /// Returns a <see cref="ISubStructuredWriter"/> that scopes the writer. 
        /// </summary>
        /// <returns>A writer that becomes the <see cref="Current"/> one and must be disposed once its scope must be closed.</returns>
        ISubStructuredWriter OpenSubWriter();

        /// <summary>
        /// Writes the given object into the structured output. Type information required to restore 
        /// the object with <see cref="IStructuredReader.ReadInlineObject"/> are injected.
        /// The current Xml element must be opened and it will be closed by this method.
        /// </summary>
        /// <param name="o">The object to write. May be null.</param>
        void WriteInlineObject( object o );

        /// <summary>
        /// Writes the given object either via its <see cref="IStructuredSerializable"/> implementation
        /// or thanks to a <see cref="IStructuredSerializer{T}"/> into the structured output.
        /// </summary>
        /// <param name="o">The object to write. May be null.</param>
        /// <remarks>
        /// This method does not write any type information. Since its type name is not written, it can not be read
        /// back with <see cref="IStructuredReader.ReadInlineObject"/> method but only
        /// with <see cref="IStructuredReader.ReadInlineObjectStructured"/>
        /// </remarks>
        void WriteInlineObjectStructured( object o );

        /// <summary>
        /// The associated <see cref="XmlWriter"/> wrapped by this <see cref="IStructuredWriter"/>.
        /// </summary>
        XmlWriter Xml { get; }

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> to which this reader is ultimately bound.
        /// </summary>
        /// <remarks>
        /// This is the root provider: any <see cref="ISubStructuredReader"/> subordinated
        /// to a <see cref="IStructuredReader"/> are bound to the same base service provider.
        /// </remarks>
        IServiceProvider BaseServiceProvider { get; }

        /// <summary>
        /// Gets the <see cref="ISimpleServiceContainer"/> for this writer.
        /// </summary>
        ISimpleServiceContainer ServiceContainer { get; }

    }
}
