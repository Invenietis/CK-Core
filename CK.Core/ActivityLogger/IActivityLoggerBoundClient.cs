#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityLogger\IActivityLoggerBoundClient.cs) is part of CiviKey. 
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

namespace CK.Core
{
    /// <summary>
    /// Specialized <see cref="IActivityLoggerClient"/> that is bound to one <see cref="IActivityLogger"/>.
    /// Clients that can not be registered into multiple outputs (and receive logs from multiple loggers at the same time) should implement this 
    /// interface in order to control their registration/uregistration.
    /// </summary>
    public interface IActivityLoggerBoundClient : IActivityLoggerClient
    {
        /// <summary>
        /// Called by <see cref="IActivityLoggerOutput"/> when registering or unregistering
        /// this client.
        /// </summary>
        /// <param name="source">The logger that will send log.</param>
        /// <param name="forceBuggyRemove">True if this method MUST allow the new source without any exceptions: this is used with a null <paramref name="source"/> to
        /// remove this client because one of its method throwed an exception.</param>
        void SetLogger( IActivityLogger source, bool forceBuggyRemove );
    }
}
