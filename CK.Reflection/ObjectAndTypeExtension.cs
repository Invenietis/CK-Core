#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Reflection\ObjectAndTypeExtension.cs) is part of CiviKey. 
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

#region Licence
/*----------------------------------------------------------------------------
 * Copyright (C) 2009 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *-----------------------------------------------------------------------------*/
//
// Modified by Olivier Spinelli. feb. 12, 2011
//
// - Changed the namespace.
// - Added overload with throwOnError (optional) parameter.
// - Removed XXXOrFail versions.
// - Integrated with CK.Reflection.Helper class methods (the ObjectAndTypeExtension class exposes only extension methods).
//
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace CK.Reflection
{
    /// <summary>
    /// Utility methods for common reflection tasks.
    /// Adapted from http://kennethxublogsource.googlecode.com/svn/trunk/CommonReflection/src/Common.Reflection/Reflections.cs
    /// by Kenneth Xu.
    /// </summary>
    public static class ObjectAndTypeExtension
    {
        /// <summary>
        /// Extension method to obtain a delegate of type <typeparamref name="TDelegate"/> that can be used to call the 
        /// static method with given method <paramref name="name"/> from given <paramref name="type"/>. The method signature must be compatible
        /// with the parameter and return type of <typeparamref name="TDelegate"/>.
        /// </summary>
        /// <typeparam name="TDelegate">Type of a .Net delegate.</typeparam>
        /// <param name="type">The type to locate the compatible method.</param>
        /// <param name="name">The name of the method.</param>
        /// <returns>A delegate of type <typeparamref name="TDelegate"/> or null when no matching method if found.</returns>
        public static TDelegate GetStaticInvoker<TDelegate>( this Type type, string name )
            where TDelegate : class
        {
            return GetStaticInvoker<TDelegate>( type, name, false );
        }

        /// <summary>
        /// Extension method to obtain a delegate of type <typeparamref name="TDelegate"/> that can be used to call the 
        /// static method with given method <paramref name="name"/> from given <paramref name="type"/>. The method signature must be compatible  
        /// with the parameter and return type of <typeparamref name="TDelegate"/>.
        /// </summary>
        /// <typeparam name="TDelegate">Type of a .Net delegate.</typeparam>
        /// <param name="type">The type to locate the compatible method.</param>
        /// <param name="name">The name of the method.</param>
        /// <param name="throwOnError">True to raise a <see cref="MissingMethodException"/> when not found.</param>
        /// <returns>A delegate of type <typeparamref name="TDelegate"/> or null if the method has not been found and <paramref name="throwOnError"/> is false.</returns>
        /// <exception name="MissingMethodException">When there is no matching method found and <paramref name="throwOnError"/> is true.</exception>
        public static TDelegate GetStaticInvoker<TDelegate>( this Type type, string name, bool throwOnError )
            where TDelegate : class
        {
            return new DelegateBuilder<TDelegate>( type, name, throwOnError, false ).CreateInvoker();
        }

        /// <summary>
        /// Extension method to obtain a delegate of type <typeparamref name="TDelegate"/> that can be used to call the 
        /// instance method with given method <paramref name="name"/> from given <paramref name="type"/>. The first parameter type of <c>TDelegate</c> 
        /// must be assignable to the given <paramref name="type"/>. The rest parameters and return type of <c>TDelegate</c> must be compatible with 
        /// the signature of the method.
        /// </summary>
        /// <typeparam name="TDelegate">Type of a .Net delegate.</typeparam>
        /// <param name="type">The type to locate the compatible method.</param>
        /// <param name="name">The name of the method.</param>
        /// <returns>A delegate of type <typeparamref name="TDelegate"/> or null when no matching method if found.</returns>
        public static TDelegate GetInstanceInvoker<TDelegate>( this Type type, string name )
            where TDelegate : class
        {
            return GetInstanceInvoker<TDelegate>( type, name, false );
        }

        /// <summary>
        /// Extension method to obtain a delegate of type <typeparamref name="TDelegate"/> that can be used to call the 
        /// instance method with given method <paramref name="name"/> from given <paramref name="type"/>. The first parameter type of <c>TDelegate</c> 
        /// must be assignable to the given <paramref name="type"/>. The rest parameters and return type of <c>TDelegate</c> must be compatible with 
        /// the signature of the method.
        /// </summary>
        /// <typeparam name="TDelegate">Type of a .Net delegate.</typeparam>
        /// <param name="type">The type to locate the compatible method.</param>
        /// <param name="name">The name of the method.</param>
        /// <param name="throwOnError">True to raise a <see cref="MissingMethodException"/> when not found.</param>
        /// <returns>A delegate of type <typeparamref name="TDelegate"/> or null when no matching method if found and <paramref name="throwOnError"/> is false.</returns>
        /// <exception name="MissingMethodException">When there is no matching method found and <paramref name="throwOnError"/> is true.</exception>
        public static TDelegate GetInstanceInvoker<TDelegate>( this Type type, string name, bool throwOnError )
            where TDelegate : class
        {
            return new DelegateBuilder<TDelegate>( type, name, throwOnError, true ).CreateInvoker();
        }

        /// <summary>
        /// Extension method to obtain a delegate of type <typeparamref name="TDelegate"/> that can be used to call the 
        /// instance method with given method <paramref name="name"/> on specific <paramref name="obj">object</paramref>. 
        /// The method signature must be compatible with the signature of <typeparamref name="TDelegate"/>.
        /// </summary>
        /// <typeparam name="TDelegate">Type of a .Net delegate.</typeparam>
        /// <param name="obj">The object instance to find the method.</param>
        /// <param name="name">The name of the method.</param>
        /// <returns>A delegate of type <typeparamref name="TDelegate"/> or null when no matching method if found.</returns>
        public static TDelegate GetInstanceInvoker<TDelegate>( this object obj, string name )
            where TDelegate : class
        {
            return GetInstanceInvoker<TDelegate>( obj, name, false );
        }

        /// <summary>
        /// Extension method to obtain a delegate of type <typeparamref name="TDelegate"/> that can be used to call the 
        /// instance method with given method <paramref name="name"/> on specific <paramref name="obj">object</paramref>. 
        /// The method signature must be compatible with the signature of <typeparamref name="TDelegate"/>.
        /// </summary>
        /// <typeparam name="TDelegate">Type of a .Net delegate.</typeparam>
        /// <param name="obj">The object instance to find the method.</param>
        /// <param name="name">The name of the method.</param>
        /// <param name="throwOnError">True to raise a <see cref="MissingMethodException"/> when not found.</param>
        /// <returns>A delegate of type <typeparamref name="TDelegate"/> or null when no matching method if found and <paramref name="throwOnError"/> is false.</returns>
        /// <exception name="MissingMethodException">When there is no matching method found and <paramref name="throwOnError"/> is true.</exception>
        public static TDelegate GetInstanceInvoker<TDelegate>( this object obj, string name, bool throwOnError )
            where TDelegate : class
        {
            return new DelegateBuilder<TDelegate>( obj, obj.GetType(), name, throwOnError ).CreateInvoker();
        }

        /// <summary>
        /// Extension method to obtain a delegate of type specified by parameter <typeparamref name="TDelegate"/> that can be used to make non virtual
        /// call to instance method with given method <paramref name="name"/> on given <paramref name="type"/>.
        /// The first parameter type of <c>TDelegate</c> must be assignable to the given <paramref name="type"/>.
        /// Remaining parameters and return type of <c>TDelegate</c> must be compatible with the signature of the method.
        /// </summary>
        /// <typeparam name="TDelegate">Type of a .Net delegate.</typeparam>
        /// <param name="type">The type to locate the compatible method.</param>
        /// <param name="name">The name of the method.</param>
        /// <returns>A delegate of type <typeparamref name="TDelegate"/> or null when no matching method if found.</returns>
        public static TDelegate GetNonVirtualInvoker<TDelegate>( this Type type, string name )
            where TDelegate : class
        {
            return GetNonVirtualInvoker<TDelegate>( type, name, false );
        }

        /// <summary>
        /// Extension method to obtain a delegate of type specified by parameter <typeparamref name="TDelegate"/> that can be used to make non virtual
        /// call to instance method with given method <paramref name="name"/> on given <paramref name="type"/>.
        /// The first parameter type of <c>TDelegate</c> must be assignable to the given <paramref name="type"/>.
        /// Remaining parameters and return type of <c>TDelegate</c> must be compatible with the signature of the method.
        /// </summary>
        /// <typeparam name="TDelegate">Type of a .Net delegate.</typeparam>
        /// <param name="type">The type to locate the compatible method.</param>
        /// <param name="name">The name of the method.</param>
        /// <param name="throwOnError">True to raise a <see cref="MissingMethodException"/> when not found.</param>
        /// <returns>A delegate of type <typeparamref name="TDelegate"/> or null when no matching method if found and <paramref name="throwOnError"/> is false.</returns>
        /// <exception name="MissingMethodException">When there is no matching method found and <paramref name="throwOnError"/> is true.</exception>
        public static TDelegate GetNonVirtualInvoker<TDelegate>( this Type type, string name, bool throwOnError )
            where TDelegate : class
        {
            return new DelegateBuilder<TDelegate>( type, name, throwOnError, true ).CreateInvoker( true );
        }

        /// <summary>
        /// Extension method to obtain a delegate of type specified by parameter <typeparamref name="TDelegate"/> that can be used to make non virtual
        /// call on the specific <paramref name="obj">object</paramref> to the instance method of given <paramref name="name"/> defined in the 
        /// given <paramref name="type"/> or its ancestor.
        /// The method signature must be compatible with the signature of <typeparamref name="TDelegate"/>.
        /// </summary>
        /// <typeparam name="TDelegate">Type of a .Net delegate.</typeparam>
        /// <param name="obj">The object instance to invoke the method.</param>
        /// <param name="type">The type to find the method.</param>
        /// <param name="name">The name of the method.</param>
        /// <returns>A delegate of type <typeparamref name="TDelegate"/> or null when no matching method if found.</returns>
        public static TDelegate GetNonVirtualInvoker<TDelegate>( this object obj, Type type, string name )
            where TDelegate : class
        {
            return GetNonVirtualInvoker<TDelegate>( obj, type, name, false );
        }

        /// <summary>
        /// Extension method to obtain a delegate of type specified by parameter <typeparamref name="TDelegate"/> that can be used to make non virtual
        /// call on the specific <paramref name="obj">object</paramref> to the instance method of given <paramref name="name"/> defined in the 
        /// given <paramref name="type"/> or its ancestor.
        /// The method signature must be compatible with the signature of <typeparamref name="TDelegate"/>.
        /// </summary>
        /// <typeparam name="TDelegate">Type of a .Net delegate.</typeparam>
        /// <param name="obj">The object instance to invoke the method.</param>
        /// <param name="type">The type to find the method.</param>
        /// <param name="name">The name of the method.</param>
        /// <param name="throwOnError">True to raise a <see cref="MissingMethodException"/> when not found.</param>
        /// <returns>A delegate of type <typeparamref name="TDelegate"/> or null when no matching method if found and <paramref name="throwOnError"/> is false.</returns>
        /// <exception name="MissingMethodException">When there is no matching method found and <paramref name="throwOnError"/> is true.</exception>
        public static TDelegate GetNonVirtualInvoker<TDelegate>( this object obj, Type type, string name, bool throwOnError )
            where TDelegate : class
        {
            return new DelegateBuilder<TDelegate>( obj, type, name, throwOnError ).CreateInvoker( true );
        }

        /// <summary>
        /// This is a more general purpose method to obtain a delegate of type specified by parameter <typeparamref name="TDelegate"/> that can 
        /// be used to call on the specific <paramref name="obj">object</paramref> to the method of given <paramref name="name"/> defined in the given 
        /// <paramref name="type"/> or its ancestor. The method signature must be compatible with the signature of <typeparamref name="TDelegate"/>.
        /// </summary>
        /// <typeparam name="TDelegate">Type of a .Net delegate.</typeparam>
        /// <param name="obj">The object instance to invoke the method or null for static methods and open instance methods.</param>
        /// <param name="type">The type to find the method.</param>
        /// <param name="name">The name of the method.</param>
        /// <param name="bindingAttr">A bitmask comprised of one or more <see cref="BindingFlags"/> that specify how the search is conducted. -or- Zero, to return null.</param>
        /// <param name="filter">The additional filter to include/exclude methods.</param>
        /// <param name="filterMessage">The description of the additional filter criteria that will be included in the exception message when no matching method found.</param>
        /// <param name="throwOnError">True to raise a <see cref="MissingMethodException"/> when not found.</param>
        /// <returns>A delegate of type <typeparamref name="TDelegate"/>.</returns>
        /// <exception name="MissingMethodException">When there is no matching method found.</exception>
        public static TDelegate GetInvoker<TDelegate>( object obj, Type type, string name, BindingFlags bindingAttr, Predicate<MethodInfo> filter, string filterMessage, bool throwOnError )
            where TDelegate : class
        {
            return new DelegateBuilder<TDelegate>( obj, type, name, throwOnError, bindingAttr )
            {
                MethodFilter = filter,
                MethodFilterMessage = filterMessage
            }.CreateInvoker();
        }


        static DynamicMethod CreateDynamicMethod( MethodInfo m )
        {
            var types = m.IsStatic ? ReflectionHelper.CreateParametersType( m.GetParameters() ) : ReflectionHelper.CreateParametersType( m.GetParameters(), m.DeclaringType );
            DynamicMethod dynamicMethod = new DynamicMethod( "NonVirtualInvoker_" + m.Name, m.ReturnType, types, m.DeclaringType );
            ILGenerator il = dynamicMethod.GetILGenerator();
            il.RepushActualParameters( true, types.Length );
            il.EmitCall( OpCodes.Call, m, null );
            il.Emit( OpCodes.Ret );
            return dynamicMethod;
        }

        private class DelegateBuilder<T> where T : class
        {
            #region Constants
            private const BindingFlags ALL_STATIC_METHOD =
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod;

            private const BindingFlags ALL_INSTANCE_METHOD =
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod;
            #endregion

            private readonly bool _throwOnError;
            private readonly string _methodName;
            private readonly Type _targetType;
            private readonly object _targetObject;
            private readonly BindingFlags _bindingAttr;
            private Type _returnType;
            private Type[] _parameterTypes;

            internal Predicate<MethodInfo> MethodFilter { get; set; }
            internal string MethodFilterMessage { get; set; }

            public DelegateBuilder( object targetObject, Type targetType, string methodName, bool throwOnError )
                : this( targetObject, targetType, methodName, throwOnError, ALL_INSTANCE_METHOD )
            {
            }

            public DelegateBuilder( Type targetType, string methodName, bool throwOnError, bool isInstanceMethod )
                : this( null, targetType, methodName, throwOnError, isInstanceMethod ? ALL_INSTANCE_METHOD : ALL_STATIC_METHOD )
            {
            }

            internal DelegateBuilder( object targetObject, Type targetType, string methodName, bool throwOnError, BindingFlags bindingAttr )
            {
                if( !typeof( Delegate ).IsAssignableFrom( typeof( T ) ) )
                {
                    throw new ArgumentException( "Expecting type parameter to be a Delegate type, but got " + typeof( T ).FullName );
                }
                _targetObject = targetObject;
                _targetType = targetType;
                _methodName = methodName;
                _throwOnError = throwOnError;
                _bindingAttr = bindingAttr;
            }

            public T CreateInvoker()
            {
                return CreateInvoker( false );
            }

            public T CreateInvoker( bool nonVirtual )
            {
                MethodInfo method;
                if( (method  = GetMethod()) == null ) return null;
                try
                {
                    if( nonVirtual && method.IsVirtual )
                    {
                        var dynamicMethod = CreateDynamicMethod( method );
                        return _targetObject == null ?
                            dynamicMethod.CreateDelegate( typeof( T ) ) as T :
                            dynamicMethod.CreateDelegate( typeof( T ), _targetObject ) as T;
                    }
                    return _targetObject == null ?
                        Delegate.CreateDelegate( typeof( T ), method ) as T :
                        Delegate.CreateDelegate( typeof( T ), _targetObject, method ) as T;
                }
                catch( ArgumentException ex )
                {
                    if( _throwOnError ) throw new MissingMethodException( BuildExceptionMessage(), ex );
                    return null;
                }
            }

            private MethodInfo GetMethod()
            {
                MethodInfo invokeMethod = typeof( T ).GetMethod( "Invoke" );
                ParameterInfo[] parameters = invokeMethod.GetParameters();
                _returnType = invokeMethod.ReturnType;

                bool instanceToStatic = (_targetObject == null && _bindingAttr == ALL_INSTANCE_METHOD);
                if( instanceToStatic )
                {
                    if( parameters.Length == 0 )
                    {
                        throw new InvalidOperationException( string.Format(
                            "Delegate {0} has no parameter. It is required to have at least one parameter that is assignable from target type.",
                            typeof( T ) ) );
                    }
                    Type instanceType = parameters[0].ParameterType;
                    if( !_targetType.IsAssignableFrom( instanceType ) )
                    {
                        if( _throwOnError )
                        {
                            throw new MissingMethodException( string.Format(
                                "Target type {0} is not assignable to the first parameter of delegate {1}.",
                                _targetType, instanceType ) );
                        }
                        return null;
                    }
                }
                _parameterTypes = instanceToStatic ? ReflectionHelper.CreateParametersType( parameters, 1 ) : ReflectionHelper.CreateParametersType( parameters );

                var method = _targetType.GetMethod( _methodName, _bindingAttr, null, _parameterTypes, null );
                var methodFilter = MethodFilter;
                if( method != null && methodFilter != null && !methodFilter( method ) )
                {
                    method = null;
                }
                if( method == null && _throwOnError )
                {
                    throw new MissingMethodException( BuildExceptionMessage() );
                }
                return method;
            }

            private string BuildExceptionMessage()
            {
                StringBuilder sb = new StringBuilder()
                    .Append( "No matching method found in the type " )
                    .Append( _targetType )
                    .Append( " for signature " )
                    .Append( _returnType ).Append( " " )
                    .Append( _methodName ).Append( "(" );
                if( _parameterTypes.Length > 0 )
                {
                    foreach( Type parameter in _parameterTypes )
                    {
                        sb.Append( parameter ).Append( ", " );
                    }
                    sb.Length -= 2;
                }
                sb.Append( ") with binding flags: " ).Append( _bindingAttr );
                if( MethodFilter != null )
                {
                    sb.Append( " with filter " ).Append( MethodFilterMessage ?? MethodFilter.ToString() );
                }
                sb.Append( "." );
                return sb.ToString();
            }
        }
    }
}
