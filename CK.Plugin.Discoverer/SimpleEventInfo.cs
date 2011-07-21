using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using CK.Core;
using CK.Plugin.Discoverer;

namespace CK.Plugin
{
    /// <summary>
    /// Contains an event's basic info.
    /// </summary>
    [Serializable]
    public class SimpleEventInfo : ISimpleEventInfo, IComparable<SimpleEventInfo>
    {
        string _name;

        public string Name { get { return _name; } }

        public SimpleEventInfo()
        {
        }

        internal void Initialize( Discoverer.Runner.SimpleEventInfo r )
        {
            _name = r.Name;
        }

        internal bool Merge( Discoverer.Runner.SimpleEventInfo r )
        {
            return false;
        }

        public int CompareTo( SimpleEventInfo other )
        {
            if( this == other ) return 0;
            int cmp = _name.CompareTo( other.Name );
            return cmp;
        }

    }
}
