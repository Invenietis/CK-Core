#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Interop\PInvoker.cs) is part of CiviKey. 
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
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.InteropServices;
using CK.Reflection;

namespace CK.Interop
{
    /// <summary>
    /// Static class that dynamically handles the <see cref="CK.Interop.DllImportAttribute">CK.Interop.DllImportAttribute</see>.
    /// </summary>
    static public class PInvoker
    {
        static ModuleBuilder _moduleBuilder;
        static Dictionary<Type,object> _cache;
        static object _lock = new object();
        static int _countName;

        static readonly ConstructorInfo _impAttrCtor;
        static readonly FieldInfo[] _impAttrFields;

        static PInvoker()
        {
            // The DLLImportAttribute type.
            Type iType = typeof( System.Runtime.InteropServices.DllImportAttribute );

            // Create a ConstuctorInfo for the DLLImportAttribute, specifying
            // the constructor that takes a string argument.
            _impAttrCtor = iType.GetConstructor( new Type[] { typeof( string ) } );
            // Create an array containing the fields of the DLLImportAttribute class.
            _impAttrFields = new FieldInfo[] 
            {
                iType.GetField("EntryPoint"),
                iType.GetField("ExactSpelling"),
                iType.GetField("PreserveSig"),
                iType.GetField("SetLastError"),
                iType.GetField("CallingConvention"),
                iType.GetField("CharSet"),
                iType.GetField("BestFitMapping"),
                iType.GetField("ThrowOnUnmappableChar")
            };
        }

        /// <summary>
        /// Gets an automatically generated implementation for an interface that must be marked with a <see cref="CK.Interop.NativeDllAttribute"/>.
        /// </summary>
        /// <typeparam name="T">Interface that must be obtained.</typeparam>
        /// <returns>Implementation that can be used to call the native functions.</returns>
        public static T GetInvoker<T>() where T : class
        {
            lock( _lock )
            {
                object result = null;
                if( _moduleBuilder == null )
                {
                    AssemblyName assemblyName = new AssemblyName( "CKPInvokeAssembly" );
                    assemblyName.Version = new Version( 1, 0, 0, 0 );
                    AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly( assemblyName, AssemblyBuilderAccess.Run );
                    _moduleBuilder = assemblyBuilder.DefineDynamicModule( "Module" );
                    _cache = new Dictionary<Type, object>();
                }
                else if( _cache.TryGetValue( typeof( T ), out result ) ) return (T)result;
                result = DoCreate( _moduleBuilder, typeof( T ) );
                _cache.Add( typeof( T ), result );
                return (T)result;
            }
        }

        static object DoCreate( ModuleBuilder mb, Type t )
        {
            NativeDllAttribute[] dllAttributes = (NativeDllAttribute[])t.GetCustomAttributes( typeof( NativeDllAttribute ), false );
            if( dllAttributes.Length == 0 ) throw new Exception( "Interface must be marked with CK.Interop.NativeDllAttribute." );
            string defaultDllName = dllAttributes[0].GetBestDefaultName();

            TypeBuilder type = mb.DefineType( String.Format( "CK_Dyn_{0}_{1}", t.Name, ++_countName ) );
            type.AddInterfaceImplementation( t );

            foreach( MethodInfo m in t.GetMethods() )
            {
                DllImportAttribute a = (DllImportAttribute)Attribute.GetCustomAttribute( m, typeof( DllImportAttribute ), false );
                if( a != null )
                {
                    // DllName
                    string dllName = a.GetBestDllName( defaultDllName );

                    // Extract parameters type.
                    Type[] parameterTypes = ReflectionHelper.CreateParametersType( m.GetParameters() );

                    MethodBuilder nativeMethod = CreateNative( type, dllName, m, a, parameterTypes );

                    MethodBuilder method = type.DefineMethod( m.Name,
                                                                MethodAttributes.Public | MethodAttributes.Virtual,
                                                                m.ReturnType,
                                                                parameterTypes );
                    ILGenerator g = method.GetILGenerator();
                    g.RepushActualParameters( false, parameterTypes.Length );
                    g.EmitCall( OpCodes.Call, nativeMethod, null );
                    g.Emit( OpCodes.Ret );
                }
            }
            Type finalType = type.CreateType();
            return Activator.CreateInstance( finalType );

        }

        private static MethodBuilder CreateNative( TypeBuilder tb, string dllName, MethodInfo m, DllImportAttribute a, Type[] parameterTypes )
        {
            // This code is from msdn itself (the example associated to TypeBuilder.DefinePInvokeMethod).
            MethodBuilder native = tb.DefineMethod( "_st_" + m.Name, MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig, m.ReturnType, parameterTypes );

            // Create an array of values to assign to the fields of the
            // DLLImportAttribute class, when constructing an attribute
            // for the native function.
            object[] fieldvalues = new object[]
            {
                a.GetBestEntryPoint( m.Name ),
                a.ExactSpelling,
                a.PreserveSig,
                a.SetLastError,
                a.CallingConvention,
                a.CharSet,
                a.BestFitMapping,
                a.ThrowOnUnmappableChar
            };

            // Construct a DLLImportAttribute.
            CustomAttributeBuilder attr = new CustomAttributeBuilder( _impAttrCtor, new object[] { dllName }, _impAttrFields, fieldvalues );

            // Apply the DLLCustomAttribute to the PInvoke method.
            native.SetCustomAttribute( attr );

            // The PInvoke method does not have a method body.
            return native;
        }


    }
}
