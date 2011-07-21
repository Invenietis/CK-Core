using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using CK.Core;

namespace CK.Plugin
{
    /// <summary>
    ///  Basic Info describing a method
    /// </summary>
    public interface ISimpleMethodInfo
    {        
        string ReturnType { get; }
        string Name { get; }
        IReadOnlyList<ISimpleParameterInfo> Parameters { get; }
    }
}
