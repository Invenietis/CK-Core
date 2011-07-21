using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin.Discoverer;

namespace CK.Plugin
{
    /// <summary>
    ///  Contains a property's basic info
    /// </summary>
    public class SimplePropertyInfo : ISimplePropertyInfo, IComparable<SimplePropertyInfo>
    {
        string _name;
        string _propertyType;

        public string Name { get { return _name; } }
        public string PropertyType { get { return _propertyType; } }

        public SimplePropertyInfo()
        {
        }

        public void Initialize(Discoverer.Runner.SimplePropertyInfo r)
        {
            _name = r.Name;
            _propertyType = r.PropertyType;
        }

        public bool Merge( Discoverer.Runner.SimplePropertyInfo propInfo )
        {
            bool hasChanged = false;

            if (_propertyType != propInfo.PropertyType)
            {
                _propertyType = propInfo.PropertyType;
                hasChanged = true;
            }

            return hasChanged;
        }

        public int CompareTo(SimplePropertyInfo other)
        {
            if (this == other) return 0;
            int cmp = _name.CompareTo(other.Name);
            if (cmp == 0) cmp = _propertyType.CompareTo(other.PropertyType);
            return cmp;
        }
    } 
}
