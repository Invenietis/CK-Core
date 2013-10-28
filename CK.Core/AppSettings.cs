using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// Very simple façade for simple application settings.
    /// This does not handle multiple configurations per key (like ConfigurationManager.AppSettings can do since it is a NameValueCollection) but
    /// can expose potentially complex configuration objects instead of only strings.
    /// It can be initialized only once, before any other access, and when not initialized tries to automatically use the standard ConfigurationManager.AppSettings 
    /// through late binding. However, it supports multiple overriding and reverting to the original configuration. 
    /// (Override support and restoration is mainly designed for tests but the override functionnality alone can be a useful feature in real life application.)
    /// </summary>
    public class AppSettings
    {
        readonly object _lock = new object();
        Func<string,object> _initializedGetObject;
        Func<string,object> _getObject;
        bool _initialized;

        /// <summary>
        /// Gets the default, singleton, AppSettings object.
        /// </summary>
        static public readonly AppSettings Default = new AppSettings();

        /// <summary>
        /// Initializes this <see cref="AppSettings"/> object. This can be called only once
        /// prior to any use of this object.
        /// When not called before the first access, the .Net ConfigurationManager.AppSettings is used if possible (late binding).
        /// </summary>
        /// <param name="getConfigurationObject">The fucntion that ultimately </param>
        public void Initialize( Func<string, object> getConfigurationObject )
        {
            if( getConfigurationObject == null ) throw new ArgumentNullException( "getConfigurationObject" );
            lock( _lock )
            {
                if( _initialized ) throw new CKException( "AppSettingsAlreadyInitialied" );
                _initializedGetObject = _getObject = getConfigurationObject;
                _initialized = true;
            }
        }

        /// <summary>
        /// Overrides this <see cref="AppSettings"/> object configuration function. This can be called after <see cref="Initialize"/> or <see cref="DefaultInitialize"/>
        /// or directly: any access to the configuration since any access triggers a call to DefaultInitialize.
        /// The function that overrides the current configuration is called with the previously active function to enable chaining (filtering, alteration of the keys and or the values).
        /// </summary>
        /// <param name="filterConfigurationObject">The fucntion that ultimately </param>
        public void Override( Func<Func<string, object>,string, object> filterConfigurationObject )
        {
            if( filterConfigurationObject == null ) throw new ArgumentNullException( "filterConfigurationObject" );
            lock( _lock )
            {
                if( !_initialized ) DefaultInitialization();
                // Local required to avoid closure to this field:
                var prev = _getObject;
                _getObject = s => filterConfigurationObject( prev, s );
            }
        }

        /// <summary>
        /// Reverts all calls to <see cref="Override"/>: the initial configuration is restored.
        /// </summary>
        public void RevertOverrides()
        {
            lock( _lock )
            {
                if( !_initialized ) DefaultInitialization();
                _getObject = _initializedGetObject;
            }
        }

        /// <summary>
        /// Automatically bind to standard ConfigurationManager.AppSettings to obtain configuration strings.
        /// This method is automatically called when this object is not yet initialized on any access other than <see cref="Initialize"/>.
        /// </summary>
        public void DefaultInitialize()
        {
            lock( _lock )
            {
                if( _initialized ) throw new CKException( R.AppSettingsAlreadyInitialized );
                DoDefaultInitialize();
            }
        }

        /// <summary>
        /// Gets the settings as a string: <see cref="Object.ToString"/> is called on object.
        /// Null if the key is not found.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <returns>The string (object expressed as a string) or null if no such configuration exists.</returns>
        public string this[string key]
        {
            get 
            { 
                if( !_initialized ) DefaultInitialization();
                var o = _getObject( key );
                return o != null ? o.ToString() : null;
            }
        }

        /// <summary>
        /// Gets the settings as an object.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <returns>The configured available object or null if no such configuration exists.</returns>
        public object Get( string key )
        {
            if( !_initialized ) DefaultInitialization();
            return _getObject( key );
        }

        /// <summary>
        /// Gets the settings as a typed object: if the object is not available or is not of the given type, the default value is returned;
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <param name="defaultValue">The default value to return if no object exists.</param>
        /// <returns>The configured available object or the <paramref name="defaultValue"/> if no such configuration exists.</returns>
        public T Get<T>( string key, T defaultValue )
        {
            if( !_initialized ) DefaultInitialization();
            var o = _getObject( key );
            return o is T ? (T)o : defaultValue;
        }

        /// <summary>
        /// Gets the settings as an object: if the object is not available, an exception is thrown.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <returns>The configured available object.</returns>
        public object GetRequired( string key )
        {
            if( !_initialized ) DefaultInitialization();
            var o = _getObject( key );
            if( o == null ) throw new CKException( R.AppSettingsRequiredConfigurationMissing, key );
            return o;
        }

        /// <summary>
        /// Gets the settings as a typed object: if the object is not available or is not of the given type, an exception is thrown.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <returns>The configured available object.</returns>
        public T GetRequired<T>( string key )
        {
            if( !_initialized ) DefaultInitialization();
            var o = _getObject( key );
            if( o == null ) throw new CKException( R.AppSettingsRequiredConfigurationMissing, key );
            if( !(o is T) ) throw new CKException( R.AppSettingsRequiredConfigurationBadType, key, typeof(T).FullName );
            return (T)o;
        }

        void DefaultInitialization()
        {
            lock( _lock )
            {
                if( _initialized ) return;
                DoDefaultInitialize();
            }
        }

        const string _defType = "System.Configuration.ConfigurationManager, System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

        void DoDefaultInitialize()
        {
            Type configMananger = SimpleTypeFinder.Default.ResolveType( _defType, false );
            // Type op_equality is not portable: use ReferenceEquals.
            if( !ReferenceEquals( configMananger, null ) )
            {
                Type[] stringParams = new Type[] { typeof( string ) };
                MethodInfo getAppSettings = configMananger.GetProperty( "AppSettings", BindingFlags.Public | BindingFlags.Static ).GetGetMethod();
                MethodInfo indexer = getAppSettings.ReturnType.GetProperty( "Item", typeof( string ), stringParams ).GetGetMethod();

                DynamicMethod getter = new DynamicMethod( "CK-ReadConfigurationManagerAppSettings", typeof( string ), stringParams, true );
                ILGenerator g = getter.GetILGenerator();
                g.EmitCall( OpCodes.Call, getAppSettings, null );
                g.Emit( OpCodes.Ldarg_0 );
                g.EmitCall( OpCodes.Call, indexer, null );
                g.Emit( OpCodes.Ret );
                _getObject = (Func<string, object>)getter.CreateDelegate( typeof( Func<string, object> ) );
                _initialized = true;
            }
            else throw new CKException( R.AppSettingsDefaultInitializationFailed );
        }

    }
}
