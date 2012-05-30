#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Host\Service\ProxyFactory.cs) is part of CiviKey. 
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
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using CK.Reflection;
using System.IO;

namespace CK.Plugin.Hosting
{
	internal class ProxyFactory
	{
		static int _typeID;
		static ModuleBuilder _moduleBuilder;
		static MethodInfo _delegateCombine;
		static MethodInfo _delegateGetInvocationList;
		static MethodInfo _delegateGetMethod;
		static MethodInfo _delegateRemove;

		static ProxyFactory()
		{
			AssemblyName assemblyName = new AssemblyName("CKProxyAssembly");
			assemblyName.Version = new Version( 1, 0, 0, 0 );
           
#if(DEBUG) //Signing the DynamicAssembly when being in Release Mode
            StrongNameKeyPair kp;
            using( Stream stream = Assembly.GetAssembly( typeof( ProxyFactory ) ).GetManifestResourceStream( "CK.Plugin.DynamicKeyPair.DynamicKeyPair.snk" ) )
            {
                //PublicKey : 00240000048000009400000006020000002400005253413100040000010001009fbf2868f04bdf33df4c8c0517bb4c3d743b5b27fcd94009d42d6607446c1887a837e66545221788ecfff8786e85564c839ff56267fe1a3225cd9d8d9caa5aae3ba5d8f67f86ff9dbc5d66f16ba95bacde6d0e02f452fae20022edaea26d31e52870358d0dda69e592ea5cef609a054dac4dbbaa02edc32fb7652df9c0e8e9cd
                byte[] result = new byte[stream.Length]; 
                stream.Read(result,0,(int)stream.Length);
                kp = new StrongNameKeyPair( result );
            }
            assemblyName.KeyPair = kp;
#endif
			// Creates a new Assembly for running only (not saved).
			AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly( assemblyName, AssemblyBuilderAccess.Run );
			// Creates a new Module
			_moduleBuilder = assemblyBuilder.DefineDynamicModule( "ProxiesModule" );
			
			_delegateGetInvocationList = typeof( Delegate ).GetMethod( "GetInvocationList", Type.EmptyTypes );
			_delegateGetMethod = typeof( Delegate ).GetMethod( "get_Method", Type.EmptyTypes );

			Type[] paramTwoDelegates = new Type[] { typeof( Delegate ), typeof( Delegate ) };
			_delegateCombine = typeof( Delegate ).GetMethod( "Combine", paramTwoDelegates );
			_delegateRemove = typeof( Delegate ).GetMethod( "Remove", paramTwoDelegates );

		}

        class ProxyGenerator
        {
            TypeBuilder _typeBuilder;
            IProxyDefinition _definition;
            FieldBuilder _implField;
            HashSet<MethodInfo> _processedMethods;
            List<MethodInfo> _mRefs;
            List<EventInfo> _eRefs;

            public ProxyGenerator( TypeBuilder typeBuilder, IProxyDefinition definition )
            {
                _typeBuilder = typeBuilder;
                _definition = definition;
                // Define a member variable to hold the implementation
                _implField = typeBuilder.DefineField( "_impl", definition.TypeInterface, FieldAttributes.Private );
                _processedMethods = new HashSet<MethodInfo>();
                _mRefs = new List<MethodInfo>();
                _eRefs = new List<EventInfo>();
            }

            public void DefineConstructor()
            {
                // Defines constructor that accepts the typeInterface (the implementation). 
                ConstructorBuilder ctor = _typeBuilder.DefineConstructor(
                    MethodAttributes.Public,
                    CallingConventions.Standard,
                    new Type[] { _definition.TypeInterface, typeof( Type ), typeof( IList<MethodInfo> ), typeof( IList<EventInfo> ) } );

                // Generates ctor body. 
                {
                    ConstructorInfo ctorProxyBase = _definition.ProxyBase.GetConstructor( BindingFlags.Instance | BindingFlags.NonPublic, null,
                        new Type[] { typeof( object ), typeof( Type ), typeof( IList<MethodInfo> ), typeof( IList<EventInfo> ) }, null );
                    
                    ILGenerator g = ctor.GetILGenerator();
                    // Calls base ctor.
                    g.LdArg( 0 );
                    g.LdArg( 1 );
                    g.LdArg( 2 );
                    g.LdArg( 3 );
                    g.LdArg( 4 );
                    g.Emit( OpCodes.Call, ctorProxyBase );
                    // _impl = unavailableService;
                    g.LdArg( 0 );
                    g.LdArg( 1 );
                    g.Emit( OpCodes.Stfld, _implField );
                    g.Emit( OpCodes.Ret );
                }
            }

            public void DefineServiceProperty()
            {
                // The Service property of the IService<typeInterface> must return
                // the proxy itself, not _impl.
                MethodBuilder servicePropertyGet = _typeBuilder.DefineMethod(
                    "get_Service",
                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                    CallingConventions.HasThis,
                    _definition.TypeInterface,
                    Type.EmptyTypes );
                {
                    ILGenerator g = servicePropertyGet.GetILGenerator();
                    // return this;
                    g.Emit( OpCodes.Ldarg_0 );
                    g.Emit( OpCodes.Ret );
                }
                PropertyBuilder serviceProperty = _typeBuilder.DefineProperty( "Service", PropertyAttributes.HasDefault, _definition.TypeInterface, Type.EmptyTypes );
                serviceProperty.SetGetMethod( servicePropertyGet );
            }

            public void DefineImplementationProperty()
            {
                // Implementation = get_RawImpl/set_RawImpl
                MethodBuilder implementationPropertyGet = _typeBuilder.DefineMethod(
                    "get_RawImpl",
                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                    typeof( object ),
                    Type.EmptyTypes );
                {
                    ILGenerator g = implementationPropertyGet.GetILGenerator();
                    // return _impl;
                    g.Emit( OpCodes.Ldarg_0 );
                    g.Emit( OpCodes.Ldfld, _implField );
                    g.Emit( OpCodes.Ret );
                }
                MethodBuilder implementationPropertySet = _typeBuilder.DefineMethod(
                    "set_RawImpl",
                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                    typeof( void ),
                    new Type[] { typeof( object ) } );
                {
                    ILGenerator g = implementationPropertySet.GetILGenerator();
                    // _impl = (T)value;
                    g.Emit( OpCodes.Ldarg_0 );
                    g.Emit( OpCodes.Ldarg_1 );
                    g.Emit( OpCodes.Castclass, _implField.FieldType );
                    g.Emit( OpCodes.Stfld, _implField );
                    g.Emit( OpCodes.Ret );
                }
            }

            public void ImplementInterface()
            {
                // Starts with properties and events.
                ImplementProperties();
                ImplementEvents();
                // Non processed methods are then implemented.
                ImplementRemainingMethods();
            }

            public ServiceProxyBase Finalize( object unavailableImpl )
            {
                Type proxyType = _typeBuilder.CreateType();
                ServiceProxyBase p = (ServiceProxyBase)Activator.CreateInstance( proxyType, unavailableImpl, _definition.TypeInterface, _mRefs, _eRefs );
                return p;
            }

            void ImplementProperties()
            {
                foreach( PropertyInfo p in ReflectionHelper.GetFlattenProperties( _definition.TypeInterface ) )
                {
                    MethodInfo mGet = p.GetGetMethod( true );
                    if( mGet != null )
                    {
                        ProxyOptions optGet = _definition.GetPropertyMethodOptions( p, mGet );
                        GenerateInterceptor( mGet, optGet );
                    }
                    MethodInfo mSet = p.GetSetMethod( true );
                    if( mSet != null )
                    {
                        ProxyOptions optSet = _definition.GetPropertyMethodOptions( p, mSet );
                        GenerateInterceptor( mSet, optSet );
                    }
                }
            }

            void ImplementEvents()
            {
                foreach( EventInfo e in ReflectionHelper.GetFlattenEvents( _definition.TypeInterface ) )
                {
                    ProxyOptions optEvent = _definition.GetEventOptions( e );
                    DefineEventSupport( e, optEvent );
                }
            }

            void DefineEventSupport( EventInfo e, ProxyOptions generationOptions )
            {
                // Defines the hook field: Delegate _dXXX;
                FieldBuilder dField = _typeBuilder.DefineField( "_d" + e.Name, typeof( Delegate ), FieldAttributes.Private );

                // Defines the event field: <EventHandler> _hookXXX;
                FieldBuilder hField = _typeBuilder.DefineField( "_hook" + e.Name, e.EventHandlerType, FieldAttributes.Private );

                int eventMetaRef = RegisterRef( _eRefs, e );

                // Implements our hook method.
                MethodBuilder mHookB;
                {
                    MethodInfo mCall = e.EventHandlerType.GetMethod( "Invoke" );
                    Type[] parameters = ReflectionHelper.CreateParametersType( mCall.GetParameters() );
                    mHookB = _typeBuilder.DefineMethod( "_realService_" + e.Name, MethodAttributes.Private, CallingConventions.HasThis, typeof( void ), parameters );
                    {
                        SetDebuggerStepThroughAttribute( mHookB );
                        ILGenerator g = mHookB.GetILGenerator();
                        LocalBuilder logOptions = g.DeclareLocal( typeof( ServiceLogEventOptions ) );
                        LocalBuilder logger = g.DeclareLocal( typeof( LogEventEntry ) );

                        g.Emit( OpCodes.Ldarg_0 );
                        g.LdInt32( eventMetaRef );
                        g.Emit( OpCodes.Ldloca_S, logger );
                        g.Emit( OpCodes.Ldloca_S, logOptions );
                        string getLoggerName;
                        switch( generationOptions.RuntimeCheckStatus )
                        {
                            case ProxyOptions.CheckStatus.None: getLoggerName = "GetLoggerEventForAnyCall"; break;
                            case ProxyOptions.CheckStatus.NotDisabled: getLoggerName = "GetLoggerEventForNotDisabledCall"; break;
                            default: getLoggerName = "GetLoggerEventForRunningCall"; break; //ProxyOptions.CheckStatus.Running
                        }
                        g.EmitCall( OpCodes.Call, _definition.ProxyBase.GetMethod( getLoggerName, BindingFlags.NonPublic | BindingFlags.Instance ), null );

                        Label doRaise = g.DefineLabel();
                        g.Emit( OpCodes.Brtrue_S, doRaise );
                        g.Emit( OpCodes.Ret );
                        g.MarkLabel( doRaise );
                        
                        LocalBuilder client = g.DeclareLocal( typeof( Delegate ) );
                        LocalBuilder exception = generationOptions.CatchExceptions ? g.DeclareLocal( typeof( Exception ) ) : null;
                        LocalBuilder list = g.DeclareLocal( typeof( Delegate[] ) );
                        LocalBuilder listLength = g.DeclareLocal( typeof( int ) );
                        LocalBuilder index = g.DeclareLocal( typeof( int ) );
                        // Maps actual parameters.
                        for( int i = 0; i < parameters.Length; ++i )
                        {
                            if( parameters[i].IsAssignableFrom( _definition.TypeInterface ) )
                            {
                                g.LdArg( i + 1 );
                                g.Emit( OpCodes.Ldarg_0 );
                                g.Emit( OpCodes.Ldfld, _implField );
                                Label notTheSender = g.DefineLabel();
                                g.Emit( OpCodes.Bne_Un_S, notTheSender );
                                g.Emit( OpCodes.Ldarg_0 );
                                g.StArg( i + 1 );
                                g.MarkLabel( notTheSender );
                            }
                        }
                        // Should we log parameters?
                        if( parameters.Length > 0 )
                        {
                            Label skipLogParam = g.DefineLabel();
                            g.LdLoc( logOptions );
                            g.LdInt32( (int)ServiceLogEventOptions.LogParameters );
                            g.Emit( OpCodes.And );
                            g.Emit( OpCodes.Brfalse, skipLogParam );

                            LocalBuilder paramsArray = g.DeclareLocal( typeof( object[] ) );
                            g.CreateObjectArrayFromInstanceParameters( paramsArray, parameters );

                            g.LdLoc( logger );
                            g.LdLoc( paramsArray );
                            g.Emit( OpCodes.Stfld, typeof( LogEventEntry ).GetField( "_parameters", BindingFlags.Instance | BindingFlags.NonPublic ) );

                            g.MarkLabel( skipLogParam );
                        }


                        // Gets all the delegate to call in list.
                        g.Emit( OpCodes.Ldarg_0 );
                        g.Emit( OpCodes.Ldfld, dField );
                        g.EmitCall( OpCodes.Callvirt, _delegateGetInvocationList, null );
                        g.StLoc( list );
                        // listLength = list.Length;
                        g.LdLoc( list );
                        g.Emit( OpCodes.Ldlen );
                        g.Emit( OpCodes.Conv_I4 );
                        g.StLoc( listLength );
                        // index = 0;
                        g.Emit( OpCodes.Ldc_I4_0 );
                        g.StLoc( index );

                        Label beginOfLoop = g.DefineLabel();
                        Label endOfLoop = g.DefineLabel();
                        g.Emit( OpCodes.Br_S, endOfLoop );

                        g.MarkLabel( beginOfLoop );
                        // client = list[index];
                        g.LdLoc( list );
                        g.LdLoc( index );
                        g.Emit( OpCodes.Ldelem_Ref );
                        g.StLoc( client );

                        if( generationOptions.CatchExceptions )
                        {
                            g.BeginExceptionBlock();
                        }
                        g.LdLoc( client );
                        g.Emit( OpCodes.Castclass, e.EventHandlerType );
                        g.RepushActualParameters( false, parameters.Length );
                        g.EmitCall( OpCodes.Callvirt, mCall, null );

                        if( generationOptions.CatchExceptions )
                        {
                            Label bottomOfLoop = g.DefineLabel();
                            g.Emit( OpCodes.Leave_S, bottomOfLoop );

                            g.BeginCatchBlock( typeof( Exception ) );
                            g.StLoc( exception );

                            g.Emit( OpCodes.Ldarg_0 );
                            g.LdInt32( eventMetaRef );
                            g.LdLoc( client );
                            g.EmitCall( OpCodes.Callvirt, _delegateGetMethod, null );

                            g.LdLoc( exception );
                            g.Emit( OpCodes.Ldloca_S, logger );
                            g.EmitCall( OpCodes.Call, _definition.ProxyBase.GetMethod( "OnEventHandlingException", BindingFlags.NonPublic | BindingFlags.Instance ), null );

                            Label continueDispatch = g.DefineLabel();
                            g.Emit( OpCodes.Brtrue_S, continueDispatch );
                            g.Emit( OpCodes.Rethrow );
                            g.MarkLabel( continueDispatch );
                            
                            g.Emit( OpCodes.Leave_S, bottomOfLoop );
                            g.EndExceptionBlock();

                            g.MarkLabel( bottomOfLoop );
                        }
                        // ++index;
                        g.LdLoc( index );
                        g.Emit( OpCodes.Ldc_I4_1 );
                        g.Emit( OpCodes.Add );
                        g.StLoc( index );

                        // Checks whether we must continue the loop.
                        g.MarkLabel( endOfLoop );
                        g.LdLoc( index );
                        g.LdLoc( listLength );
                        g.Emit( OpCodes.Blt_S, beginOfLoop );

                        // if( (o & LogMethodOptions.Leave) != 0 )
                        // {
                            g.LdLoc( logOptions );
                            g.LdInt32( (int)ServiceLogEventOptions.EndRaise );
                            g.Emit( OpCodes.And );
                            Label skipLogPostCall = g.DefineLabel();
                            g.Emit( OpCodes.Brfalse, skipLogPostCall );

                            g.Emit( OpCodes.Ldarg_0 );
                            g.LdLoc( logger );
                            g.EmitCall( OpCodes.Call, _definition.ProxyBase.GetMethod( "LogEndRaise", BindingFlags.NonPublic | BindingFlags.Instance ), null );
                            g.MarkLabel( skipLogPostCall );
                        // }

                        g.Emit( OpCodes.Ret );
                    }

                }
                // Defines the event property itself: <EventHandler> XXX;
                EventBuilder eB = _typeBuilder.DefineEvent( e.Name, e.Attributes, e.EventHandlerType );

                // Implements the add_
                MethodInfo mAdd = e.GetAddMethod( true );
                if( mAdd != null )
                {
                    // Registers the method to skip its processing.
                    _processedMethods.Add( mAdd );

                    Type[] parameters = ReflectionHelper.CreateParametersType( mAdd.GetParameters() );
                    MethodBuilder mAddB = _typeBuilder.DefineMethod( mAdd.Name,
                        MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual,
                        CallingConventions.HasThis, mAdd.ReturnType, parameters );
                    {
                        SetDebuggerStepThroughAttribute( mAddB );
                        ILGenerator g = mAddB.GetILGenerator();

                        Label dFieldOK = g.DefineLabel();

                        g.Emit( OpCodes.Ldarg_0 );
                        g.Emit( OpCodes.Ldfld, dField );
                        g.Emit( OpCodes.Brtrue_S, dFieldOK );

                        Label hFieldOK = g.DefineLabel();
                        g.Emit( OpCodes.Ldarg_0 );
                        g.Emit( OpCodes.Ldfld, hField );
                        g.Emit( OpCodes.Brtrue_S, hFieldOK );
                        g.Emit( OpCodes.Ldarg_0 );
                        g.Emit( OpCodes.Ldarg_0 );
                        g.Emit( OpCodes.Ldftn, mHookB );
                        g.Emit( OpCodes.Newobj, e.EventHandlerType.GetConstructor( new Type[] { typeof( Object ), typeof( IntPtr ) } ) );
                        g.Emit( OpCodes.Stfld, hField );

                        g.MarkLabel( hFieldOK );
                        g.Emit( OpCodes.Ldarg_0 );
                        g.Emit( OpCodes.Ldfld, _implField );
                        g.Emit( OpCodes.Ldarg_0 );
                        g.Emit( OpCodes.Ldfld, hField );
                        g.Emit( OpCodes.Callvirt, mAdd );

                        g.MarkLabel( dFieldOK );
                        g.Emit( OpCodes.Ldarg_0 );
                        g.Emit( OpCodes.Ldarg_0 );
                        g.Emit( OpCodes.Ldfld, dField );
                        g.Emit( OpCodes.Ldarg_1 );
                        g.Emit( OpCodes.Call, _delegateCombine );
                        g.Emit( OpCodes.Stfld, dField );

                        g.Emit( OpCodes.Ret );
                    }
                    eB.SetAddOnMethod( mAddB );
                }

                // Implements the remove_
                MethodInfo mRemove = e.GetRemoveMethod( true );
                if( mRemove != null )
                {
                    // Registers the method to skip its processing.
                    _processedMethods.Add( mRemove );

                    Type[] parameters = ReflectionHelper.CreateParametersType( mRemove.GetParameters() );
                    MethodBuilder mRemoveB = _typeBuilder.DefineMethod( mRemove.Name,
                        MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.Virtual,
                        CallingConventions.HasThis,
                        mRemove.ReturnType, parameters );
                    {
                        SetDebuggerStepThroughAttribute( mRemoveB );
                        ILGenerator g = mRemoveB.GetILGenerator();

                        g.Emit( OpCodes.Ldarg_0 );
                        g.Emit( OpCodes.Ldarg_0 );
                        g.Emit( OpCodes.Ldfld, dField );
                        g.Emit( OpCodes.Ldarg_1 );
                        g.Emit( OpCodes.Call, _delegateRemove );
                        g.Emit( OpCodes.Stfld, dField );
                        g.Emit( OpCodes.Ldarg_0 );
                        g.Emit( OpCodes.Ldfld, dField );
                        Label end = g.DefineLabel();
                        g.Emit( OpCodes.Brtrue_S, end );
                        g.Emit( OpCodes.Ldarg_0 );
                        g.Emit( OpCodes.Ldfld, _implField );
                        g.Emit( OpCodes.Ldarg_0 );
                        g.Emit( OpCodes.Ldfld, hField );
                        g.Emit( OpCodes.Callvirt, mRemove );
                        g.MarkLabel( end );

                        g.Emit( OpCodes.Ret );
                    }
                    eB.SetRemoveOnMethod( mRemoveB );
                }
            }

            void ImplementRemainingMethods()
            {
                // For each methods in definition.TypeInterface...
                foreach( MethodInfo m in ReflectionHelper.GetFlattenMethods( _definition.TypeInterface ) )
                {
                    if( !_processedMethods.Contains( m ) )
                    {
                        ProxyOptions generationOptions = _definition.GetMethodOptions( m );
                        GenerateInterceptor( m, generationOptions );
                    }
                }
            }

            /// <summary>
            /// Registers an index to a <see cref="MemberInfo"/>.
            /// </summary>
            int RegisterRef<T>( List<T> reg, T element )
            {
                int i = reg.IndexOf( element );
                if( i < 0 )
                {
                    i = reg.Count;
                    reg.Add( element );
                }
                return i;
            }

            /// <summary>
            /// Generates the exact signature and the code that relays the call 
            /// to the _impl corresponding method.
            /// </summary>
            void GenerateInterceptor( MethodInfo m, ProxyOptions generationOptions )
            {
                // Registers the method.
                Debug.Assert( m != null && !_processedMethods.Contains( m ) );
                _processedMethods.Add( m );

                Type[] parameters;
                MethodBuilder mB = CreateInterfaceMethodBuilder( _typeBuilder, m, out parameters );
                SetDebuggerStepThroughAttribute( mB );

                int metaRef = RegisterRef( _mRefs, m );

                #region Body generation
                {
                    ILGenerator g = mB.GetILGenerator();

                    LocalBuilder logOption = g.DeclareLocal( typeof( ServiceLogMethodOptions ) );
                    LocalBuilder logger = g.DeclareLocal( typeof( LogMethodEntry ) );
                    
                    // The retValue local is used only if we must intercept 
                    // the return value (of course, if there is a return value).
                    LocalBuilder retValue = null;
                    if( m.ReturnType != typeof( void ) )
                    {
                        retValue = g.DeclareLocal( m.ReturnType );
                    }

                    g.Emit( OpCodes.Ldarg_0 );
                    g.LdInt32( metaRef );
                    g.Emit( OpCodes.Ldloca_S, logger );

                    string getLoggerName;
                    switch( generationOptions.RuntimeCheckStatus )
                    {
                        case ProxyOptions.CheckStatus.None: getLoggerName = "GetLoggerForAnyCall"; break;
                        case ProxyOptions.CheckStatus.NotDisabled: getLoggerName = "GetLoggerForNotDisabledCall"; break;
                        default: getLoggerName = "GetLoggerForRunningCall"; break; //ProxyOptions.CheckStatus.Running
                    }
                    g.EmitCall( OpCodes.Call, _definition.ProxyBase.GetMethod( getLoggerName, BindingFlags.NonPublic | BindingFlags.Instance ), null );                   
                    
                    g.StLoc( logOption );
                    if( parameters.Length > 0 )
                    {
                        Label skipLogParam = g.DefineLabel();
                        g.LdLoc( logOption );
                        g.LdInt32( (int)ServiceLogMethodOptions.LogParameters );
                        g.Emit( OpCodes.And );
                        g.Emit( OpCodes.Brfalse, skipLogParam );
                        
                        LocalBuilder paramsArray = g.DeclareLocal( typeof( object[] ) );
                        g.CreateObjectArrayFromInstanceParameters( paramsArray, parameters );

                        g.LdLoc( logger );
                        g.LdLoc( paramsArray );
                        g.Emit( OpCodes.Stfld, typeof(LogMethodEntry).GetField( "_parameters", BindingFlags.Instance|BindingFlags.NonPublic ) ); 
                    
                        g.MarkLabel( skipLogParam );
                    }
                    LocalBuilder exception = null;
                    if( generationOptions.CatchExceptions )
                    {
                        exception = g.DeclareLocal( typeof( Exception ) );
                        g.BeginExceptionBlock();
                    }
                    
                    // Pushes the _impl field on the stack.
                    g.Emit( OpCodes.Ldarg_0 );
                    g.Emit( OpCodes.Ldfld, _implField );
                    // Pushes all the parameters on the stack.
                    g.RepushActualParameters( false, parameters.Length );
                    g.EmitCall( OpCodes.Callvirt, m, null );

                    // if( (o & LogMethodOptions.Leave) != 0 )
                    g.LdLoc( logOption );
                    g.LdInt32( (int)ServiceLogMethodOptions.Leave );
                    g.Emit( OpCodes.And );
                    Label skipLogPostCall = g.DefineLabel();
                    g.Emit( OpCodes.Brfalse, skipLogPostCall );

                    // {
                    if( retValue == null )
                    {
                        g.Emit( OpCodes.Ldarg_0 );
                        g.LdLoc( logger );
                        g.EmitCall( OpCodes.Call, _definition.ProxyBase.GetMethod( "LogEndCall", BindingFlags.NonPublic | BindingFlags.Instance ), null );
                    }
                    else
                    {
                        Label skipLogWithValue = g.DefineLabel();
                        g.LdLoc( logOption );
                        g.LdInt32( (int)ServiceLogMethodOptions.LogReturnValue );
                        g.Emit( OpCodes.And );
                        g.Emit( OpCodes.Brfalse, skipLogWithValue );

                        // Save retValue.
                        g.StLoc( retValue );

                        g.Emit( OpCodes.Ldarg_0 );
                        g.LdLoc( logger );
                        g.LdLoc( retValue );
                        if( m.ReturnType.IsGenericParameter || m.ReturnType.IsValueType )
                        {
                            g.Emit( OpCodes.Box, m.ReturnType );
                        }
                        g.EmitCall( OpCodes.Call, _definition.ProxyBase.GetMethod( "LogEndCallWithValue", BindingFlags.NonPublic | BindingFlags.Instance ), null );

                        // Repush retValue and go to end.
                        g.LdLoc( retValue );
                        g.Emit( OpCodes.Br_S, skipLogPostCall );

                        // Just call LogEndCall without return value management (stack is okay).
                        g.MarkLabel( skipLogWithValue );
                        g.Emit( OpCodes.Ldarg_0 );
                        g.LdLoc( logger );
                        g.EmitCall( OpCodes.Call, _definition.ProxyBase.GetMethod( "LogEndCall", BindingFlags.NonPublic | BindingFlags.Instance ), null );
                    }
                    // }
                    g.MarkLabel( skipLogPostCall );

                    if( generationOptions.CatchExceptions )
                    {
                        Label end = g.DefineLabel();
                        g.Emit( OpCodes.Br_S, end );
                        
                        g.BeginCatchBlock( typeof( Exception ) );
                        g.StLoc( exception );

                        // if( (o & LogMethodOptions.LogError) != 0 )
                        g.LdLoc( logOption );
                        g.LdInt32( (int)ServiceLogMethodOptions.LogError );
                        g.Emit( OpCodes.And );
                        Label skipExceptionCall = g.DefineLabel();
                        g.Emit( OpCodes.Brfalse, skipExceptionCall );

                        g.Emit( OpCodes.Ldarg_0 );
                        g.LdInt32( metaRef );
                        g.LdLoc( exception );
                        g.LdLoc( logger );
                        g.EmitCall( OpCodes.Call, _definition.ProxyBase.GetMethod( "OnCallException", BindingFlags.NonPublic | BindingFlags.Instance ), null );

                        g.MarkLabel( skipExceptionCall );

                        g.Emit( OpCodes.Rethrow );
                        g.EndExceptionBlock();

                        g.MarkLabel( end );
                    }
                    g.Emit( OpCodes.Ret );
               }
                #endregion

            }

            void SetDebuggerStepThroughAttribute( MethodBuilder mB )
            {
                ConstructorInfo ctor = typeof( System.Diagnostics.DebuggerStepThroughAttribute ).GetConstructor( Type.EmptyTypes );
                CustomAttributeBuilder attr = new CustomAttributeBuilder( ctor, new object[0] );
                mB.SetCustomAttribute( attr );
            }

            /// <summary>
            /// Creates a <see cref="MethodBuilder"/> for a given method. 
            /// Handles generic parameters on the method.
            /// </summary>
            /// <param name="typeBuilder"></param>
            /// <param name="m"></param>
            /// <param name="parameters"></param>
            /// <returns></returns>
            public static MethodBuilder CreateInterfaceMethodBuilder( TypeBuilder typeBuilder, MethodInfo m, out Type[] parameters )
            {
                // Initializes the signature with only its name, attributes and calling conventions first.
                MethodBuilder mB = typeBuilder.DefineMethod(
                    m.Name,
                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                    CallingConventions.HasThis );

                parameters = ReflectionHelper.CreateParametersType( m.GetParameters() );
                // If it is a Generic method definition (since we are working with an interface, 
                // it can not be a Generic closed nor opened method).
                if( m.IsGenericMethodDefinition )
                {
                    Type[] genArgs = m.GetGenericArguments();
                    Debug.Assert( genArgs.Length > 0 );
                    string[] names = new string[genArgs.Length];
                    for( int i = 0; i < names.Length; ++i ) names[i] = genArgs[i].Name;
                    // Defines generic parameters.
                    GenericTypeParameterBuilder[] genTypes = mB.DefineGenericParameters( names );
                    for( int i = 0; i < names.Length; ++i )
                    {
                        Type source = genArgs[i];
                        GenericTypeParameterBuilder target = genTypes[i];
                        target.SetGenericParameterAttributes( source.GenericParameterAttributes );
                        Type[] sourceConstraints = source.GetGenericParameterConstraints();
                        List<Type> interfaces = new List<Type>();
                        foreach( Type c in sourceConstraints )
                        {
                            if( c.IsClass ) target.SetBaseTypeConstraint( c );
                            else interfaces.Add( c );
                        }
                        target.SetInterfaceConstraints( interfaces.ToArray() );
                    }
                }
                // Now that generic parameters have been defined, configures the signature.
                mB.SetReturnType( m.ReturnType );
                mB.SetParameters( parameters );
                return mB;
            }


        }

		/// <summary>
		/// Creates a proxyfied interface according to the given definition.
		/// </summary>
		/// <param name="definition">Definition of the proxy to build.</param>
		/// <param name="initialImplementation">Optional first and available implementation.</param>
		/// <returns></returns>
		internal static ServiceProxyBase CreateProxy( IProxyDefinition definition )
		{
			Debug.Assert( definition.TypeInterface.IsInterface, "This check MUST be done by ProxyDefinition implementation." );

			string dynamicTypeName = String.Format( "{0}_Proxy_{1}", definition.TypeInterface.Name, Interlocked.Increment( ref _typeID ) );

            // Defines a public sealed class that implements typeInterface only.
            TypeBuilder typeBuilder = _moduleBuilder.DefineType(
                    dynamicTypeName,
                    TypeAttributes.Class | TypeAttributes.Sealed,
                    definition.ProxyBase,
                    new Type[] { definition.TypeInterface } );

            // This defines the IService<typeInterface> interface.
            if( definition.IsDynamicService )
            {
                // Our proxy object will implement both typeInterface and IService<typeInterface> interfaces.
                Type serviceInterfaceType = typeof( IService<> ).MakeGenericType( new Type[] { definition.TypeInterface } );
                typeBuilder.AddInterfaceImplementation( serviceInterfaceType );
            }
            
            ProxyGenerator pg = new ProxyGenerator( typeBuilder, definition );

            pg.DefineConstructor();

			pg.DefineServiceProperty();

			pg.DefineImplementationProperty();

            pg.ImplementInterface();

            object unavailableImpl = CreateUnavailableImplementation( definition.TypeInterface, dynamicTypeName + "_UN" );

			return pg.Finalize( unavailableImpl );
		}

        private static object CreateUnavailableImplementation( Type interfaceType, string dynamicTypeName )
        {
            object unavailableImpl;
            // Defines a public sealed class that only implements typeInterface 
            // and for which all methods throw ServiceNotAvailableException.
            TypeBuilder typeBuilderNotAvailable = _moduleBuilder.DefineType(
                dynamicTypeName,
                TypeAttributes.Class | TypeAttributes.Sealed,
                typeof( object ),
                new Type[] { interfaceType } );

            MethodInfo mGetTypeFromHandle = typeof( Type ).GetMethod( "GetTypeFromHandle" );
            ConstructorInfo cNotAvailableException = typeof( ServiceNotAvailableException ).GetConstructor( new[] { typeof( Type ) } );
            foreach( MethodInfo m in ReflectionHelper.GetFlattenMethods( interfaceType ) )
            {
                Type[] parameters;
                MethodBuilder mB = ProxyGenerator.CreateInterfaceMethodBuilder( typeBuilderNotAvailable, m, out parameters );
                ILGenerator g = mB.GetILGenerator();
                g.Emit( OpCodes.Ldtoken, interfaceType );
                g.EmitCall( OpCodes.Call, mGetTypeFromHandle, null );
                g.Emit( OpCodes.Newobj, cNotAvailableException );
                g.Emit( OpCodes.Throw );
            }
            Type unavailableType = typeBuilderNotAvailable.CreateType();
            unavailableImpl = Activator.CreateInstance( unavailableType );
            return unavailableImpl;
        }

	}

}
