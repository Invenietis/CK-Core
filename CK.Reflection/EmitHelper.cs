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
            MethodBuilder m = tB.DefineMethod( method.Name, mA, returnType, parametersTypes );
            EmitEmptyImplementation( m, returnType, parameters );
            return m;
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
                if( returnType.IsValueType )
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
        /// <returns>The <see cref="PropertyBuilder"/> to enable, for instance, creation of custom attributes on the property.</returns>
        public static PropertyBuilder ImplementStubProperty( TypeBuilder tB, PropertyInfo property, bool isVirtual = false )
        {
            if( tB == null ) throw new ArgumentNullException( "tB" );
            if( property == null ) throw new ArgumentNullException( "property" );

            FieldBuilder backField = tB.DefineField( "_" + property.Name + property.GetHashCode(), property.PropertyType, FieldAttributes.Private );

            MethodInfo getMethod = property.GetGetMethod( true );
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
            MethodInfo setMethod = property.GetSetMethod( true );
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
