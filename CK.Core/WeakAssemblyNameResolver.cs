using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace CK.Core
{

    /// <summary>
    /// Assemby loader helper: hooks the <see cref="AppDomain.AssemblyResolve"/> event
    /// in order to try to load a version-less assembly.
    /// The <see cref="GetResolvedList"/> is maintained and can be used to diagnose any assembly binding issues.
    /// All members are thread safe.
    /// </summary>
    public static class WeakAssemblyNameResolver
    {
        static int _installCount;
        static List<KeyValuePair<AssemblyName, AssemblyName>> _list = new List<KeyValuePair<AssemblyName, AssemblyName>>();

        /// <summary>
        /// Gets whether this helper is active.
        /// </summary>
        public static bool IsInstalled => _installCount >= 0;

        /// <summary>
        /// Installs the hook if not already installed.
        /// Instead of using Install/<see cref="Uninstall"/>, the <see cref="TemporaryInstall"/> helper should be used.
        /// </summary>
        public static void Install()
        {
            if( Interlocked.Increment( ref _installCount ) == 1 )
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            }
        }

        /// <summary>
        /// Uninstall the hook if possible.
        /// </summary>
        public static void Uninstall()
        {
            if( Interlocked.Decrement( ref _installCount ) == 0 )
            {
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            }
        }

        /// <summary>
        /// Gets the pair of requested name and eventually resolved name that have been resolved so far.
        /// </summary>
        /// <returns>A copy of the internally maintained list of resolved assembly names.</returns>
        public static KeyValuePair<AssemblyName, AssemblyName>[] GetResolvedList()
        {
            lock( _list )
            {
                return _list.ToArray();
            }
        }

        class Auto : IDisposable
        {
            bool _done;

            public void Dispose()
            {
                if( !_done )
                {
                    _done = true;
                    Uninstall();
                }
            }
        }

        /// <summary>
        /// Temporary installs the hook that will be uninstalled when the returned object will be disposed.
        /// </summary>
        /// <returns>The dispoable to dispose when done.</returns>
        public static IDisposable TemporaryInstall()
        {
            Install();
            return new Auto();
        }

        static Assembly CurrentDomain_AssemblyResolve( object sender, ResolveEventArgs args )
        {
            var failed = new AssemblyName( args.Name );
            var resolved = failed.Version != null && string.IsNullOrWhiteSpace( failed.CultureName )
                    ? Assembly.Load( new AssemblyName( failed.Name ) )
                    : null;
            lock( _list )
            {
                _list.Add( new KeyValuePair<AssemblyName, AssemblyName>( failed, resolved?.GetName() ) );
            }
            return resolved;
        }
    }
}
