using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Plugin.Discoverer.Runner
{
    /// <summary>
    ///  Contains property's basic info.
    /// </summary>
    [Serializable]
    public class SimplePropertyInfo : IComparable<SimplePropertyInfo>
    {
        string _name;
        string _propertyType;

        public string Name { get { return _name; } }

        public string PropertyType { get { return _propertyType; } }

        public SimplePropertyInfo(string name, string propertyType)
        {
            _name = name;
            _propertyType = propertyType;
        }

        public int CompareTo( SimplePropertyInfo other )
        {
            if( this == other ) return 0;
            return _name.CompareTo( other.Name );
        }
    } 
}
