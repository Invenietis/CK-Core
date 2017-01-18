#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Reflection\TypeBuilderExtension.cs) is part of CiviKey. 
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
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace CK.Reflection
{
    /// <summary>
    /// Provides extension methods on <see cref="TypeBuilder"/> class.
    /// </summary>
    public static class TypeBuilderExtension
    {
        /// <summary>
        /// Creates constructors that relay calls to public and protected constructors in the base class.
        /// </summary> 
        /// <param name="this">This <see cref="TypeBuilder"/>.</param>
        /// <param name="baseConstructorfilter">
        /// Optional predicate used to filter constructors that must be implemented and set its <see cref="MethodAttributes"/>. 
        /// When this function returns null, the constructor is not implemented, otherwise it can return the baseConstructor's Attribute.
        /// When null, all public and protected constructors are replicated with the same access (public or protected).
        /// </param>
        /// <param name="constructorAttributesFilter">
        /// Optional predicate used to filter constructors' attributes. 
        /// When null, all attributes are redefined.
        /// </param>
        /// <param name="parameterAttributesFilter">
        /// Optional predicate used to filter constructors' arguments' attributes. 
        /// When null, all parameters are redefined.
        /// </param>
        public static void DefinePassThroughConstructors( this TypeBuilder @this,
                                                            Func<ConstructorInfo,MethodAttributes?> baseConstructorfilter = null,
                                                            Func<ConstructorInfo, CustomAttributeData, bool> constructorAttributesFilter = null,
                                                            Func<ParameterInfo, CustomAttributeData, bool> parameterAttributesFilter = null )
        {
            Type baseType = @this.BaseType;
            foreach( var baseCtor in baseType.GetConstructors( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ) )
            {
                if( baseCtor.IsPrivate ) continue;
                MethodAttributes? newCtorAttr = baseCtor.Attributes;
                if( baseConstructorfilter != null && !(newCtorAttr = baseConstructorfilter( baseCtor )).HasValue ) continue;
                var parameters = baseCtor.GetParameters();
                if( parameters.Length == 0 ) @this.DefineDefaultConstructor( newCtorAttr.Value );
                else
                {
                    Type[] parameterTypes = ReflectionHelper.CreateParametersType( parameters );
                    // REVIEW: Type.EmptyTypes replaces GetRequiredCustomModifiers and GetOptionalCustomModifiers 
                    Type[][] requiredCustomModifiers =  parameters.Select( p => Type.EmptyTypes ).ToArray();
                    Type[][] optionalCustomModifiers = parameters.Select( p => Type.EmptyTypes ).ToArray();

                    var ctor = @this.DefineConstructor( newCtorAttr.Value, baseCtor.CallingConvention, parameterTypes, requiredCustomModifiers, optionalCustomModifiers );
                    for( var i = 0; i < parameters.Length; ++i )
                    {
                        ParameterInfo parameter = parameters[i];
                        ParameterBuilder pBuilder = ctor.DefineParameter( i + 1, parameter.Attributes, parameter.Name );
                        if( (parameter.Attributes & ParameterAttributes.HasDefault) != 0 )
                        {
                            pBuilder.SetConstant( parameter.DefaultValue );
                        }
                        if( parameterAttributesFilter != null )
                        {
                            ReflectionHelper.GenerateCustomAttributeBuilder( parameter.CustomAttributes, pBuilder.SetCustomAttribute, a => parameterAttributesFilter( parameter, a ) );
                        }
                        else
                        {
                            ReflectionHelper.GenerateCustomAttributeBuilder( parameter.CustomAttributes, pBuilder.SetCustomAttribute );
                        }
                    }
                    if( constructorAttributesFilter != null )
                    {
                        ReflectionHelper.GenerateCustomAttributeBuilder( baseCtor.CustomAttributes, ctor.SetCustomAttribute, a => constructorAttributesFilter( baseCtor, a ) );
                    }
                    else
                    {
                        ReflectionHelper.GenerateCustomAttributeBuilder( baseCtor.CustomAttributes, ctor.SetCustomAttribute );
                    }
                    var g = ctor.GetILGenerator();
                    g.RepushActualParameters( true, parameters.Length + 1 );
                    g.Emit( OpCodes.Call, baseCtor );
                    g.Emit( OpCodes.Ret );
                }
            }
        }

    }
}
