#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActionSequence\ActionSequenceExtension.cs) is part of CiviKey. 
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

namespace CK.Storage
{
    /// <summary>
    /// Holds extensions methods of <see cref="IActionSequence"/> interface.
    /// </summary>
    static public class ActionSequenceExtension
	{
        /// <summary>
        /// <see cref="IActionSequence.Append(Action)">Appends</see> the action to this <see cref="IActionSequence"/> if possible, or executes it immediately.
        /// </summary>
        /// <param name="s">This <see cref="IActionSequence"/>. Can be null.</param>
        /// <param name="action">The action to execute. Can be null: in such case nothing is done.</param>
        static public void NowOrLater( this IActionSequence s, Action action )
        {
            if( action != null )
            {
                if( s != null && !s.ReadOnly ) s.Append( action );
                else action();
            }
        }

        /// <summary>
        /// <see cref="IActionSequence.Append{T}(Action{T},T)">Appends</see> the action to this <see cref="IActionSequence"/> if possible, or executes it immediately.
        /// </summary>
        /// <param name="s">This <see cref="IActionSequence"/>. Can be null.</param>
        /// <param name="p">Parameter of the action.</param>
        /// <param name="action">The action to execute. Can be null: in such case nothing is done.</param>
        static public void NowOrLater<T>( this IActionSequence s, Action<T> action, T p )
        {
            if( action != null )
            {
                if( s != null && !s.ReadOnly ) s.Append( action, p );
                else action( p );
            }
        }

        /// <summary>
        /// <see cref="IActionSequence.Append{T1,T2}(Action{T1,T2},T1,T2)">Appends</see> the action to this <see cref="IActionSequence"/> if possible, 
        /// or executes it immediately.
        /// </summary>
        /// <param name="s">This <see cref="IActionSequence"/>. Can be null.</param>
        /// <param name="action">The action to execute. Can be null: in such case nothing is done.</param>
        /// <param name="p1">First action parameter.</param>
        /// <param name="p2">Second action parameter.</param>
        static public void NowOrLater<T1, T2>( this IActionSequence s, Action<T1, T2> action, T1 p1, T2 p2 )
        {
            if( action != null )
            {
                if( s != null && !s.ReadOnly ) s.Append( action, p1, p2 );
                else action( p1, p2 );
            }
        }

        /// <summary>
        /// <see cref="IActionSequence.Append{T1,T2,T3}(Action{T1,T2,T3},T1,T2,T3)">Appends</see> the action to 
        /// this <see cref="IActionSequence"/> if possible, or executes it immediately.
        /// </summary>
        /// <param name="s">This <see cref="IActionSequence"/>. Can be null.</param>
        /// <param name="action">The action to execute. Can be null: in such case nothing is done.</param>
        /// <param name="p1">First action parameter.</param>
        /// <param name="p2">Second action parameter.</param>
        /// <param name="p3">Third action parameter.</param>
        static public void NowOrLater<T1, T2, T3>( this IActionSequence s, Action<T1, T2, T3> action, T1 p1, T2 p2, T3 p3 )
        {
            if( action != null )
            {
                if( s != null && !s.ReadOnly ) s.Append( action, p1, p2, p3 );
                else action( p1, p2, p3 );
            }
        }

        /// <summary>
        /// <see cref="IActionSequence.Append(EventHandler,object,EventArgs)">Appends</see> an event raising to this <see cref="IActionSequence"/> if possible, or executes it immediately.
        /// </summary>
        /// <param name="s">This <see cref="IActionSequence"/>. Can be null.</param>
        /// <param name="e">The <see cref="EventHandler"/> delegate.</param>
        /// <param name="source">Source of the event.</param>
        /// <param name="eventArgs">Event argument.</param>
        static public void NowOrLater( this IActionSequence s, EventHandler e, object source, EventArgs eventArgs )
        {
            if( e != null )
            {
                if( s != null && !s.ReadOnly ) s.Append( e, source, eventArgs );
                else e( source, eventArgs );
            }
        }

        /// <summary>
        /// <see cref="IActionSequence.Append{T}(EventHandler{T},object,T)">Appends</see> an event raising to this <see cref="IActionSequence"/> if possible, or executes it immediately.
        /// </summary>
        /// <typeparam name="T">Must be a class that inherits from <see cref="EventArgs"/>.</typeparam>
        /// <param name="s">This <see cref="IActionSequence"/>. Can be null.</param>
        /// <param name="e">The <see cref="EventHandler{T}"/> delegate.</param>
        /// <param name="source">Source of the event.</param>
        /// <param name="eventArgs">Event argument.</param>
        static public void NowOrLater<T>( this IActionSequence s, EventHandler<T> e, object source, T eventArgs ) where T : EventArgs
        {
            if( e != null )
            {
                if( s != null && !s.ReadOnly ) s.Append( e, source, eventArgs );
                else e( source, eventArgs );
            }
        }


    }
}
