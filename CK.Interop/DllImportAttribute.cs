#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Interop\DllImportAttribute.cs) is part of CiviKey. 
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
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CK.Interop
{
    /// <summary>
    /// This attribute duplicates the <see cref="System.Runtime.InteropServices.DllImportAttribute"/> fields and adds
    /// three optionals fields: <see cref="DllNameGeneric"/>, <see cref="DllName32"/> and <see cref="DllName64"/> that enables to specify the 
    /// mapped dll.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method, AllowMultiple = false )]
    public class DllImportAttribute : Attribute
    {
        /// <summary>
        /// Optional: overrides, if not null nor empty, the <see cref="NativeDllAttribute"/> <see cref="NativeDllAttribute.DefaultDllNameGeneric"/> property.
        /// </summary>
        public string DllNameGeneric;

        /// <summary>
        /// Optional: overrides, if not null nor empty, the <see cref="NativeDllAttribute"/> <see cref="NativeDllAttribute.DefaultDllName32"/> property.
        /// </summary>
        public string DllName32;

        /// <summary>
        /// Optional: overrides, if not null nor empty, the <see cref="NativeDllAttribute"/> <see cref="NativeDllAttribute.DefaultDllName64"/> property.
        /// </summary>
        public string DllName64;

        /// <summary>
        /// Enables or disables best-fit mapping behavior when converting Unicode characters
        /// to ANSI characters. Defaults to true.
        /// </summary>
        public bool BestFitMapping = true;

        /// <summary>
        /// Indicates the calling convention of an entry point. Defaults to <see cref="CallingConvention.Winapi"/>.
        /// </summary>
        public CallingConvention CallingConvention = CallingConvention.Winapi;

        /// <summary>
        /// Indicates how to marshal string parameters to the method and controls name mangling.
        /// Defaults to <see cref="CharSet.Auto"/> (and not to <see cref="CharSet.Ansi"/> that is the default of c# and VB language).
        /// </summary>
        public CharSet CharSet = CharSet.Auto;

        /// <summary>
        /// Indicates the name or ordinal of the DLL entry point to be called.
        /// Defaults to the name of the method.
        /// </summary>
        public string EntryPoint;

        /// <summary>
        /// Indicates the name or ordinal of the DLL entry point to be called when running in 32 bits environment.
        /// Defaults to <see cref="EntryPoint"/>.
        /// </summary>
        public string EntryPoint32;

        /// <summary>
        /// Indicates the name or ordinal of the DLL entry point to be called when running in 64 bits environment.
        /// Defaults to <see cref="EntryPoint"/>.
        /// </summary>
        public string EntryPoint64;

        /// <summary>
        /// Controls whether the <see cref="CharSet"/> field causes the common language 
        /// runtime to search an unmanaged DLL for entry-point names other than the one specified.
        /// </summary>
        public bool ExactSpelling;

        /// <summary>
        /// Indicates whether unmanaged methods that have HRESULT or retval return values
        /// are directly translated (true, the default) or whether HRESULT or retval return values are automatically
        /// converted to exceptions (when false). Defaults to true.
        /// </summary>
        public bool PreserveSig = true;
        
        /// <summary>
        /// Indicates whether the callee calls the SetLastError Win32 API function before
        /// returning from the attributed method. Defaults to false.
        /// </summary>
        public bool SetLastError;

        /// <summary>
        /// Enables or disables the throwing of an exception on an unmappable Unicode
        /// character that is converted to an ANSI "?" character. Defaults to false.
        /// </summary>
        public bool ThrowOnUnmappableChar;


        internal string GetBestDllName( string defaultName )
        {
            if( IntPtr.Size == 4 )
            {
                if( !String.IsNullOrEmpty( DllName32 ) ) return DllName32;
            }
            else
            {
                if( !String.IsNullOrEmpty( DllName64 ) ) return DllName64;
            }
            if( !String.IsNullOrEmpty( DllNameGeneric ) ) return DllNameGeneric;
            return defaultName;
        }

        internal string GetBestEntryPoint( string defaultName )
        {
            if( IntPtr.Size == 4 )
            {
                if( !String.IsNullOrEmpty( EntryPoint32 ) ) return EntryPoint32;
            }
            else
            {
                if( !String.IsNullOrEmpty( EntryPoint64 ) ) return EntryPoint64;
            }
            if( !String.IsNullOrEmpty( EntryPoint ) ) return EntryPoint;
            return defaultName;
        }

    }
}
