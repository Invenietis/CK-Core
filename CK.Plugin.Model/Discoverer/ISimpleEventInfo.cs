using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using CK.Core;

namespace CK.Plugin
{
    /// <summary>
    /// Basic Info describing an event.
    /// </summary>
    public interface ISimpleEventInfo
    {
        string Name { get; }
    }    
}
