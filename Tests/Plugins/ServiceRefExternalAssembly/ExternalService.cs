using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using System.Windows;

namespace ServiceRefExternalAssembly
{
    public interface ExternalService : IDynamicService
    {
        UIElement Test();
    }
}
