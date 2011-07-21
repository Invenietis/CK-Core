using CK.Plugin;
using CK.Storage;

namespace CK.Plugin.Config
{
    /// <summary>
    /// Implements a <see cref="IStructuredSerializer{T}"/> for <see cref="RequirementLayer"/>.
    /// </summary>
    public class RequirementLayerSerializer : IStructuredSerializer<RequirementLayer>
    {
        /// <summary>
        /// Gets the singleton instance for this <see cref="RequirementLayerSerializer"/>.
        /// </summary>
        public static readonly IStructuredSerializer<RequirementLayer> Instance = new RequirementLayerSerializer();

        private RequirementLayerSerializer()
        {
        }

        object IStructuredSerializer<RequirementLayer>.ReadInlineContent( IStructuredReader sr, RequirementLayer reqLayer )
        {
            RequirementLayer.ReadInlineFromXml( sr.Xml, ref reqLayer );
            return reqLayer;
        }

        void IStructuredSerializer<RequirementLayer>.WriteInlineContent( IStructuredWriter sw, RequirementLayer o )
        {
            RequirementLayer.WriteInlineToXml( sw.Xml, o );
        }

    }
}
