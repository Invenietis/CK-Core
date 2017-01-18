#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Reflection\EmitHelper.cs) is part of CiviKey. 
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
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;

namespace CK.Reflection
{
    /// <summary>
    /// Collection of helpers to emit dynamic code. 
    /// </summary>
    public static class EmitHelper
    {
        /// <summary>
        /// Implements a method as a no operation method. Method can be virtual, abstract or not.
        /// </summary>
        /// <param name="tB">The <see cref="TypeBuilder"/> for the new type.</param>
        /// <param name="method">The method to implement.</param>
        /// <param name="isVirtual">Defaults to false: the method is sealed. True to keep the method virtual. </param>
        /// <returns>The <see cref="MethodBuilder"/> to enable, for instance, creation of custom attributes on the method.</returns>
        public static MethodBuilder ImplementEmptyStubMethod( TypeBuilder tB, MethodInfo method, bool isVirtual = false )
        {
            if( tB == null ) throw new ArgumentNullException( "tB" );
            if( method == null ) throw new ArgumentNullException( "method" );

            ParameterInfo[] parameters = method.GetParameters();
            Type[] parametersTypes = ReflectionHelper.CreateParametersType( parameters );
            Type returnType = method.ReturnType;

            MethodAttributes mA = method.Attributes & ~(MethodAttributes.Abstract | MethodAttributes.VtableLayoutMask);
            if( isVirtual ) mA |= MethodAttributes.Virtual;
            MethodBuilder mB = tB.DefineMethod( method.Name, mA );
            if( method.ContainsGenericParameters )
            {
                int i = 0;

                Type[] genericArguments = method.GetGenericArguments();
                string[] names = genericArguments.Select( t => String.Format( "T{0}", i++ ) ).ToArray();

                var genericParameters = mB.DefineGenericParameters( names );
                for( i = 0; i < names.Length; ++i )
                {
                    var genericTypeArgument = genericArguments[i].GetTypeInfo();
                    GenericTypeParameterBuilder genericTypeBuilder = genericParameters[i];


                    genericTypeBuilder.SetGenericParameterAttributes( genericTypeArgument.GenericParameterAttributes );
                    genericTypeBuilder.SetInterfaceConstraints( genericTypeArgument.GetGenericParameterConstraints() );
                }
            }
            mB.SetReturnType( method.ReturnType );
            mB.SetParameters( ReflectionHelper.CreateParametersType( parameters ) );
            EmitEmptyImplementation( mB, returnType, parameters );
            return mB;
        }

        private static void EmitEmptyImplementation( MethodBuilder vM, Type returnType, ParameterInfo[] parameters )
        {
            ILGenerator gVM = vM.GetILGenerator();
            for( int i = 0; i < parameters.Length; ++i )
            {
                // DefineParameter use 0 for the return parameter.
                ParameterInfo param = parameters[i];
                vM.DefineParameter( i + 1, param.Attributes, param.Name );
                if( param.IsOut )
                {
                    Debug.Assert( param.ParameterType.IsByRef, "'Out' is just an attribute on 'by ref' parameters (unfortunate for covariance support)." );
                    gVM.StoreDefaultValueForOutParameter( param );
                }
            }
            if( returnType != typeof( void ) )
            {
                if( returnType.GetTypeInfo().IsValueType )
                {
                    LocalBuilder retValue = gVM.DeclareLocal( returnType );
                    gVM.Emit( OpCodes.Ldloca_S, retValue.LocalIndex );
                    gVM.Emit( OpCodes.Initobj, returnType );
                    gVM.LdLoc( retValue );
                }
                else
                {
                    gVM.Emit( OpCodes.Ldnull );
                }
            }
            gVM.Emit( OpCodes.Ret );
        }

        /// <summary>
        /// Implement a property with getter/setter that relies on a private backup field.
        /// This is useful only to provide a temporary implementation of abstract properties that would be generated in a second time (this does not 
        /// provide more than auto implemented properties available in C# 3.0 and later.
        /// </summary>
        /// <param name="tB">The <see cref="TypeBuilder"/> for the new type.</param>
        /// <param name="property">The property to implement.</param>
        /// <param name="isVirtual">Defaults to false: the method is sealed. True to keep the method virtual. </param>
        /// <param name="alwaysImplementSetter">When true a setter is implemented even if the <paramref name="property"/> has no setter.</param>
        /// <returns>The <see cref="PropertyBuilder"/> to enable, for instance, creation of custom attributes on the property.</returns>
        public static PropertyBuilder ImplementStubProperty( TypeBuilder tB, PropertyInfo property, bool isVirtual = false, bool alwaysImplementSetter = false )
        {
            if( tB == null ) throw new ArgumentNullException( "tB" );
            if( property == null ) throw new ArgumentNullException( "property" );

            FieldBuilder backField = tB.DefineField( "_" + property.Name + property.GetHashCode(), property.PropertyType, FieldAttributes.Private );

            MethodInfo getMethod = property.GetMethod;
            MethodBuilder mGet = null;
            if( getMethod != null )
            {
                MethodAttributes mA = getMethod.Attributes & ~(MethodAttributes.Abstract | MethodAttributes.VtableLayoutMask);
                if( isVirtual ) mA |= MethodAttributes.Virtual;
                mGet = tB.DefineMethod( getMethod.Name, mA, property.PropertyType, Type.EmptyTypes );
                ILGenerator g = mGet.GetILGenerator();
                g.LdArg( 0 );
                g.Emit( OpCodes.Ldfld, backField );
                g.Emit( OpCodes.Ret );
            }
            MethodInfo setMethod = property.SetMethod;
            if( setMethod == null && alwaysImplementSetter )
            {
                // We only use Attributes and Name from the method info.
                setMethod = getMethod;
            }
            MethodBuilder mSet = null;
            if( setMethod != null )
            {
                MethodAttributes mA = setMethod.Attributes & ~(MethodAttributes.Abstract | MethodAttributes.VtableLayoutMask);
                if( isVirtual ) mA |= MethodAttributes.Virtual;
                mSet = tB.DefineMethod( setMethod.Name, mA, typeof( void ), new[] { property.PropertyType } );
                ILGenerator g = mSet.GetILGenerator();
                g.LdArg( 0 );
                g.LdArg( 1 );
                g.Emit( OpCodes.Stfld, backField );
                g.Emit( OpCodes.Ret );
            }

            PropertyBuilder p = tB.DefineProperty( property.Name, property.Attributes, property.PropertyType, Type.EmptyTypes );
            if( mGet != null ) p.SetGetMethod( mGet );
            if( mSet != null ) p.SetSetMethod( mSet );
            return p;
        }
    }
}
