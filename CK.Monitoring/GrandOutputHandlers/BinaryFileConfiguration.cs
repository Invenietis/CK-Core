using System.Xml.Linq;
using CK.Core;
using System.IO;

namespace CK.Monitoring.GrandOutputHandlers
{
    /// <summary>
    /// Configuration object for <see cref="BinaryFile"/>.
    /// </summary>
    [HandlerType( typeof(BinaryFile) )]
    public class BinaryFileConfiguration : FileConfigurationBase
    {
        /// <summary>
        /// Initializes a new <see cref="BinaryFileConfiguration"/>.
        /// </summary>
        /// <param name="name">Name of this configuration.</param>
        public BinaryFileConfiguration( string name )
            : base( name )
        {
        }
    }
}
