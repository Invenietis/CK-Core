using System.Xml.Linq;
using CK.Core;
using System.IO;

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
            MaxCountPerFile = 20000;
            FileBufferSize = 4096;
            UseGzipCompression = false;
        }

        /// <summary>
        /// Gets or sets the path of the file. When not rooted (see <see cref="System.IO.Path.IsPathRooted"/>),
        /// it is a sub path in <see cref="SystemActivityMonitor.RootLogPath"/>.
        /// It defaults to null: it must be specified.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the maximal count of entries per file.
        /// Defaults to 20000.
        /// </summary>
        public int MaxCountPerFile { get; set; }

        /// <summary>
        /// Gets or sets whether files will be opened with <see cref="FileOptions.WriteThrough"/>.
        /// Defaults to false.
        /// </summary>
        public bool FileWriteThrough { get; set; }

        /// <summary>
        /// Gets or sets the buffer size used to write files.
        /// Defaults to 4096.
        /// </summary>
        public int FileBufferSize { get; set; }

        /// <summary>
        /// Gets or sets whether to use Gzip compression after closing log files.
        /// Defaults to false.
        /// </summary>
        public bool UseGzipCompression { get; set; }

        /// <summary>
        /// Initializes (or reinitializes) this <see cref="BinaryFileConfiguration"/> from a <see cref="XElement"/>.
        /// </summary>
        /// <param name="monitor">Monitor to report errors or warnings.</param>
        /// <param name="xml">Source XML element.</param>
        protected override void Initialize( IActivityMonitor monitor, XElement xml )
        {
            Path = xml.AttributeRequired( "Path" ).Value;
            MaxCountPerFile = (int?)xml.Attribute( "MaxCountPerFile" ) ?? MaxCountPerFile;
            UseGzipCompression = (bool?)xml.Attribute( "UseGzipCompression" ) ?? UseGzipCompression;
        }
    }
}
