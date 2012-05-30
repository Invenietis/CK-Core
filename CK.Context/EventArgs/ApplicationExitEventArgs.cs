#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\EventArgs\ApplicationExitEventArgs.cs) is part of CiviKey. 
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

namespace CK.Context
{
    /// <summary>
    /// The argument of <see cref="IContext.ApplicationExited"/> event.
    /// Whatever the <see cref="HostShouldExit"/> value is, this event indicates the death of the <see cref="IContext"/> that emits it:
    /// any reference to the context should be released.
    /// </summary>
    public class ApplicationExitedEventArgs : ContextEventArgs
    {
        /// <summary>
        /// This parameter actually concerns the application host: plugins have no real reasons to take it into account.
        /// When true, the host should leave: this is typically triggered by an "Exit" button in a plugin.
        /// </summary>
        public bool HostShouldExit { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="ApplicationExitedEventArgs"/>.
        /// </summary>
        /// <param name="ctx">The source context.</param>
        /// <param name="hostShouldExit">See <see cref="HostShouldExit"/>.</param>
        public ApplicationExitedEventArgs( IContext ctx, bool hostShouldExit )
            : base( ctx )
        {
            HostShouldExit = hostShouldExit;
        }
    }

}
