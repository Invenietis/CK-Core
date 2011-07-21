using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace CK.Plugin
{
    /// <summary>
    /// Contains a parameter's basic info
    /// </summary>
    [Serializable]
    public class SimpleParameterInfo : ISimpleParameterInfo, IComparable<SimpleParameterInfo>
    {
        string _parameterName;
        string _parameterType;

        public string ParameterName { get { return _parameterName; } }
        public string ParameterType { get { return _parameterType; } }

        public SimpleParameterInfo()
        {            
        }

        internal void Initialize( Discoverer.Runner.SimpleParameterInfo rP )
        {
            _parameterType = rP.ParameterType;
            _parameterName = rP.ParameterName;
        }

        internal bool Merge( Discoverer.Runner.SimpleParameterInfo rP )
        {
            Debug.Assert( rP.ParameterType == _parameterType );
            if( rP.ParameterName != _parameterName )
            {
                _parameterName = rP.ParameterName;
                return true;
            }
            return false;
        }

        public int CompareTo( SimpleParameterInfo other )
        {
            if (this == other) return 0;
            int cmp = _parameterName.CompareTo(other.ParameterName);
            if (cmp == 0) cmp = _parameterType.CompareTo(other.ParameterType);
            return cmp;
        }

    }
}
