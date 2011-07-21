using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Plugin.Config
{
    public class SharedDictionaryWriterEventArgs : EventArgs
    {
        public readonly ISharedDictionaryWriter Writer;

        public readonly IObjectPluginAssociation ObjectPlugin;

        public SharedDictionaryWriterEventArgs( ISharedDictionaryWriter w, IObjectPluginAssociation op )
        {
            Writer = w;
            ObjectPlugin = op;
        }

    }

}
