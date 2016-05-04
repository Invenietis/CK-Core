using System.Xml.Linq;
using CK.Core;
using System.IO;

namespace CK.Monitoring.GrandOutputHandlers
{
    /// <summary>
    /// Configuration object for <see cref="BinaryFile"/>.
    /// </summary>
    [HandlerType( typeof(TextFile) )]
    public class TextFileConfiguration : FileConfigurationBase
    {
        /// <summary>
        /// Initializes a new <see cref="TextFileConfiguration"/>.
        /// </summary>
        /// <param name="name">Name of this configuration.</param>
        public TextFileConfiguration( string name )
            : base( name )
        {
        }

        /// <summary>
        /// Gets or sets whether the monitor identifier should prefix all lines.
        /// Defaults to false.
        /// </summary>
        public bool MonitorColumn { get; set; }

        /// <summary>
        /// Initializes (or reinitializes) this <see cref="TextFileConfiguration"/> from a <see cref="XElement"/>.
        /// </summary>
        /// <param name="monitor">Monitor to report errors or warnings.</param>
        /// <param name="xml">Source XML element.</param>
        protected override void Initialize( IActivityMonitor monitor, XElement xml )
        {
            Path = xml.AttributeRequired( "Path" ).Value;
            MonitorColumn = (bool?)xml.Attribute( "MonitorColumn" ) ?? false;
        }

    }
}
