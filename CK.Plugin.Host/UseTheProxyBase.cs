namespace CK.Plugin.Hosting
{
    /// <summary>
    /// Fake internal class that forces the compiler to keep <see cref="ServiceProxyBase"/> implementation.
    /// Without it, get_Status (for instance) is not ignored at compile time and hence, not defined when 
    /// the ServiceProxyBase is used as the base class by dynamic proxies.
    /// </summary>
    class UseTheProxyBase : ServiceProxyBase, IService<CK.Plugin.IDynamicService>
    {
        UseTheProxyBase()
            : base( null, typeof( CK.Plugin.IDynamicService ), null, null )
        {
        }

        protected override object RawImpl
        {
            get { return null; }
            set { }
        }

        public CK.Plugin.IDynamicService Service
        {
            get { return null; }
        }

    }
}
