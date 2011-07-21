using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Plugin.Discoverer.Runner
{
    /// <summary>
    /// Contains a parameter's basic info
    /// </summary>
    [Serializable]
    public class SimpleParameterInfo
    {
        string _parameterName;
        string _parameterType;

        public string ParameterName { get { return _parameterName; } }

        public string ParameterType { get { return _parameterType; } }

        public SimpleParameterInfo(string parameterName, string parameterType)
        {
            _parameterType = parameterType;
            _parameterName = parameterName;
        }

    }
}
