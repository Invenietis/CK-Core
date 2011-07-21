using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Plugin.Config
{
    public static class ConfigurationManager
    {
        static public IConfigManagerExtended Create( ISharedDictionary dic )
        {
            return new ConfigManagerImpl( dic );
        }
    }
}
