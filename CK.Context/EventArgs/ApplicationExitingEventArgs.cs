#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\EventArgs\ApplicationExitingEventArgs.cs) is part of CiviKey. 
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
    /// The argument of <see cref="IContext.ApplicationExiting"/> event.
    /// </summary>
    public class ApplicationExitingEventArgs : ApplicationExitedEventArgs
    {
        /// <summary>
        /// Gets or sets whether this closing request should be canceled or not.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="ApplicationExitedEventArgs"/>.
        /// </summary>
        /// <param name="ctx">The source context.</param>
        /// <param name="hostShouldExit">See <see cref="HostShouldExit"/>.</param>
        public ApplicationExitingEventArgs( IContext ctx, bool hostShouldExit )
            : base( ctx, hostShouldExit )
        {
        }
    }
}
