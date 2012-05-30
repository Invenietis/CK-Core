#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Host\Service\LogMethodError.cs) is part of CiviKey. 
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

namespace CK.Plugin.Hosting
{
    /// <summary>
    /// Used for error that occured in a non-logged method.
    /// </summary>
    class LogMethodError : LogHostEventArgs, ILogMethodError
    {
        int _depth;
        MethodInfo _method;
        Exception _exception;

        internal LogMethodError( int lsn, int depth, MethodInfo m, Exception ex )
        {
            LSN = lsn;
            _depth = depth;
            _method = m;
            _exception = ex;
        }

        public override LogEntryType EntryType
        {
            get { return LogEntryType.MethodError; }
        }

        public override int Depth
        {
            get { return _depth; }
        }

        public MethodInfo Method
        {
            get { return _method; }
        }

        public MemberInfo Culprit
        {
            get { return _method; }
        }

        public Exception Error
        {
            get { return _exception; }
        }

        public override MemberInfo Member
        {
            get { return _method; }
        }
       
        public ILogMethodEntry MethodEntry 
        {
            get { return null; }
        }

    }
}
