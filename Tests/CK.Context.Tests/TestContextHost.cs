using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CK.Context.Tests
{
    class TestContextHost : AbstractContextHost
    {
        string _name;

        public TestContextHost( string name )
        {
            _name = name;
        }

        public Uri SystemConfigAddress
        {
            get { return new Uri( Path.Combine( TestBase.AppFolder, "Config-Sys-" + _name ) ); }
        }

        public Uri DefaultUserConfigAddress
        {
            get { return new Uri( Path.Combine( TestBase.AppFolder, "Config-Usr-" + _name ) ); }
        }

        public KeyValuePair<string, Uri> DefaultContextProfile
        {
            get { return new KeyValuePair<string, Uri>( "Default-" + _name, new Uri( Path.Combine( TestBase.AppFolder, "Ctx-" + _name ) ) ); }
        }


        protected override Uri GetSystemConfigAddress()
        {
            return SystemConfigAddress;
        }

        protected override Uri GetDefaultUserConfigAddress( bool saving )
        {
            return DefaultUserConfigAddress;
        }

        protected override KeyValuePair<string, Uri> GetDefaultContextProfile( bool saving )
        {
            return DefaultContextProfile;
        }
    }
}
