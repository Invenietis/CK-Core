using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.SharedDic;
using CK.Storage;
using System.Xml;
using CK.Plugin.Config;
using CK.Core;

namespace CK.Context
{

    public sealed partial class Context
    {
        /// <summary>
        /// Writes a context.
        /// </summary>
        public void SaveContext( IStructuredWriter writer )
        {
            if( writer == null ) throw new ArgumentNullException( "writer" );

            using( ISharedDictionaryWriter dw = _dic.RegisterWriter( writer ) )
            {
                XmlWriter w = writer.Xml;
                w.WriteStartElement( "CKContext" );
                writer.WriteInlineObjectStructuredElement( "RequirementLayer", _reqLayer );
                dw.WritePluginsDataElement( "PluginData", _proxifiedContext );
                w.WriteEndElement();
            }
        }

        public bool LoadContext( IStructuredReader reader )
        {
            if( reader == null ) throw new ArgumentNullException( "reader" );
            if( !reader.Xml.IsStartElement( "CKContext" ) ) return false;
            using( ISharedDictionaryReader dr = _dic.RegisterReader( reader, MergeMode.None ) )
            {
                XmlReader r = reader.Xml;
                r.Read();
                reader.ReadInlineObjectStructuredElement( "RequirementLayer", _reqLayer );
                dr.ReadPluginsDataElement( "PluginData", _proxifiedContext );
                r.ReadEndElement();
            }
            return true;
        }

    }
}
