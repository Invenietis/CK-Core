#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActionSequence\ActionSequence.cs) is part of CiviKey. 
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

namespace CK.Storage
{
    /// <summary>
    /// Implementation of <see cref="IActionSequence"/>. 
    /// Actions are executed when one of the <see cref="Run"/> methods is called and in the order they have been added.
    /// An action that is executing can <see cref="M:Append"/> a new action that will be executed later
    /// but during the same current <see cref="Run"/> call.
    /// </summary>
    /// <remarks>
    /// This is a low level implementation: no cycle nor call reentrancy detection are made. It is 
    /// up to the developper to correctly append the actions to the sequence.
    /// </remarks>
    public class ActionSequence : IActionSequence
    {
        #region Implementation
        interface IAction
        {
            IAction Next { get; set; }
            void Run();
        }

        class SE : IAction
        {
            IAction IAction.Next { get; set; }
            void IAction.Run() { A( Source, Args ); }
            public EventHandler A;
            public object Source;
            public EventArgs Args;
        }

        class SE<T> : IAction where T : EventArgs
        {
            IAction IAction.Next { get; set; }
            void IAction.Run() { A( Source, Args ); }
            public EventHandler<T> A;
            public object Source;
            public T Args;
        }

        class S0 : IAction
        {
            IAction IAction.Next { get; set; }
            void IAction.Run() { A(); }
            public Action A;
        }

        class S1<T> : IAction
        {
            IAction IAction.Next { get; set; }
            void IAction.Run() { A( P1 ); }
            public Action<T> A;
            public T P1;
        }

        class S2<T1, T2> : IAction
        {
            IAction IAction.Next { get; set; }
            void IAction.Run() { A( P1, P2 ); }
            public Action<T1,T2> A;
            public T1 P1;
            public T2 P2;
        }

        class S3<T1, T2, T3> : IAction
        {
            IAction IAction.Next { get; set; }
            void IAction.Run() { A( P1, P2, P3 ); }
            public Action<T1,T2,T3> A;
            public T1 P1;
            public T2 P2;
            public T3 P3;
        }

        IAction _first;
        IAction _last;
        bool _readOnly;

        void Append( IAction a )
        {
            if( _readOnly ) throw new Exception( R.ActionSequenceReadOnly );
            if( _first == null ) _first = _last = a;
            else _last.Next = _last = a;
        }

        #endregion

        /// <summary>
        /// Gets or sets a boolean that states whether this sequence is read-only. 
        /// When a sequence is read-only, any attempt to append an action is an error (an
        /// exception will be thrown).
        /// Defaults to false.
        /// </summary>
        public bool ReadOnly 
        { 
            get { return _readOnly; }
            set { _readOnly = value; } 
        }

        /// <summary>
        /// Appends an event raising.
        /// </summary>
        /// <param name="e">The <see cref="EventHandler"/> delegate.</param>
        /// <param name="source">Source of the event.</param>
        /// <param name="eventArgs">Event argument.</param>
        public void Append( EventHandler e, object source, EventArgs eventArgs )
        {
            Append( new SE() { A = e, Source = source, Args = eventArgs } );
        }

        /// <summary>
        /// Appends an event raising.
        /// </summary>
        /// <typeparam name="T">Must be a class that inherits from <see cref="EventArgs"/>.</typeparam>
        /// <param name="e">The <see cref="EventHandler{T}"/> delegate.</param>
        /// <param name="source">Source of the event.</param>
        /// <param name="eventArgs">Event argument.</param>
        public void Append<T>( EventHandler<T> e, object source, T eventArgs ) where T : EventArgs
        {
            Append( new SE<T>() { A = e, Source = source, Args = eventArgs } );
        }

        /// <summary>
        /// Appends an action (without parameters).
        /// </summary>
        /// <param name="action">No parameter <see cref="Action"/> delegate.</param>
        public void Append( Action action )
        {
            Append( new S0() { A = action } );
        }

        /// <summary>
        /// Appends an action (with one parameter).
        /// </summary>
        /// <param name="action">One parameter <see cref="Action"/> delegate.</param>
        /// <param name="parameter">Action parameter.</param>
        public void Append<T>( Action<T> action, T parameter )
        {
            Append( new S1<T>() { A = action, P1 = parameter } );
        }

        /// <summary>
        /// Appends an action (with two parameters).
        /// </summary>
        /// <param name="action">Two parameters <see cref="Action"/> delegate.</param>
        /// <param name="p1">First action parameter.</param>
        /// <param name="p2">Second action parameter.</param>
        public void Append<T1, T2>( Action<T1, T2> action, T1 p1, T2 p2 )
        {
            Append( new S2<T1, T2>() { A = action, P1 = p1, P2 = p2 } );
        }

        /// <summary>
        /// Appends an action (with three parameters).
        /// </summary>
        /// <param name="action">Three parameters <see cref="Action"/> delegate.</param>
        /// <param name="p1">First action parameter.</param>
        /// <param name="p2">Second action parameter.</param>
        /// <param name="p3">Third action parameter.</param>
        public void Append<T1, T2, T3>( Action<T1, T2, T3> action, T1 p1, T2 p2, T3 p3 )
        {
            Append( new S3<T1, T2, T3>() { A = action, P1 = p1, P2 = p2, P3 = p3 } );
        }

        /// <summary>
        /// Removes all recorded actions.
        /// </summary>
        public void Clear()
        {
            _first = _last = null;
        }

        /// <summary>
        /// Invokes the sequence of actions and keep them in this sequence.
        /// </summary>
        public void Run()
        {
            IAction a = _first;
            while( a != null )
            {
                a.Run();
                a = a.Next;
            }
        }
        
        /// <summary>
        /// Invokes the sequence of actions and forget them as soon as they are executed.
        /// </summary>
        public void RunOnce()
        {
            while( _first != null )
            {
                _first.Run();
                _first = _first.Next;
            }
        }

        /// <summary>
        /// Invokes the sequence of actions and forget them as soon as they are executed.
        /// Traps any exception and continues the execution.
        /// </summary>
        /// <returns>A non empty list of exceptions if (at least) one error occured, null in no error occured.</returns>
        public IList<Exception> RunOnceSafe()
        {
            List<Exception> errors = null;
            while( _first != null )
            {
                try
                {
                    _first.Run();
                }
                catch( Exception ex )
                {
                    if( errors == null ) errors = new List<Exception>();
                    errors.Add( ex );
                }
                _first = _first.Next;
            }
            return errors;
        }
    }
}
