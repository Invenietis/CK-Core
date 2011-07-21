using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Plugin
{
    /// <summary>
    /// Carries extension methods for <see cref="N:CK.Plugin"/> interfaces and classes.
    /// </summary>
    public static class PluginModelExtension
    {
        /// <summary>
        /// Adds a <see cref="RequirementLayer"/>. 
        /// The same requirements layer can be added multiple times. 
        /// Only the last (balanced) call to <see cref="PluginModelExtension.Remove(ISimplePluginRunner,RequirementLayer)">Remove</see> will actually remove the layer.
        /// </summary>
        /// <param name="runner">This <see cref="ISimplePluginRunner"/>.</param>
        /// <param name="r">The requirements layer to add.</param>
        public static void Add( this ISimplePluginRunner runner, RequirementLayer r )
        {
            runner.Add( r, true );
        }

        /// <summary>
        /// Removes one <see cref="RequirementLayer"/>. 
        /// Use <see cref="ISimplePluginRunner.Remove(RequirementLayer,bool)"/> to force the remove regardless of the number of times it has been <see cref="ISimplePluginRunner.Add">added</see>.
        /// </summary>
        /// <param name="runner">This <see cref="ISimplePluginRunner"/>.</param>
        /// <param name="r">The requirements layer to remove.</param>
        /// <returns>True if the layer has been found, false otherwise.</returns>
        public static bool Remove( this ISimplePluginRunner runner, RequirementLayer r )
        {
            return runner.Remove( r, false );
        }

    }
}
