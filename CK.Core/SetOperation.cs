using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Core
{
    /// <summary>
    /// Defines the six basic operations available between two sets.
    /// </summary>
    public enum SetOperation
    {
        /// <summary>
        /// No operation.
        /// </summary>
        None,
        
        /// <summary>
        /// Union of the sets (keeps items of first or second set).
        /// </summary>
        Union,
        
        /// <summary>
        /// Intersection of the sets (keeps only items simultaneously that belong to both sets).
        /// </summary>
        Intersect,
        
        /// <summary>
        /// Exclusion (keeps only items of the first that do not belong to the second one).
        /// </summary>
        Except,
        
        /// <summary>
        /// Symetric exclusion (keeps items that belong to first or second set but not to both).
        /// </summary>
        SymetricExcept,

        /// <summary>
        /// Replace the first set by the second one.
        /// </summary>
        Replace
    }

}
