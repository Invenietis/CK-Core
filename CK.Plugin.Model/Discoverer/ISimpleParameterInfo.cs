using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Plugin
{
    /// <summary>
    /// Basic Info describing a parameter
    /// </summary>
    public interface ISimpleParameterInfo
    {
        string ParameterName { get; }
        string ParameterType { get; }
    }
}
