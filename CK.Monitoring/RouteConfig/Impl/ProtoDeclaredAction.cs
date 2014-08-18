#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Monitoring\RouteConfig\Impl\ProtoDeclaredAction.cs) is part of CiviKey. 
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace CK.RouteConfig.Impl
{
    class ProtoDeclaredAction
    {
        ActionConfiguration _action;
        bool _isCloned;

        public ProtoDeclaredAction( ActionConfiguration a )
        {
            _action = a;
        }

        public ActionConfiguration Action
        {
            get { return _action; }
        }

        internal bool Override( IActivityMonitor monitor, IReadOnlyList<string> fullPath, ActionConfiguration a )
        {
            var e = fullPath.GetEnumerator();
            if( !e.MoveNext() ) throw new ArgumentException( "Must not be empty.", "fullPath" );
            if( e.Current != _action.Name ) throw new ArgumentException( "Must start with the action name.", "fullPath" );
            if( !e.MoveNext() )
            {
                _action = a;
                _isCloned = false;
                monitor.Info().Send( "Action '{0}' has been overridden.", a.Name );
                return true;
            }
            ActionCompositeConfiguration parent;
            int idx = FindInComposite( e, out parent );
            if( idx >= 0 )
            {
                Debug.Assert( _action is ActionCompositeConfiguration, "It is a composite." );
                Debug.Assert( _action.IsCloneable, "A composite is cloneable." );
                if( !_isCloned )
                {
                    _action = ((ActionCompositeConfiguration)_action).CloneComposite( true );
                    monitor.Info().Send( "Action '{0}' has been cloned in order to override an inner action.", String.Join( "/", fullPath ) );
                    _isCloned = true;
                    idx = FindInComposite( e, out parent );
                }
                Debug.Assert( parent.Children[idx].Name == fullPath.Last() );
                parent.Override( idx, a );
                monitor.Info().Send( "Inner action '{0}' has been overridden.", String.Join( "/", fullPath ) );
                return true;
            }
            monitor.Error().Send( "Action '{0}' not found. Unable to override it.", String.Join( "/", fullPath ) );
            return false;
        }

        int FindInComposite( IEnumerator<string> path, out ActionCompositeConfiguration parent )
        {
            parent = null;
            ActionCompositeConfiguration composite = _action as ActionCompositeConfiguration;
            if( composite == null ) return -1;
            return FindInComposite( composite, path, ref parent );
        }

        static int FindInComposite( ActionCompositeConfiguration start, IEnumerator<string> path, ref ActionCompositeConfiguration parent )
        {
            string current = path.Current;
            for( int i = 0; i < start.Children.Count; ++i )
            {
                if( start.Children[i].Name == current )
                {
                    if( !path.MoveNext() )
                    {
                        parent = start;
                        return i;
                    }
                    ActionCompositeConfiguration newStart = start.Children[i] as ActionCompositeConfiguration;
                    if( newStart == null ) return -1;
                    return FindInComposite( newStart, path, ref parent );
                }
            }
            return -1;
        }

    }
}
