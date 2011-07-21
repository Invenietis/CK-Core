using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Plugin.Config
{
    public class SharedDictionaryReaderEventArgs : EventArgs
    {
        public readonly ISharedDictionaryReader Reader;

        public readonly IObjectPluginAssociation ObjectPlugin;

        public SharedDictionaryReaderEventArgs( ISharedDictionaryReader r, IObjectPluginAssociation op )
        {
            Reader = r;
            ObjectPlugin = op;
        }

    }

}
