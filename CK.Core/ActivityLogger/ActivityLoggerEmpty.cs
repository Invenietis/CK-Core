#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityLogger\ActivityLoggerEmpty.cs) is part of CiviKey. 
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// An implementation of <see cref="IActivityLogger"/> that does nothing (null object design pattern).
    /// Can be specialized. 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ActivityLoggerEmpty : IActivityLogger
    {
        /// <summary>
        /// Empty <see cref="IActivityLogger"/> (null object design pattern).
        /// </summary>
        static public readonly IActivityLogger Empty = new ActivityLoggerEmpty();

        CKTrait IActivityLogger.AutoTags
        {
            get { return ActivityLogger.RegisteredTags.EmptyTrait; }
            set { }
        }

        LogLevelFilter IActivityLogger.Filter
        {
            get { return LogLevelFilter.Off; }
            set { }
        }

        IActivityLogger IActivityLogger.UnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc, Exception ex )
        {
            return this;
        }

        IDisposable IActivityLogger.OpenGroup( CKTrait tags, LogLevel level, Func<string> getConclusionText, string text, DateTime logTimeUtc, Exception ex )
        {
            return Util.EmptyDisposable;
        }

        void IActivityLogger.CloseGroup( DateTime logTimeUtc, object conclusion )
        {
        }

        IActivityLoggerOutput IActivityLogger.Output
        {
            get { return ActivityLoggerOutput.Empty; }
        }

    }
}
