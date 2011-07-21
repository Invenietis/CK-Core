using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using CK.Core;

namespace CK.Plugin.Discoverer.Runner
{
    /// <summary>
    /// Contains an event's basic info
    /// </summary>
    [Serializable]
    public class SimpleEventInfo : IComparable<SimpleEventInfo>
    {
        string _name;

        public string Name { get { return _name; } }

        public SimpleEventInfo(string name)
        {
            _name = name;
        }

        public SimpleEventInfo Clone()
        {
            SimpleEventInfo copiedEvent = new SimpleEventInfo(this.Name);
            return copiedEvent;
        }

        #region IComparable<ISimpleEventInfo> Members

        /// <summary>
        /// Compares the names of the events
        /// </summary>
        /// <param name="other"></param>
        /// <returns>1 if other is "less" than the current object, 0 if the two have the same name, -1 otherwise</returns>
        public int CompareTo( SimpleEventInfo other )
        {
            if( this == other ) return 0;
            int cmp = _name.CompareTo( other.Name );            
            return cmp;
        }

        #endregion
    }    
}
