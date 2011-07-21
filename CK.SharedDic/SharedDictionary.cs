using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin.Config;

namespace CK.SharedDic
{
    /// <summary>
    /// Factory for <see cref="ISharedDictionary"/> objects.
    /// </summary>
    public static class SharedDictionary
    {
        /// <summary>
        /// Creates a new <see cref="ISharedDictionary"/>.
        /// </summary>
        /// <param name="serviceProvider">Optional service provider.</param>
        /// <returns>An implementation of a <see cref="ISharedDictionary"/>.</returns>
        static public ISharedDictionary Create( IServiceProvider serviceProvider )
        {
            return new SharedDictionaryImpl( serviceProvider );
        }
    }
}
