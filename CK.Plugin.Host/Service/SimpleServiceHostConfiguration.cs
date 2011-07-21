using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;


namespace CK.Plugin.Hosting
{
    /// <summary>
    /// Simple dictionary based implementation of <see cref="IServiceHostConfiguration"/>.
    /// </summary>
    public class SimpleServiceHostConfiguration : IServiceHostConfiguration, ISimpleServiceHostConfiguration
    {
        Dictionary<MethodInfo,ServiceLogMethodOptions> _methods;
        Dictionary<EventInfo,ServiceLogEventOptions> _events;

        public SimpleServiceHostConfiguration()
        {
            _methods = new Dictionary<MethodInfo, ServiceLogMethodOptions>();
            _events = new Dictionary<EventInfo, ServiceLogEventOptions>();
        }

        public void Clear()
        {
            _methods.Clear();
            _events.Clear();
        }

        public void SetConfiguration( MethodInfo m, ServiceLogMethodOptions option )
        {
            _methods[m] = option;
        }

        public void SetConfiguration( PropertyInfo p, ServiceLogMethodOptions option )
        {
            MethodInfo mG = p.GetGetMethod();
            if( mG != null ) SetConfiguration( mG, option );
            MethodInfo mS = p.GetSetMethod();
            if( mS != null ) SetConfiguration( mS, option );
        }

        public void SetConfiguration( EventInfo e, ServiceLogEventOptions option )
        {
            _events[e] = option;
        }

        public void SetMethodGroupConfiguration( Type type, string methodName, ServiceLogMethodOptions option )
        {
            foreach( var m in CK.Reflection.Helper.GetFlattenMethods( type ).Where( m => m.Name == methodName ) )
            {
                _methods[m] = option;
            }
        }
        
        public void SetAllMethodsConfiguration( Type type, ServiceLogMethodOptions option )
        {
            foreach( var m in CK.Reflection.Helper.GetFlattenMethods( type ).Where( m => !m.IsSpecialName ) )
            {
                _methods[m] = option;
            }
        }

        public void SetAllPropertiesConfiguration( Type type, ServiceLogMethodOptions option )
        {
            foreach( var p in CK.Reflection.Helper.GetFlattenProperties( type ) ) SetConfiguration( p, option );
        }

        public void SetAllEventsConfiguration( Type type, ServiceLogEventOptions option )
        {
            foreach( var e in CK.Reflection.Helper.GetFlattenEvents( type ) )
            {
                _events[e] = option;
            }
        }

        /// <summary>
        /// Returns the <see cref="ServiceLogMethodOptions"/> for the given method.
        /// </summary>
        /// <param name="m">Method for which options should be obtained.</param>
        /// <returns>Configuration for the method.</returns>
        public ServiceLogMethodOptions GetOptions( MethodInfo m )
        {
            ServiceLogMethodOptions o;
            _methods.TryGetValue( m, out o );
            return o;
        }

        /// <summary>
        /// Returns the <see cref="ServiceLogEventOptions"/> for the given event.
        /// </summary>
        /// <param name="e">Event for which options should be obtained.</param>
        /// <returns>Configuration for the event.</returns>
        public ServiceLogEventOptions GetOptions( EventInfo e )
        {
            ServiceLogEventOptions o;
            _events.TryGetValue( e, out o );
            return o;
        }

    }

}
