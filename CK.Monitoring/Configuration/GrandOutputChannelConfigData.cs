using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CK.Core;

namespace CK.Monitoring
{
    public class GrandOutputChannelConfigData
    {
        public LogFilter MinimalFilter;

        public GrandOutputChannelConfigData( XElement xml )
        {
            MinimalFilter = xml.GetAttributeLogFilter( "MinimalFilter", true ).Value;
        }
    }
}
