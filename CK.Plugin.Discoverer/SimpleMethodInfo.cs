using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using CK.Core;
using System.Diagnostics;

namespace CK.Plugin
{
    /// <summary>
    ///  Contains a method's basic info
    /// </summary>
    [Serializable]
    public class SimpleMethodInfo : ISimpleMethodInfo, IComparable<SimpleMethodInfo>
    {
        IList<SimpleParameterInfo> _parameters;
        IReadOnlyList<ISimpleParameterInfo> _parametersEx;

        string _returnType;
        string _name;

        public string ReturnType { get { return _returnType; } }
        public string Name { get { return _name; } }
        public IList<SimpleParameterInfo> Parameters { get { return _parameters; } }
        IReadOnlyList<ISimpleParameterInfo> ISimpleMethodInfo.Parameters { get { return _parametersEx; } }
        
        public SimpleMethodInfo()
        {
            _parameters = new List<SimpleParameterInfo>();
            _parametersEx = new ReadOnlyListOnIList<SimpleParameterInfo>(_parameters);
        }

        internal void Initialize( Discoverer.Runner.SimpleMethodInfo r )
        {
            _name = r.Name;
            _returnType = r.ReturnType;
            foreach (Discoverer.Runner.SimpleParameterInfo rP in r.Parameters)
            {
                SimpleParameterInfo p = new SimpleParameterInfo();
                p.Initialize(rP);
                _parameters.Add(p);
            }
        }

        internal bool Merge( Discoverer.Runner.SimpleMethodInfo r )
        {
            Debug.Assert( _name == r.Name );
            Debug.Assert( _returnType == r.ReturnType );
            Debug.Assert( _parameters.Count == r.Parameters.Count );
            
            bool hasChanged = false;
            for( int i = 0; i < _parameters.Count; ++i )
            {
                hasChanged |= _parameters[i].Merge( r.Parameters[i] );
            }
            return hasChanged;
        }

        public int CompareTo(SimpleMethodInfo other)
        {
            if (this == other) return 0;
            int cmp = this.GetSimpleSignature().CompareTo(other.GetSimpleSignature());
            if (cmp == 0) cmp = _returnType.CompareTo(other.ReturnType);
            return cmp;
        }

    }
}
