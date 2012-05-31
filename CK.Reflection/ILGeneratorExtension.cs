#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Reflection\ILGeneratorExtension.cs) is part of CiviKey. 
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
using System.Diagnostics;

namespace CK.Reflection
{
    /// <summary>
    /// Provides extension methods on <see cref="ILGenerator"/> class.
    /// </summary>
    public static class ILGeneratorExtension
    {
        /// <summary>
        /// Emits the optimal IL to push the actual parameter values on the stack (<see cref="OpCodes.Ldarg_0"/>... <see cref="OpCodes.Ldarg"/>).
        /// </summary>
        /// <param name="g">This <see cref="ILGenerator"/> object.</param>
        /// <param name="startAtArgument0">False to skip the very first argument: for a method instance Arg0 is the 'this' object (see <see cref="System.Reflection.CallingConventions"/>) HasThis and ExplicitThis).</param>
        /// <param name="count">Number of parameters to push.</param>
        public static void RepushActualParameters( this ILGenerator g, bool startAtArgument0, int count )
        {
            if( count <= 0 ) return;
            if( startAtArgument0 )
            {
                g.Emit( OpCodes.Ldarg_0 );
                --count;
            }
            if( count > 0 )
            {
                g.Emit( OpCodes.Ldarg_1 );
                if( count > 1 )
                {
                    g.Emit( OpCodes.Ldarg_2 );
                    if( count > 2 )
                    {
                        g.Emit( OpCodes.Ldarg_3 );
                        if( count > 3 )
                        {
                            for( int iParam = 4; iParam <= Math.Min( count, 255 ); ++iParam )
                            {
                                g.Emit( OpCodes.Ldarg_S, (byte)iParam );
                            }
                            if( count > 255 )
                            {
                                for( int iParam = 256; iParam <= count; ++iParam )
                                {
                                    g.Emit( OpCodes.Ldarg, (short)iParam );
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Emits the IL to push (<see cref="OpCodes.Ldloc"/>) the given local on top of the stack.
        /// </summary>
        /// <param name="g">This <see cref="ILGenerator"/> object.</param>
        /// <param name="local">The local variable to push.</param>
        public static void LdLoc( this ILGenerator g, LocalBuilder local )
        {
            int i = local.LocalIndex;
            if( i == 0 ) g.Emit( OpCodes.Ldloc_0 );
            else if( i == 1 ) g.Emit( OpCodes.Ldloc_1 );
            else if( i == 2 ) g.Emit( OpCodes.Ldloc_2 );
            else if( i == 3 ) g.Emit( OpCodes.Ldloc_3 );
            else if( i < 255 ) g.Emit( OpCodes.Ldloc_S, (byte)i );
            else g.Emit( OpCodes.Ldloc, (short)i );
        }

        /// <summary>
        /// Emits the IL to pop (<see cref="OpCodes.Stloc"/>) the top of the stack into a local variable.
        /// </summary>
        /// <param name="g">This <see cref="ILGenerator"/> object.</param>
        /// <param name="local">The local variable to pop.</param>
        public static void StLoc( this ILGenerator g, LocalBuilder local )
        {
            int i = local.LocalIndex;
            if( i == 0 ) g.Emit( OpCodes.Stloc_0 );
            else if( i == 1 ) g.Emit( OpCodes.Stloc_1 );
            else if( i == 2 ) g.Emit( OpCodes.Stloc_2 );
            else if( i == 3 ) g.Emit( OpCodes.Stloc_3 );
            else if( i < 255 ) g.Emit( OpCodes.Stloc_S, (byte)i );
            else g.Emit( OpCodes.Stloc, (short)i );
        }

        /// <summary>
        /// Emits the IL to push the integer (emits the best opcode depending on the value: <see cref="OpCodes.Ldc_I4_0"/> 
        /// or <see cref="OpCodes.Ldc_I4_M1"/> for instance) value onto the stack.
        /// </summary>
        /// <param name="g">This <see cref="ILGenerator"/> object.</param>
        /// <param name="i">The integer value to push.</param>
        public static void LdInt32( this ILGenerator g, int i )
        {
            if( i == 0 ) g.Emit( OpCodes.Ldc_I4_0 );
            else if( i == 1 ) g.Emit( OpCodes.Ldc_I4_1 );
            else if( i == 2 ) g.Emit( OpCodes.Ldc_I4_2 );
            else if( i == 3 ) g.Emit( OpCodes.Ldc_I4_3 );
            else if( i == 4 ) g.Emit( OpCodes.Ldc_I4_4 );
            else if( i == 5 ) g.Emit( OpCodes.Ldc_I4_5 );
            else if( i == 6 ) g.Emit( OpCodes.Ldc_I4_6 );
            else if( i == 7 ) g.Emit( OpCodes.Ldc_I4_7 );
            else if( i == 8 ) g.Emit( OpCodes.Ldc_I4_8 );
            else if( i == -1 ) g.Emit( OpCodes.Ldc_I4_M1 );
            else if( i >= -128 && i <= 127 ) g.Emit( OpCodes.Ldc_I4_S, (byte)i );
            else g.Emit( OpCodes.Ldc_I4, i );
        }

        /// <summary>
        /// Emits the IL to push (<see cref="OpCodes.Ldarg"/>) the actual argument at the given index onto the stack.
        /// </summary>
        /// <param name="g">This <see cref="ILGenerator"/> object.</param>
        /// <param name="i">Parameter index (0 being the 'this' for instance method).</param>
        public static void LdArg( this ILGenerator g, int i )
        {
            if( i == 0 ) g.Emit( OpCodes.Ldarg_0 );
            else if( i == 1 ) g.Emit( OpCodes.Ldarg_1 );
            else if( i == 2 ) g.Emit( OpCodes.Ldarg_2 );
            else if( i == 3 ) g.Emit( OpCodes.Ldarg_3 );
            else if( i < 255 ) g.Emit( OpCodes.Ldarg_S, (byte)i );
            else g.Emit( OpCodes.Ldarg, (short)i );
        }

        /// <summary>
        /// Emits the IL to pop (<see cref="OpCodes.Starg"/>) the top of the stach into the actual argument at the given index.
        /// </summary>
        /// <param name="g">This <see cref="ILGenerator"/> object.</param>
        /// <param name="i">Parameter index (0 being the 'this' for instance method).</param>
        public static void StArg( this ILGenerator g, int i )
        {
            if( i < 255 ) g.Emit( OpCodes.Starg_S, (byte)i );
            else g.Emit( OpCodes.Starg, (short)i );
        }

        /// <summary>
        /// Emits the IL to create a new array (<see cref="OpCodes.Newarr"/>) of objects and fills 
        /// it with the actual arguments of the method skipping the very first one: this must be used
        /// only inside a method with <see cref="System.Reflection.CallingConventions.HasThis"/> set.
        /// </summary>
        /// <param name="g">This <see cref="ILGenerator"/> object.</param>
        /// <param name="array">The local variable.</param>
        /// <param name="parameters">Type of the method parameters.</param>
        public static void CreateObjectArrayFromInstanceParameters( this ILGenerator g, LocalBuilder array, Type[] parameters )
        {
            g.LdInt32( parameters.Length );
            g.Emit( OpCodes.Newarr, typeof( object ) );
            g.StLoc( array );
            for( int i = 0; i < parameters.Length; ++i )
            {
                g.LdLoc( array );
                g.LdInt32( i );
                g.LdArg( i + 1 );
                if( parameters[i].IsGenericParameter || parameters[i].IsValueType )
                {
                    g.Emit( OpCodes.Box, parameters[i] );
                }
                g.Emit( OpCodes.Stelem_Ref );
            }
        }

    }
}
