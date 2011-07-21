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
    public class RunnerRequirementsSnapshot : IReadOnlyCollection<RequirementLayerSnapshot>
    {
        IList<RequirementLayerSnapshot> _layers;
        Dictionary<object,SolvedConfigStatus> _final;

        internal RunnerRequirementsSnapshot( RunnerRequirements cfg )
        {
            _layers = new RequirementLayerSnapshot[ cfg.Count ];
            int i = 0;
            foreach( RequirementLayer l in cfg ) _layers[i++] = new RequirementLayerSnapshot( l );
            _final = cfg.CreateFinalConfigSnapshot();
        }

        internal Dictionary<object, SolvedConfigStatus> FinalConfigSnapshot 
        { 
            get { return _final; } 
        }
 
        public SolvedConfigStatus FinalRequirement( Guid pluginId )
        {
            return _final.GetValueWithDefault( pluginId, SolvedConfigStatus.Optional );
        }

        public SolvedConfigStatus FinalRequirement( string serviceFullName )
        {
            return _final.GetValueWithDefault( serviceFullName, SolvedConfigStatus.Optional );
        }
        
        public bool Contains( object item )
        {
            RequirementLayerSnapshot s = item as RequirementLayerSnapshot;
            return s != null ? _layers.IndexOf( s ) >= 0 : false;
        }

        public int Count
        {
            get { return _layers.Count; }
        }

        public IEnumerator<RequirementLayerSnapshot> GetEnumerator()
        {
            return _layers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _layers.GetEnumerator();
        }

    }
}
