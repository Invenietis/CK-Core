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
    public static class XmlMonitoringExtensions
    {

        static public LogFilter GetAttributeLogFilter( this XElement @this,  string name )
        {
            XAttribute a = @this.Attribute( name );
            return a != null ? LogFilter.Parse( a.Value ) : LogFilter.Undefined;
        }

    }
}
