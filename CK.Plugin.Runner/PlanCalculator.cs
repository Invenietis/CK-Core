#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Runner\PlanCalculator.cs) is part of CiviKey. 
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
using System.Diagnostics;
using CK.Core;
using System.Collections;
using System.Linq;
using CK.Plugin.Config;
using System.Reflection;

namespace CK.Plugin.Hosting
{
    /// <summary>
    /// This class calculates and returns the best <see cref="ExecutionPlan"/> for a given configuration context.
    /// </summary>
    internal class PlanCalculator
    {
        class PluginData
        {
            public PluginData( IPluginInfo p, int index, bool isRunning )
            {
                PluginInfo = p;
                Index = index;
                IsRunning = isRunning;
            }
            public readonly IPluginInfo PluginInfo;
            public readonly int Index;
            public readonly bool IsRunning;
            public bool Locked;
        }
        IPluginDiscoverer _discoverer;
        List<PluginData> _mappingArray;
        Dictionary<IPluginInfo, PluginData> _mappingDic;
        Dictionary<object, SolvedConfigStatus> _finalConfig;
        BitArray _parseMap; 
        Predicate<IPluginInfo> IsPluginRunning;
        ExecutionPlan _lastBestPlan;

        /// <summary>
        /// Fields that stores the fact that something changed during an apply of the <see cref="LastBestPlan"/> and
        /// that a new plan must be computed. This is stored here since the life time of this PlanCalculator is
        /// bound to the global PluginRunner.Apply method execution and can be reused if necessary (by calling ObtainBestPlan again).
        /// </summary>
        public bool ReapplyNeeded;

        public PlanCalculator( IPluginDiscoverer discoverer, Predicate<IPluginInfo> isPluginRunning )
        {
            _discoverer = discoverer;
            _mappingArray = new List<PluginData>();
            _mappingDic = new Dictionary<IPluginInfo, PluginData>();
            IsPluginRunning = isPluginRunning;
        }

        /// <summary>
        /// Launches ComputeCombination for each "plugin status combination".
        /// Gets the lower cost among all the combinations generated.
        /// </summary>
        internal ExecutionPlan ObtainBestPlan( Dictionary<object, SolvedConfigStatus> finalConfig, bool stopLaunchedOptionals )
        {
            _finalConfig = finalConfig;
            if( _parseMap == null ) _parseMap = new BitArray( _discoverer.Plugins.Count );
            else _parseMap.Length = _discoverer.Plugins.Count;
            _mappingArray.Clear();
            _mappingDic.Clear();

            int disabledCount = 0;

            // Locking plugins : 
            // - If a plugin is disabled, it should not be launched, we do not add it to the map
            // - If a plugin needs to be started (MustExistAndRun),  we lock its value to true
            // - If a plugin is the only implemention of a service and that this service has to be started, we lock this plugin's value to true
            // - If a plugin has no service references and does not implement any services as well, and that it is not asked to be 
            //   started and it is NOT running or it is running but stopLaunchedOptionals is true, we lock its value to false;
            int index = 0;
            foreach( IPluginInfo pI in _discoverer.Plugins )
            {
                // SolvedConfigStatus of the actual plugin.
                SolvedConfigStatus pluginStatus = _finalConfig.GetValueWithDefault( pI, SolvedConfigStatus.Optional );
                if( pluginStatus == SolvedConfigStatus.Disabled )
                {
                    // If a plugin is disabled, it should not be launched, we do not add it to the map.
                    disabledCount++;
                    continue;
                }

                // SolvedConfigStatus of the implemented service if any.
                SolvedConfigStatus serviceStatus = pI.Service != null 
                    ? _finalConfig.GetValueWithDefault( pI.Service, SolvedConfigStatus.Optional ) 
                    : SolvedConfigStatus.Optional;

                if( serviceStatus == SolvedConfigStatus.Disabled )
                {
                    // If a plugin is disabled, it should not be launched, we do not add it to the map
                    disabledCount++;
                    continue;
                }

                // Here, we have no more disabled plugins.
                // Initializes a PluginData for this particular plugin and allocates
                // a new index in the bit array.
                Debug.Assert( index == _mappingArray.Count );
                PluginData pluginData = new PluginData( pI, index, IsPluginRunning( pI ) );
                _mappingArray.Add( pluginData );
                _mappingDic.Add( pI, pluginData );

                if( pluginStatus == SolvedConfigStatus.MustExistAndRun
                        || (serviceStatus == SolvedConfigStatus.MustExistAndRun && pI.Service.Implementations.Count == 1) )
                {
                    // If a plugin needs to be started (MustExistAndRun), we lock its value to true.
                    // If a plugin is the only implemention of a service and that this service has to be started, we lock this plugin's value to true.
                    _parseMap.Set( index, true );
                    pluginData.Locked = true;
                }
                else if( pI.Service == null && pI.ServiceReferences.Count == 0 ) // This plugin is independent.
                {
                    // This is only an optimization. 
                    // The cost function gives a cost to the stop or the start of a plugin. When a plugin is independant like in this case, we lock its 
                    // status by taking into account the requirement (should it run? MustExistTryStart/OptionalTryStart) and its current status (IsPluginRunning) 
                    // and the stopLaunchedOptionals boolean.
                    if( (pluginStatus != SolvedConfigStatus.MustExistTryStart || pluginStatus != SolvedConfigStatus.OptionalTryStart) 
                        && ( !pluginData.IsRunning || stopLaunchedOptionals ))
                    {
                        // If a plugin has no service references and does not implement any services as well, 
                        // and that it is not asked to be started AND it is not running, we lock its value to false;
                        _parseMap.Set( index, false );
                        // We do not set thereWillBeNoChange to false: there will ACTUALLY be no changes
                        // since the plugin is NOT running.
                    }
                    else
                    {
                        // If a plugin has no service references and does not implement any services as well, 
                        // and that it is asked to be started OR is currently running, we lock its value to true;
                        _parseMap.Set( index, true );
                    }
                    pluginData.Locked = true;
                }
                index++;
            }

            // Trim the parseMap, to remove indexes that should have been filled by disabled plugins.
            Debug.Assert( _parseMap.Length >= disabledCount );
            _parseMap.Length -= disabledCount;

            // If the parseMap has a length of 0, it means either that there are no plugins or that all plugins are disabled.
            // In either of these cases, we don't calculate any execution plan. But we still have a valid execution plan, it just doesn't have any plugins to launch.

            BitArray bestCombination = _parseMap;
            if( _parseMap.Length > 0 )
            {
                int bestCost = Int32.MaxValue;
                double combinationsCount = Math.Pow( 2, _parseMap.Length - _mappingDic.Values.Count( ( e ) => { return e.Locked == true; } ) );

                for( int i = 0; i < combinationsCount; i++ )
                {
                    int cost = ComputeCombination(stopLaunchedOptionals);
                    Debug.Assert( cost >= 0 );
                    // Return if the cost is equal to 0 (no better solution).
                    if( cost == 0 )
                    {
                        return _lastBestPlan = GenerateExecutionPlan( _parseMap );
                    }
                    if( cost < bestCost )
                    {
                        bestCost = cost;
                        bestCombination = (BitArray)_parseMap.Clone();
                    }
                    GenerateNextCombination();
                }
                // If there is no valid combination, we return an impossible plan and
                // we do not keep it as the LastBestPlan.
                if( bestCost == Int32.MaxValue ) return GenerateExecutionPlan( null );
            }
            return _lastBestPlan = GenerateExecutionPlan( bestCombination );
        }

        /// <summary>
        /// Gets the last (the current) execution plan computed by <see cref="ObtainBestPlan"/>
        /// that is not impossible
        /// </summary>
        internal ExecutionPlan LastBestPlan
        {
            get { return _lastBestPlan; }
        }

        /// <summary>
        /// Launches ComputeElementCost on each plugin that can be found in the parseMap set as parameter
        /// </summary>
        /// <returns>the cost of the parseMap set as parameter</returns>
        int ComputeCombination( bool stopLaunchedOptionals )
        {
            int cost = 0;

            //If the given parseMap launches 2 plugins that implement the same service, discard it.
            List<IServiceInfo> services = new List<IServiceInfo>();
            for( int i = 0; i < _parseMap.Count; i++ )
            {
                if( _parseMap[i] )
                {
                    IPluginInfo plugin = _mappingArray[i].PluginInfo;
                    Debug.Assert( plugin != null );
                    if( plugin.Service != null )
                    {
                        if( !services.Contains( plugin.Service ) )
                        {
                            services.Add( plugin.Service );
                        }
                        else
                        {
                            return Int32.MaxValue;
                        }
                    }
                }
            }

            for( int i = 0; i < _parseMap.Count; i++ )
            {
                int elementCost = ComputeElementCost( i, stopLaunchedOptionals );
                if( elementCost != Int32.MaxValue ) cost += elementCost;
                else return Int32.MaxValue;
            }

            Debug.Assert( cost >= 0 );

            return cost;
        }

        /// <summary>
        /// Gets the index of the plugin which cost to calculate, together with the current parseMap
        /// Returns how much launching (or not) this plugin costs.
        /// </summary>
        /// <param name="i">The index of the plugin to compute</param>
        /// <returns>The plugin's cost</returns>
        int ComputeElementCost( int i, bool stopLaunchedOptionals )
        {
            int cost = 0;

            // Gets the actual plugin's SolvedConfigStatus.
            PluginData plugin = _mappingArray[i];
            IPluginInfo actualPlugin = plugin.PluginInfo;
            Debug.Assert( actualPlugin != null );

            SolvedConfigStatus status =_finalConfig.GetValueWithDefault( actualPlugin, SolvedConfigStatus.Optional );
            if( actualPlugin.Service != null )
            {
                var serviceStatus =_finalConfig.GetValueWithDefault( actualPlugin.Service.AssemblyQualifiedName, SolvedConfigStatus.Optional );
                status = status < serviceStatus ? serviceStatus : status;
            }
            
            // If the plugin has to be started.
            if( _parseMap[i] )
            {
                // Check its references.
                foreach( IServiceReferenceInfo serviceRef in actualPlugin.ServiceReferences )
                {
                    if( !CheckReference( serviceRef, _parseMap, ref cost ) ) return int.MaxValue;
                }

                #region Check the cost regarding the plugin's requirement, when the plugin is to be started

                // If the plugin needs to be started, but its not currently running.
                // we increase the cost only if the plugin is not absolutely needed.
                if( !plugin.IsRunning )
                {
                    switch( status )
                    {
                        case SolvedConfigStatus.Optional:
                        case SolvedConfigStatus.MustExist:
                            cost += 10;
                            break;
                        case SolvedConfigStatus.Disabled:
                            return Int32.MaxValue;
                    }
                }

                #endregion
            }
            // If the plugin doesn't need to be started (0 in the parseMap)
            else
            {
                // Here we check if the plugin can be stopped

                // If the plugin implements a service, and if the service has a MustExistAndRun requirement
                // and if this plugin is the only implementation of this service, so we return the max value.
                // Otherwise, if we find a substitute, the combinaison is possible.
                if( actualPlugin.Service != null )
                {
                    bool substitue = false;

                    if( _finalConfig.GetValueWithDefault( actualPlugin.Service, SolvedConfigStatus.Optional ) == SolvedConfigStatus.MustExistAndRun )
                    {
                        for( int idx = 0; idx < _parseMap.Length; idx++ )
                        {
                            if( _parseMap[idx] 
                                && _mappingArray[idx].PluginInfo.Service != null 
                                && _mappingArray[idx].PluginInfo.Service == actualPlugin.Service )
                            {
                                substitue = true;
                                break;
                            }
                        }
                        if( !substitue ) return int.MaxValue;
                    }

                }
                else if( status == SolvedConfigStatus.MustExistAndRun )
                    return int.MaxValue;

                // If this plugin is already running and stopLaunchedOptionals is set to false, don't stop it.
                if( plugin.IsRunning && !stopLaunchedOptionals ) cost += 10;

                #region Check the cost regarding the plugin's requirement when the plugin won't be started

                // if we wanted this plugin started ... 
                if( status == SolvedConfigStatus.MustExistTryStart || status == SolvedConfigStatus.OptionalTryStart) 
                    cost += 10;

                #endregion
            }

            return cost;
        }

        bool CheckReference( IServiceReferenceInfo serviceRef, BitArray parseMap, ref int cost )
        {
            // If the reference is a reference to an external service
            if( !serviceRef.Reference.IsDynamicService )
            {
                return true; // todo check if the service is available in the service container.
            }
            // Checks if at least one of the implementations of the service is available in the current map.
            bool implAvailable = serviceRef.Reference.Implementations.Count > 0;

            // If the service is really needed and is not available, return the reference cannot be resolved.
            if( serviceRef.Requirements >= RunningRequirement.MustExist && !implAvailable ) 
                return false;
            else
            {
                Debug.Assert( serviceRef.Requirements < RunningRequirement.MustExist || implAvailable );

                bool isPossible = false;
                if( !implAvailable )
                {
                    switch( serviceRef.Requirements )
                    {
                        // the plugin is stopped but it's optional -> whatever !
                        case RunningRequirement.Optional: return true;
                        case RunningRequirement.OptionalTryStart:
                            cost += 10;
                            return true;
                    }
                }
                else
                {
                    foreach( IPluginInfo impl in serviceRef.Reference.Implementations )
                    {
                        PluginData data = _mappingDic.GetValueWithDefault( impl, null );
                        if( data != null )
                        {
                            // if the plugin is running in this map
                            if( parseMap[data.Index] )
                            {
                                isPossible = true;

                                switch( serviceRef.Requirements )
                                {
                                    // the plugin is started but we don't really need it -> +10 if its not currently running.
                                    case RunningRequirement.Optional:
                                    case RunningRequirement.MustExist:
                                        if( !IsPluginRunning( impl ) ) cost += 10;
                                        break;
                                    // the plugin is started and we want it -> +0 (its what we want)
                                    case RunningRequirement.OptionalTryStart:
                                    case RunningRequirement.MustExistTryStart:
                                    case RunningRequirement.MustExistAndRun:
                                        break;
                                }
                            }
                            else
                            {
                                switch( serviceRef.Requirements )
                                {
                                    // the plugin is stopped but it's optional -> whatever !
                                    case RunningRequirement.Optional:
                                    case RunningRequirement.MustExist:
                                        isPossible = true;
                                        break;
                                    // the plugin is stopped but we wants it -> +10
                                    case RunningRequirement.OptionalTryStart:
                                        cost += 10;
                                        break;
                                    // the plugin is stopped but we absolutely needs it -> impossible.
                                    case RunningRequirement.MustExistTryStart:
                                    case RunningRequirement.MustExistAndRun:
                                        isPossible = false;
                                        break;
                                }
                            }
                        }
                        else
                            isPossible = false;
                    }
                }
                return isPossible;
            }
        }

        /// <summary>
        /// Returns the ExecutionPlan corresponding to a combination.
        /// </summary>
        /// <param name="bestCombination">The best plugin starting combination. Null for impossible plan.</param>        
        ExecutionPlan GenerateExecutionPlan( BitArray bestCombination )
        {
            if( bestCombination == null ) return new ExecutionPlan();
            List<IPluginInfo> start = new List<IPluginInfo>();
            List<IPluginInfo> stop = new List<IPluginInfo>();

            for( int i = 0; i < bestCombination.Count; i++ )
            {
                if( bestCombination[i] ) start.Add( _mappingArray[i].PluginInfo );
                else stop.Add( _mappingArray[i].PluginInfo );
            }

            Debug.Assert( (start.Count + stop.Count) == _mappingDic.Count );

            return new ExecutionPlan( start, stop, _discoverer.Plugins.Except( start.Concat( stop ) ).ToReadOnlyCollection() );
        }

        /// <summary>
        /// Generates the next "plugin status combination"
        /// </summary>
        BitArray GenerateNextCombination()
        {
            IncrementBitArray( 0 );
            return _parseMap;
        }

        /// <summary>
        /// Adds 1 to the parsed map. it gets the next "plugin status combination".
        /// Called this method with 0 as parameter, if you want to get a combination taking every plugin into account.
        /// </summary>
        /// <param name="i">set 0 if you want a combination taking every plugin into account.</param>
        void IncrementBitArray( int i )
        {
            if( !_mappingArray[i].Locked )
            {
                if( _parseMap[i] )
                {
                    _parseMap[i] = false;
                    if( ++i == _parseMap.Count )
                    {
                        return;
                    }
                    IncrementBitArray( i );
                }
                else
                {
                    _parseMap[i] = true;
                }
            }
            else if( ++i < _parseMap.Count )
            {
                IncrementBitArray( i );
            }
        }
    }
}
