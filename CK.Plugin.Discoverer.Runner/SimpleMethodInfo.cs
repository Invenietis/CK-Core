using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using CK.Core;

namespace CK.Plugin.Discoverer.Runner
{
    /// <summary>
    ///  Contains a method's basic info
    /// </summary>
    [Serializable]
    public class SimpleMethodInfo : IComparable<SimpleMethodInfo>
    {
        List<SimpleParameterInfo> _parameters;

        string _returnType;
        string _name;

        public string ReturnType { get { return _returnType; } }
        public string Name { get { return _name; } }

        public IList<SimpleParameterInfo> Parameters { get { return _parameters; } }
        
        public SimpleMethodInfo(string name, string returnType)
        {
            _parameters = new List<SimpleParameterInfo>();
            _name = name;
            _returnType = returnType;
        }

        public SimpleMethodInfo Clone()
        {
            SimpleMethodInfo copiedMethod = new SimpleMethodInfo(this.Name,this.ReturnType);
            foreach (SimpleParameterInfo p in this.Parameters)
            {
                copiedMethod.Parameters.Add(new SimpleParameterInfo(p.ParameterName, p.ParameterType));
            }

            return copiedMethod;
        }

        public string GetSimpleSignature()
        {
            StringBuilder b = new StringBuilder();
            b.Append(ReturnType).Append(' ').Append(Name).Append('(');
            foreach (var p in Parameters) b.Append(p.ParameterType).Append(',');
            b.Length = b.Length - 1;
            b.Append(')');
            return b.ToString();
        }

        public int CompareTo( SimpleMethodInfo other )
        {
            if( this == other ) return 0;
            int cmp = this.GetSimpleSignature().CompareTo( other.GetSimpleSignature() );
            return cmp;
        }

     }
}
