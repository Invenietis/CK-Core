#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Discoverer\SimpleMethodInfo.cs) is part of CiviKey. 
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
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using CK.Core;
using System.Diagnostics;

namespace CK.Plugin
{
    /// <summary>
    ///  Contains a method's basic info
    /// </summary>
    [Serializable]
    public class SimpleMethodInfo : ISimpleMethodInfo, IComparable<SimpleMethodInfo>
    {
        IList<SimpleParameterInfo> _parameters;
        IReadOnlyList<ISimpleParameterInfo> _parametersEx;

        string _returnType;
        string _name;

        public string ReturnType { get { return _returnType; } }
        public string Name { get { return _name; } }
        public IList<SimpleParameterInfo> Parameters { get { return _parameters; } }
        IReadOnlyList<ISimpleParameterInfo> ISimpleMethodInfo.Parameters { get { return _parametersEx; } }
        
        public SimpleMethodInfo()
        {
            _parameters = new List<SimpleParameterInfo>();
            _parametersEx = new ReadOnlyListOnIList<SimpleParameterInfo>(_parameters);
        }

        internal void Initialize( Discoverer.Runner.SimpleMethodInfo r )
        {
            _name = r.Name;
            _returnType = r.ReturnType;
            foreach (Discoverer.Runner.SimpleParameterInfo rP in r.Parameters)
            {
                SimpleParameterInfo p = new SimpleParameterInfo();
                p.Initialize(rP);
                _parameters.Add(p);
            }
        }

        internal bool Merge( Discoverer.Runner.SimpleMethodInfo r )
        {
            Debug.Assert( _name == r.Name );
            Debug.Assert( _returnType == r.ReturnType );
            Debug.Assert( _parameters.Count == r.Parameters.Count );
            
            bool hasChanged = false;
            for( int i = 0; i < _parameters.Count; ++i )
            {
                hasChanged |= _parameters[i].Merge( r.Parameters[i] );
            }
            return hasChanged;
        }

        public int CompareTo(SimpleMethodInfo other)
        {
            if (this == other) return 0;
            int cmp = this.GetSimpleSignature().CompareTo(other.GetSimpleSignature());
            if (cmp == 0) cmp = _returnType.CompareTo(other.ReturnType);
            return cmp;
        }

    }
}
