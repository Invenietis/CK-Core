using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CK.RouteConfig;
using CK.Core;

namespace CK.Monitoring.GrandOutputHandlers
{
    /// <summary>
    /// Configuration object for <see cref="BinaryFile"/>.
    /// </summary>
    [HandlerType( typeof(BinaryFile) )]
    public class BinaryFileConfiguration : HandlerConfiguration
    {
        /// <summary>
        /// Initializes a new <see cref="BinaryFileConfiguration"/>.
        /// </summary>
        /// <param name="name">Name of this configuration.</param>
        public BinaryFileConfiguration( string name )
            : base( name )
        {
            MaxCountPerFile = 10000;
        }

        /// <summary>
        /// Gets or sets the path of the file. When not rooted (see <see cref="System.IO.Path.IsPathRooted"/>),
        /// it is a sub path in <see cref="SystemActivityMonitor.RootLogPath"/>.
        /// It defaults to null: it must be specified.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the maximal count of entries per file.
        /// Defaults to 10000.
        /// </summary>
        public int MaxCountPerFile { get; set; }

        protected override void Initialize( IActivityMonitor monitor, XElement xml )
        {
            Path = xml.AttributeRequired( "Path" ).Value;
            MaxCountPerFile = xml.GetAttributeInt( "MaxCountPerFile", MaxCountPerFile );
        }
    }
}
