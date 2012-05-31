#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Runner\RunnerRequirements.cs) is part of CiviKey. 
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
using CK.Plugin.Config;
using CK.Core;
using System.Collections;
using System.Diagnostics;

namespace CK.Plugin.Hosting
{

    public class RunnerRequirements : IReadOnlyCollection<RequirementLayer>
    {
        PluginRunner _runner;
        List<RequirementLayer> _layers;

        // Holds final result. Associated to Service and Plugins.
        class FinalResult
        {
            // Bound to the discovered info: it can be a IServiceInfo or a IPluginInfo, or null if it does not exist.
            public IDiscoveredInfo Info { get; private set; }
            // Memorizes any load/runtime error (always null for Service).
            public IExecutionPlanResult RunningError { get; set; }
            // Final status depends on configuration, requirements, discoverer and running error.
            public SolvedConfigStatus FinalStatus { get; private set; }
            // Current running status.
            public SolvedConfigStatus RunningStatus { get; private set; }

            /// <summary>
            /// A new FinalResult is created the first time the PluginId is met.
            /// Since we do not even know if it exists in the discoverer, we set its 
            /// status to "unknown" (-1). 
            /// If it is not here, nothing changed if the status is Disabled.
            /// If it is available, nothing changed if the status is Optional.
            /// </summary>
            public FinalResult()
            {
                FinalStatus = (SolvedConfigStatus)(-1);
                RunningStatus = (SolvedConfigStatus)(-1);
            }

            public bool UpdatePlugin( Guid pluginId, bool runnerDisabled, ISolvedPluginConfiguration config, IEnumerable<RequirementLayer> layers, IPluginDiscoverer d )
            {
                SolvedConfigStatus s;
                Info = d.FindPlugin( pluginId );
                bool isAvailable = !(Info == null || Info.HasError || RunningError != null);
                if( isAvailable )
                {
                    if( runnerDisabled ) s = SolvedConfigStatus.Disabled;
                    else
                    {
                        s = config.GetStatus( pluginId );
                        foreach( RequirementLayer l in layers )
                        {
                            PluginRequirement r = l.PluginRequirements.Find( pluginId );
                            if( r != null && (int)r.Requirement > (int)s ) s = (SolvedConfigStatus)r.Requirement;
                        }
                    }
                }
                else s = SolvedConfigStatus.Disabled;
                return DoUpdateStatus( isAvailable, s );
            }

            public bool UpdateService( string serviceFullName, bool runnerDisabled, IEnumerable<RequirementLayer> layers, IPluginDiscoverer d )
            {
                Debug.Assert( RunningError == null, "A service can not be in running error." );
                SolvedConfigStatus s;
                Info = d.FindService( serviceFullName );
                bool isAvailable = !(Info == null || Info.HasError);
                if( isAvailable ) 
                {
                    if( runnerDisabled ) s = SolvedConfigStatus.Disabled;
                    else
                    {
                        s = SolvedConfigStatus.Optional;
                        foreach( RequirementLayer l in layers )
                        {
                            ServiceRequirement r = l.ServiceRequirements.Find( serviceFullName );
                            if( r != null && (int)r.Requirement > (int)s ) s = (SolvedConfigStatus)r.Requirement;
                        }
                    }
                }
                else s = SolvedConfigStatus.Disabled;
                return DoUpdateStatus( isAvailable, s );
            }

            public void SetRunningStatus( SolvedConfigStatus configured )
            {
                RunningStatus = configured;
            }

            private bool DoUpdateStatus( bool isAvailable, SolvedConfigStatus s )
            {
                if( FinalStatus == (SolvedConfigStatus)(-1) )
                {
                    // Initialization, two cases:
                    // - The plugin/service is available: we must trigger a change only if the SolvedConfigStatus differs from Optional.
                    // - The plugin/service is NOT available: we must trigger a change only if the SolvedConfigStatus differs from Disabled.
                    FinalStatus = isAvailable ? SolvedConfigStatus.Optional : SolvedConfigStatus.Disabled;
                    RunningStatus = FinalStatus;
                }
                // Already initialized.
                if( s != FinalStatus )
                {
                    FinalStatus = s;
                    return true;
                }
                return false;
            }


        }
        // Key is either a plugin identifier or a service full name.
        Dictionary<object,FinalResult> _final;
        int _nbFinalDifferFromRunning;
        bool _runnerDisabled;

        internal RunnerRequirements( PluginRunner runner )
        {
            _layers = new List<RequirementLayer>();
            _final = new Dictionary<object, FinalResult>();
            _runner = runner;
            _runner.ConfigManager.SolvedPluginConfiguration.Changed += SolvedPluginConfigurationChanged;
        }

        /// <summary>
        /// Initialize the internal count of differences and set RunningConfiguration.IsDirty accordingly.
        /// </summary>
        internal void Initialize()
        {
            Debug.Assert( _nbFinalDifferFromRunning == 0 );
            foreach( var c in _runner.ConfigManager.SolvedPluginConfiguration )
            {
                FinalResult r = new FinalResult();
                _final.Add( c.PluginId, r );
                Debug.Assert( r.FinalStatus == r.RunningStatus, "At creation time, they both are Optional." );
                if( r.UpdatePlugin( c.PluginId, _runnerDisabled, _runner.ConfigManager.SolvedPluginConfiguration, _layers, _runner.Discoverer ) )
                {
                    Debug.Assert( r.FinalStatus != r.RunningStatus, "If FinalStatus changed, then it necessarily differs from RunningStatus." );
                    ++_nbFinalDifferFromRunning;
                }
            }
            _runner.SetDirty( _nbFinalDifferFromRunning > 0 );
        }

        internal bool RunnerDisabled
        {
            get { return _runnerDisabled; }
            set 
            {
                if( _runnerDisabled != value )
                {
                    _runnerDisabled = value;
                    UpdateAll();
                }
            }
        }

        void SolvedPluginConfigurationChanged( object sender, SolvedPluginConfigurationChangedEventArs e )
        {
            if( e.SolvedPluginConfigElement != null ) Update( e.SolvedPluginConfigElement.PluginId );
            else foreach( var c in _runner.ConfigManager.SolvedPluginConfiguration ) Update( c.PluginId );
        }

        public bool Contains( object item )
        {
            RequirementLayer r = item as RequirementLayer;
            return r != null ? _layers.IndexOf( r ) >= 0 : false;
        }

        public int Count
        {
            get { return _layers.Count; }
        }

        public void Add( RequirementLayer r )
        {
            Add( r, true );
        }

        public bool Add( RequirementLayer r, bool allowDuplicate )
        {
            if( r == null ) throw new ArgumentNullException( "r" );
            int i = _layers.IndexOf( r );
            if( i < 0 )
            {
                _layers.Add( r );
                r.PluginRequirements.Changed += PluginRequirementChanged;
                r.ServiceRequirements.Changed += ServiceRequirementChanged;
                foreach( PluginRequirement p in r.PluginRequirements ) Update( p.PluginId );
                foreach( ServiceRequirement s in r.ServiceRequirements ) Update( s.AssemblyQualifiedName );
            }
            else
            {
                if( !allowDuplicate ) return false;
                _layers.Add( r );
            }
            return true;
        }

        public bool Remove( RequirementLayer r )
        {
            return Remove( r, false );
        }

        public bool Remove( RequirementLayer r, bool removeAll )
        {
            if( r == null ) throw new ArgumentNullException( "r" );
            int i = _layers.LastIndexOf( r );
            if( i < 0 ) return false;
            _layers.RemoveAt( i );
            if( i == 0 ) removeAll = true;
            else
            {
                i = _layers.LastIndexOf( r, i - 1 );
                if( i < 0 ) removeAll = true;
                else if( removeAll )
                {
                    do
                    {
                        _layers.RemoveAt( i );
                        i = i > 0 ? _layers.LastIndexOf( r, i - 1 ) : -1;
                    }
                    while( i >= 0 );
                }
            }

            if( removeAll )
            {
                r.PluginRequirements.Changed -= PluginRequirementChanged;
                r.ServiceRequirements.Changed -= ServiceRequirementChanged;
                foreach( PluginRequirement p in r.PluginRequirements ) Update( p.PluginId );
                foreach( ServiceRequirement s in r.ServiceRequirements ) Update( s.AssemblyQualifiedName );
            }
            return true;
        }

        public void ClearRunningError( Guid pluginId )
        {
            FinalResult r;
            if( _final.TryGetValue( pluginId, out r ) && r.RunningError != null )
            {
                r.RunningError = null;
                Update( pluginId, r );
            }
        }

        internal void SetRunningError( IExecutionPlanResult error )
        {
            Debug.Assert( error.Status != ExecutionPlanResultStatus.Success && error.Culprit != null, "An error is necessarily associated to a plugin." );
            
            FinalResult r = _final.GetOrSet( error.Culprit.PluginId, g => new FinalResult() );
            bool hasErrorAlready = r.RunningError != null;
            r.RunningError = error;
            if( !hasErrorAlready ) Update( error.Culprit.PluginId, r );
        }

        void Update( Guid pluginId )
        {
            FinalResult r = _final.GetOrSet( pluginId, g => new FinalResult() );
            Update( pluginId, r );
        }

        void Update( string serviceFullName )
        {
            FinalResult r = _final.GetOrSet( serviceFullName, g => new FinalResult() );
            Update( serviceFullName, r );
        }

        void UpdateAll()
        {
            foreach( var e in _final )
            {
                FinalResult r = e.Value;
                if( e.Key is Guid ) Update( (Guid)e.Key, r );
                else Update( (string)e.Key, r );
            }
        }

        void Update( Guid pluginId, FinalResult r )
        {
            if( r.UpdatePlugin( pluginId, _runnerDisabled, _runner.ConfigManager.SolvedPluginConfiguration, _layers, _runner.Discoverer ) )
            {
                UpdateCheckDirty( r );
            }
        }

        void Update( string serviceFullName, FinalResult r )
        {
            if( r.UpdateService( serviceFullName, _runnerDisabled, _layers, _runner.Discoverer ) )
            {
                UpdateCheckDirty( r );
            }
        }

        private void UpdateCheckDirty( FinalResult r )
        {
            if( r.FinalStatus != r.RunningStatus ) ++_nbFinalDifferFromRunning;
            else --_nbFinalDifferFromRunning;
            Debug.Assert( _nbFinalDifferFromRunning >= 0, "There can't be less than zero differences.." );
            _runner.SetDirty( _nbFinalDifferFromRunning > 0 );
        }

        void ServiceRequirementChanged( object sender, ServiceRequirementCollectionChangedEventArgs e )
        {
            Debug.Assert( _layers.Exists( l => l.ServiceRequirements == e.Collection ), "The source of the event belongs to our _layers." );
            if( e.AssemblyQualifiedName != null ) Update( e.AssemblyQualifiedName );
            else foreach( ServiceRequirement s in e.Collection ) Update( s.AssemblyQualifiedName );
        }

        void PluginRequirementChanged( object sender, PluginRequirementCollectionChangedEventArgs e )
        {
            Debug.Assert( _layers.Exists( l => l.PluginRequirements == e.Collection ), "The source of the event belongs to our _layers." );
            if( e.PluginId != Guid.Empty ) Update( e.PluginId );
            else foreach( PluginRequirement p in e.Collection ) Update( p.PluginId );
        }

        public IEnumerator<RequirementLayer> GetEnumerator()
        {
            return _layers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public SolvedConfigStatus FinalRequirement( Guid pluginId )
        {
            FinalResult r;
            if( _final.TryGetValue( pluginId, out r ) ) return r.FinalStatus;
            return SolvedConfigStatus.Optional;
        }

        public SolvedConfigStatus FinalRequirement( string serviceFullName )
        {
            FinalResult r;
            if( _final.TryGetValue( serviceFullName, out r ) ) return r.FinalStatus;
            return SolvedConfigStatus.Optional;
        }

        internal Dictionary<object, SolvedConfigStatus> CreateFinalConfigSnapshot()
        {
            Dictionary<object, SolvedConfigStatus> result = new Dictionary<object, SolvedConfigStatus>( _final.Count * 2 );
            foreach( var e in _final )
            {
                // Adds the object (serviceFullName or Guid) to final SolvedConfigStatus.
                result.Add( e.Key, e.Value.FinalStatus );
                // If a IPluginInfo or a IServiceInfo is associated, adds it as another key alias.
                if( e.Value.Info != null )
                {
                    result.Add( e.Value.Info, e.Value.FinalStatus );
                }
            }
            return result;
        }

        internal void UpdateRunningStatus( Dictionary<object, SolvedConfigStatus> finalConfigStatus )
        {
            _nbFinalDifferFromRunning = 0;
            foreach( var e in _final )
            {
                SolvedConfigStatus configured;
                if( finalConfigStatus.TryGetValue( e.Key, out configured ) )
                {
                    e.Value.SetRunningStatus( configured );
                }
                if( e.Value.RunningStatus != e.Value.FinalStatus ) ++_nbFinalDifferFromRunning;
            }
            _runner.SetDirty( _nbFinalDifferFromRunning > 0 );
        }

    }

}
