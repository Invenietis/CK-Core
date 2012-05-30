#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Host\UseTheProxyBase.cs) is part of CiviKey. 
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

namespace CK.Plugin.Hosting
{
    /// <summary>
    /// Fake internal class that forces the compiler to keep <see cref="ServiceProxyBase"/> implementation.
    /// Without it, get_Status (for instance) is not ignored at compile time and hence, not defined when 
    /// the ServiceProxyBase is used as the base class by dynamic proxies.
    /// </summary>
    class UseTheProxyBase : ServiceProxyBase, IService<CK.Plugin.IDynamicService>
    {
        UseTheProxyBase()
            : base( null, typeof( CK.Plugin.IDynamicService ), null, null )
        {
        }

        protected override object RawImpl
        {
            get { return null; }
            set { }
        }

        public CK.Plugin.IDynamicService Service
        {
            get { return null; }
        }

    }
}
