using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Plugin
{
    /// <summary>
    ///  Basic Info describing a property
    /// </summary>
    public interface ISimplePropertyInfo
    {
        string Name { get; }
        string PropertyType { get; }
    } 
}
