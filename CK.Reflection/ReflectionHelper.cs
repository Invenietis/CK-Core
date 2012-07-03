#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Reflection\ReflectionHelper.cs) is part of CiviKey. 
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
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Linq.Expressions;

namespace CK.Reflection
{
    /// <summary>
    /// Reflection related helpers methods that can not (really) be defined as extension methods.
    /// </summary>
    static public class ReflectionHelper
    {
        /// <summary>
        /// Describes the behavior of <see cref="CreateSetter"/> methods when no setter exists 
        /// on the property.
        /// </summary>
        public enum CreateInvalidSetterOption
        {
            /// <summary>
            /// Throws an <see cref="InvalidOperationException"/>. This is the default.
            /// </summary>
            ThrowException,
            /// <summary>
            /// Returns a null action delegate.
            /// </summary>
            NullAction,
            /// <summary>
            /// Returns a void action (an action that does nothing).
            /// </summary>
            VoidAction
        }

        /// <summary>
        /// Retrieves a <see cref="PropertyInfo"/> from a lambda function based on an instance of the holder.
        /// </summary>
        /// <typeparam name="THolder">Property holder type (will be inferred by the compiler).</typeparam>
        /// <typeparam name="TProperty">Property type (will be inferred by the compiler).</typeparam>
        /// <param name="source">An instance of the <typeparamref name="THolder"/>.</param>
        /// <param name="propertyLambda">A lambda function that selects the property.</param>
        /// <returns>Corresponding property information.</returns>
        public static PropertyInfo GetPropertyInfo<THolder, TProperty>( THolder source, Expression<Func<THolder, TProperty>> propertyLambda )
        {
            return DoGetPropertyInfo( propertyLambda );
        }

        /// <summary>
        /// Creates a setter fo a property. 
        /// </summary>
        /// <typeparam name="THolder">Property holder type (will be inferred by the compiler).</typeparam>
        /// <typeparam name="TProperty">Property type (will be inferred by the compiler).</typeparam>
        /// <param name="source">An instance of the <typeparamref name="THolder"/>.</param>
        /// <param name="propertyLambda">A lambda function that selects the property.</param>
        /// <returns>An action that takes an holder instance and the value to set.</returns>
        public static Action<THolder, TProperty> CreateSetter<THolder, TProperty>( THolder source, Expression<Func<THolder, TProperty>> propertyLambda, CreateInvalidSetterOption o = CreateInvalidSetterOption.ThrowException )
        {
            return CreateSetter<THolder,TProperty>( DoGetPropertyInfo( propertyLambda ), o );
        }

        /// <summary>
        /// Retrieves a <see cref="PropertyInfo"/> from a lambda function without requiring an instance of the holder 
        /// object and without any constraint for the type of the property.
        /// </summary>
        /// <typeparam name="THolder">Property holder type.</typeparam>
        /// <param name="propertyLambda">A lambda function that selects the property as an object.</param>
        /// <returns>Corresponding property information.</returns>
        public static PropertyInfo GetPropertyInfo<THolder>( Expression<Func<THolder, object>> propertyLambda )
        {
            return DoGetPropertyInfo( propertyLambda );
        }

        /// <summary>
        /// Retrieves a <see cref="PropertyInfo"/> from a lambda function.
        /// </summary>
        /// <typeparam name="THolder">Property holder type.</typeparam>
        /// <typeparam name="TProperty">Property type.</typeparam>
        /// <param name="propertyLambda">A lambda function that selects the property.</param>
        /// <returns>Corresponding property information.</returns>
        public static PropertyInfo GetPropertyInfo<THolder, TProperty>( Expression<Func<THolder, TProperty>> propertyLambda )
        {
            return DoGetPropertyInfo( propertyLambda );
        }

        /// <summary>
        /// Creates a setter fo a property. 
        /// </summary>
        /// <typeparam name="THolder">Property holder type.</typeparam>
        /// <typeparam name="TProperty">Property type.</typeparam>
        /// <param name="propertyLambda">A lambda function that selects the property.</param>
        /// <returns>An action that takes an holder instance and the value to set.</returns>
        public static Action<THolder, TProperty> CreateSetter<THolder, TProperty>( Expression<Func<THolder, TProperty>> propertyLambda, CreateInvalidSetterOption o = CreateInvalidSetterOption.ThrowException )
        {
            return CreateSetter<THolder, TProperty>( DoGetPropertyInfo( propertyLambda ), o );
        }

        /// <summary>
        /// Retrieves a <see cref="PropertyInfo"/> from a parameterless lambda function: a closure is actually required
        /// and the compiler generates one automatically.
        /// </summary>
        /// <typeparam name="TProperty">Property type.</typeparam>
        /// <param name="propertyLambda">A lambda function that selects a property (from the current syntaxic context).</param>
        /// <returns>Corresponding property information.</returns>
        public static PropertyInfo GetPropertyInfo<TProperty>( Expression<Func<TProperty>> propertyLambda )
        {
            return DoGetPropertyInfo( propertyLambda );
        }

        private static PropertyInfo DoGetPropertyInfo( LambdaExpression propertyLambda )
        {
            Expression exp = propertyLambda.Body;
            MemberExpression member = exp as MemberExpression;
            if( member == null
                && (exp.NodeType == ExpressionType.Convert || exp.NodeType == ExpressionType.ConvertChecked) )
            {
                member = ((UnaryExpression)exp).Operand as MemberExpression;
            }
            PropertyInfo propInfo = member != null ? member.Member as PropertyInfo : null;
            if( propInfo == null )
                throw new ArgumentException( string.Format( "Expression '{0}' must refer to a property.", propertyLambda.ToString() ) );
            return propInfo;
        }

        private static Action<THolder, TProperty> CreateSetter<THolder, TProperty>( PropertyInfo property, CreateInvalidSetterOption o )
        {
            var holderType = Expression.Parameter( typeof( THolder ), "e" );
            var propType = Expression.Parameter( typeof( TProperty ), "v" );
            MethodInfo s = property.GetSetMethod();
            if( s == null )
            {
                if( o == CreateInvalidSetterOption.ThrowException ) throw new InvalidOperationException( string.Format( "Property '{0}' has no setter.", property.Name ) );
                if( o == CreateInvalidSetterOption.NullAction ) return null;
                return VoidAction;
            }
            return (Action<THolder, TProperty>)Delegate.CreateDelegate( typeof( Action<THolder, TProperty> ), s );
        }

        static void VoidAction<T1, T2>( T1 o1, T2 o2 )
        {
        }


        /// <summary>
        /// Creates an array of type of a method parameters.
        /// </summary>
        /// <param name="parametersInfo">Parameters from which type must be extracted.</param>
        /// <param name="startIndex">The zero-based starting parameter position.</param>
        /// <returns>Parameters' types.</returns>
        public static Type[] CreateParametersType( ParameterInfo[] parametersInfo, int startIndex )
        {
            if( parametersInfo == null ) throw new ArgumentNullException( "parametersInfo" );
            int len = parametersInfo.Length;
            int lenT = len - startIndex;
            if( startIndex < 0 || lenT < 0 ) throw new ArgumentOutOfRangeException( "startIndex" );
            Type[] parameters = new Type[lenT];
            while( --lenT >= 0 ) parameters[lenT] = parametersInfo[--len].ParameterType;
            return parameters;
        }

        /// <summary>
        /// Creates an array of type of a method parameters.
        /// </summary>
        /// <param name="parametersInfo">Parameters from which type must be extracted.</param>
        /// <returns>Parameters' types.</returns>
        /// <remarks>
        /// Implementation is faster (and more simple?) than using a linq select...
        /// </remarks>
        public static Type[] CreateParametersType( ParameterInfo[] parametersInfo )
        {
            if( parametersInfo == null ) throw new ArgumentNullException( "parametersInfo" );
            int len = parametersInfo.Length;
            Type[] parameters = new Type[len];
            while( --len >= 0 ) parameters[len] = parametersInfo[len].ParameterType;
            return parameters;
        }

        /// <summary>
        /// Creates an array of type of a method parameters.
        /// </summary>
        /// <param name="m">The method for which parameter types must be extracted.</param>
        /// <param name="typeToPrepend">Type that must be injected as the fist type in the resulting array 
        /// (typically the <see cref="MemberInfo.DeclaringType">declaring type of the method</see> - the this parameter).</param>
        /// <returns>Parameters' types.</returns>
        /// <remarks>
        /// Implementation is faster (and more simple?) than using a linq select...
        /// </remarks>
        public static Type[] CreateParametersType( ParameterInfo[] parametersInfo, Type typeToPrepend )
        {
            if( parametersInfo == null ) throw new ArgumentNullException( "parametersInfo" );
            if( typeToPrepend == null ) throw new ArgumentNullException( "typeToPrepend" );
            int len = parametersInfo.Length;
            Type[] parameters = new Type[len + 1];
            parameters[0] = typeToPrepend;
            while( len > 0 ) parameters[len] = parametersInfo[--len].ParameterType;
            return parameters;
        }

        /// <summary>
        /// Gets all methods (including inherited methods and methods with special names like get_XXX 
        /// and others add_XXX) of the given interface type.
        /// </summary>
        /// <param name="interfaceType">Type to process, must be an interface.</param>
        /// <returns><see cref="IEnumerable"/> of <see cref="MethodInfo"/>.</returns>
        public static IEnumerable<MethodInfo> GetFlattenMethods( Type interfaceType )
        {
            return GetFlattenMembers( interfaceType, type => type.GetMethods() );
        }

        /// <summary>
        /// Gets all properties (including inherited properties) of the given interface type.
        /// </summary>
        /// <param name="interfaceType">Type to process, must be an interface.</param>
        /// <returns><see cref="IEnumerable"/> of <see cref="PropertyInfo"/>.</returns>
        public static IEnumerable<PropertyInfo> GetFlattenProperties( Type interfaceType )
        {
            return GetFlattenMembers( interfaceType, type => type.GetProperties() );
        }

        /// <summary>
        /// Gets all events (including inherited events) of the given interface type.
        /// </summary>
        /// <param name="interfaceType">Type to process, must be an interface.</param>
        /// <returns><see cref="IEnumerable"/> of <see cref="EventInfo"/>.</returns>
        public static IEnumerable<EventInfo> GetFlattenEvents( Type interfaceType )
        {
            return GetFlattenMembers( interfaceType, type => type.GetEvents() );
        }

        /// <summary>
        /// Gets an <see cref="IEnumerable{T}"/> that contains elements returned by the <paramref name="getFunction"/>.
        /// </summary>
        /// <typeparam name="T">Type of the element that you're looking for. <see cref="MethodInfo"/> for example.</typeparam>
        /// <param name="interfaceType">Type to process, it must be an interface.</param>
        /// <param name="getFunction">Function that takes a type and return an <see cref="IEnumerable{T}"/>, a possible implementation can be the lambda <c>t => t.GetMethods()</c></param>
        /// <returns><see cref="IEnumerable{T}"/> that contains elements returned by the <paramref name="getFunction"/>.</returns>
        public static IEnumerable<T> GetFlattenMembers<T>( Type interfaceType, Func<Type, IEnumerable<T>> getFunction )
        {
            if( interfaceType == null ) throw new ArgumentNullException( "interfaceType" );
            if( !interfaceType.IsInterface ) throw new ArgumentException( R.InterfaceTypeExpected, "interfaceType" );
            
            foreach( var item in getFunction( interfaceType ) )
                yield return item;
           
            foreach( var type in interfaceType.GetInterfaces() )
                foreach( var item in getFunction( type ) )
                    yield return item;
        }


        /// <summary>
        /// Checks whether a type is "above" (or "more general than") another one. 
        /// This check uses <see cref="Type.IsAssignableFrom"/> when the main type is not generic. 
        /// For generics (be it an interface or a class), inheritance relations are used recursively on 
        /// generic parameters: the "is assignable from" semantics is lost in favor of a more 
        /// relaxed relation, for instance <c>IEnumerable[ICollection[ValueType]]</c> "Covariant matches" with <c>IEnumerable[IList[int]]</c>.
        /// </summary>
        /// <param name="mainType">The "main type" (the one that must be "more general than" <paramref name="toMatch"/>).</param>
        /// <param name="toMatch">The type to match.</param>
        /// <returns>True if <paramref name="mainType"/> is "above" <paramref name="toMatch"/>.</returns>
        public static bool CovariantMatch( Type mainType, Type toMatch )
        {
            // If our main type is a not generic: it is an interface or a class without any type parameters.
            // We only have to test if our main type is assignable from the type to match.
            if( !mainType.IsGenericType ) return mainType.IsAssignableFrom( toMatch );

            // Our main type is a generic type: it has parameter types.
            Type[] mainTypeArgs = mainType.GetGenericArguments();

            // We rely on the generic type definition (the Gen<,,> type).
            Type genDef = mainType.GetGenericTypeDefinition();
            // If this generic is an interface, we try to find the generic interface in all the interfaces.
            if( mainType.IsInterface )
            {
                var compatibles = toMatch.GetInterfaces().Where( t => t.IsGenericType && t.GetGenericTypeDefinition() == genDef );
                return compatibles.Any( c => CovariantMatch( mainTypeArgs, c.GetGenericArguments() ) );
            }
            // If this generic is a class, we try to find the generic class in the inheritance path.
            while( toMatch != null )
            {
                if( toMatch.IsGenericType && toMatch.GetGenericTypeDefinition() == genDef )
                {
                    return CovariantMatch( mainTypeArgs, toMatch.GetGenericArguments() );
                }
                toMatch = toMatch.BaseType;
            }
            return false;
        }

        static bool CovariantMatch( Type[] mainType, Type[] toMatch )
        {
            int len = mainType.Length;
            if( len != toMatch.Length ) return false;
            while( --len >= 0 )
            {
                if( !CovariantMatch( mainType[len], toMatch[len] ) ) return false;
            }
            return true;
        }
    }
}
