#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Reflection\LegacySupport\CustomAttributeExtensions.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2014, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

#if net40

using System;
using System.Collections.Generic;
using System.Reflection;

namespace CK.Reflection
{

    public static class CustomAttributeExtensions
    {
    
        public static T GetCustomAttribute<T>( this Assembly element ) where T : Attribute
        {
            return (T)element.GetCustomAttribute( typeof( T ) );
        }

    
        public static T GetCustomAttribute<T>( this MemberInfo element ) where T : Attribute
        {
            return (T)element.GetCustomAttribute( typeof( T ) );
        }

    
        public static T GetCustomAttribute<T>( this Module element ) where T : Attribute
        {
            return (T)element.GetCustomAttribute( typeof( T ) );
        }

    
        public static T GetCustomAttribute<T>( this ParameterInfo element ) where T : Attribute
        {
            return (T)element.GetCustomAttribute( typeof( T ) );
        }

    
        public static Attribute GetCustomAttribute( this Assembly element, Type attributeType )
        {
            return Attribute.GetCustomAttribute( element, attributeType );
        }

    
        public static T GetCustomAttribute<T>( this MemberInfo element, bool inherit ) where T : Attribute
        {
            return (T)element.GetCustomAttribute( typeof( T ), inherit );
        }

    
        public static Attribute GetCustomAttribute( this MemberInfo element, Type attributeType )
        {
            return Attribute.GetCustomAttribute( element, attributeType );
        }

    
        public static Attribute GetCustomAttribute( this Module element, Type attributeType )
        {
            return Attribute.GetCustomAttribute( element, attributeType );
        }

    
        public static T GetCustomAttribute<T>( this ParameterInfo element, bool inherit ) where T : Attribute
        {
            return (T)element.GetCustomAttribute( typeof( T ), inherit );
        }

    
        public static Attribute GetCustomAttribute( this ParameterInfo element, Type attributeType )
        {
            return Attribute.GetCustomAttribute( element, attributeType );
        }

    
        public static Attribute GetCustomAttribute( this MemberInfo element, Type attributeType, bool inherit )
        {
            return Attribute.GetCustomAttribute( element, attributeType, inherit );
        }

    
        public static Attribute GetCustomAttribute( this ParameterInfo element, Type attributeType, bool inherit )
        {
            return Attribute.GetCustomAttribute( element, attributeType, inherit );
        }

    
        public static IEnumerable<Attribute> GetCustomAttributes( this Assembly element )
        {
            return Attribute.GetCustomAttributes( element );
        }

    
        public static IEnumerable<T> GetCustomAttributes<T>( this Assembly element ) where T : Attribute
        {
            return (IEnumerable<T>)element.GetCustomAttributes( typeof( T ) );
        }

    
        public static IEnumerable<Attribute> GetCustomAttributes( this MemberInfo element )
        {
            return Attribute.GetCustomAttributes( element );
        }

    
        public static IEnumerable<T> GetCustomAttributes<T>( this MemberInfo element ) where T : Attribute
        {
            return (IEnumerable<T>)element.GetCustomAttributes( typeof( T ) );
        }

    
        public static IEnumerable<Attribute> GetCustomAttributes( this Module element )
        {
            return Attribute.GetCustomAttributes( element );
        }

    
        public static IEnumerable<T> GetCustomAttributes<T>( this Module element ) where T : Attribute
        {
            return (IEnumerable<T>)element.GetCustomAttributes( typeof( T ) );
        }

    
        public static IEnumerable<Attribute> GetCustomAttributes( this ParameterInfo element )
        {
            return Attribute.GetCustomAttributes( element );
        }

    
        public static IEnumerable<T> GetCustomAttributes<T>( this ParameterInfo element ) where T : Attribute
        {
            return (IEnumerable<T>)element.GetCustomAttributes( typeof( T ) );
        }

    
        public static IEnumerable<Attribute> GetCustomAttributes( this Assembly element, Type attributeType )
        {
            return Attribute.GetCustomAttributes( element, attributeType );
        }

    
        public static IEnumerable<Attribute> GetCustomAttributes( this MemberInfo element, bool inherit )
        {
            return Attribute.GetCustomAttributes( element, inherit );
        }

    
        public static IEnumerable<T> GetCustomAttributes<T>( this MemberInfo element, bool inherit ) where T : Attribute
        {
            return (IEnumerable<T>)element.GetCustomAttributes( typeof( T ), inherit );
        }

    
        public static IEnumerable<Attribute> GetCustomAttributes( this MemberInfo element, Type attributeType )
        {
            return Attribute.GetCustomAttributes( element, attributeType );
        }

    
        public static IEnumerable<Attribute> GetCustomAttributes( this Module element, Type attributeType )
        {
            return Attribute.GetCustomAttributes( element, attributeType );
        }

    
        public static IEnumerable<Attribute> GetCustomAttributes( this ParameterInfo element, bool inherit )
        {
            return Attribute.GetCustomAttributes( element, inherit );
        }

    
        public static IEnumerable<T> GetCustomAttributes<T>( this ParameterInfo element, bool inherit ) where T : Attribute
        {
            return (IEnumerable<T>)element.GetCustomAttributes( typeof( T ), inherit );
        }

    
        public static IEnumerable<Attribute> GetCustomAttributes( this ParameterInfo element, Type attributeType )
        {
            return Attribute.GetCustomAttributes( element, attributeType );
        }

    
        public static IEnumerable<Attribute> GetCustomAttributes( this MemberInfo element, Type attributeType, bool inherit )
        {
            return Attribute.GetCustomAttributes( element, attributeType, inherit );
        }

    
        public static IEnumerable<Attribute> GetCustomAttributes( this ParameterInfo element, Type attributeType, bool inherit )
        {
            return Attribute.GetCustomAttributes( element, attributeType, inherit );
        }

    
        public static bool IsDefined( this Assembly element, Type attributeType )
        {
            return Attribute.IsDefined( element, attributeType );
        }

    
        public static bool IsDefined( this MemberInfo element, Type attributeType )
        {
            return Attribute.IsDefined( element, attributeType );
        }

    
        public static bool IsDefined( this Module element, Type attributeType )
        {
            return Attribute.IsDefined( element, attributeType );
        }

    
        public static bool IsDefined( this ParameterInfo element, Type attributeType )
        {
            return Attribute.IsDefined( element, attributeType );
        }

    
        public static bool IsDefined( this MemberInfo element, Type attributeType, bool inherit )
        {
            return Attribute.IsDefined( element, attributeType, inherit );
        }

    
        public static bool IsDefined( this ParameterInfo element, Type attributeType, bool inherit )
        {
            return Attribute.IsDefined( element, attributeType, inherit );
        }
    }
}

#endif