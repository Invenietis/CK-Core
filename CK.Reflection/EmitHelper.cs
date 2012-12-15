using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;

namespace CK.Reflection
{
    public class EmitHelper
    {
        /// <summary>
        /// Implements an abstract method as a no operation method.
        /// </summary>
        /// <param name="tB">The <see cref="TypeBuilder"/> for the new type.</param>
        /// <param name="method">The method to implement. It must be abstract or virtual otherwise an <see cref="ArgumentException"/> is thrown.</param>
        /// <param name="isVirtual">True to keep the method virtual. Defaults to false: the method is sealed.</param>
        public static void ImplementStubMethod( TypeBuilder tB, MethodInfo method, bool isVirtual = false )
        {
            if( tB == null ) throw new ArgumentNullException( "tB" );
            if( method == null ) throw new ArgumentNullException( "abstractMethod" );
            if( !method.IsAbstract && !method.IsVirtual ) throw new ArgumentException( "Method must be virtual or abstract.", "abstractMethod" );

            ParameterInfo[] parameters = method.GetParameters();
            Type[] parametersTypes = ReflectionHelper.CreateParametersType( parameters );
            Type returnType = method.ReturnType;

            MethodAttributes mA = method.Attributes & ~(MethodAttributes.Abstract | MethodAttributes.VtableLayoutMask);
            if( isVirtual ) mA |= MethodAttributes.Virtual;
            MethodBuilder m = tB.DefineMethod( method.Name, mA, returnType, parametersTypes );
            EmitEmptyImplementation( m, returnType, parameters );
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
                    Debug.Assert( param.ParameterType.IsByRef );
                    Type pType = param.ParameterType.GetElementType();
                    // Adds 1 to skip 'this' parameter.
                    gVM.LdArg( i + 1 );
                    if( pType.IsValueType )
                    {
                        gVM.Emit( OpCodes.Initobj, pType );
                    }
                    else
                    {
                        gVM.Emit( OpCodes.Ldnull );
                        gVM.Emit( OpCodes.Stind_Ref );
                    }
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
    }
}
