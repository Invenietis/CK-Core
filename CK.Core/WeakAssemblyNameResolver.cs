using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace CK.Core
{
    /// <summary>
    /// Assemby loader helper: hooks the <see cref="AppDomain.AssemblyResolve"/> event
    /// in order to try to load the assembly by name only (no version and public key token).
    /// The <see cref="GetAssemblyConflicts"/> is maintained and can be used to diagnose any assembly binding issues.
    /// All members are thread safe.
    /// </summary>
    public static class WeakAssemblyNameResolver
    {
        static int _installCount;
        static List<AssemblyLoadConflict> _list = new List<AssemblyLoadConflict>();

        /// <summary>
        /// Gets whether this helper is active.
        /// </summary>
        public static bool IsInstalled => _installCount >= 0;

        /// <summary>
        /// Installs the hook if not already installed. Always returns the current count of conflicts.
        /// Instead of using Install and <see cref="Uninstall"/>, the <see cref="TemporaryInstall"/> helper should be used.
        /// </summary>
        /// <returns>
        /// The current count of conflicts.
        /// </returns>
        public static int Install()
        {
            lock( _list )
            {
                if( ++_installCount == 1 )
                {
                    AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                }
                return _list.Count;
            }
        }

        /// <summary>
        /// Decrements the internal install count and removes the hook if it reaches 0.
        /// Always returns the conflicts that have occurred, skipping the
        /// first <paramref name="previousConflictsCount"/> ones.
        /// </summary>
        /// <param name="previousConflictsCount">Previous conflicts to skip: typically returned by <see cref="Install"/>.</param>
        /// <returns>A non null array, possibly empty, of assemby conflicts.</returns>
        public static AssemblyLoadConflict[] Uninstall( int previousConflictsCount = 0 )
        {
            lock( _list )
            {
                if( --_installCount == 0 )
                {
                    AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
                }
                return CopyConflicts( previousConflictsCount );
            }
        }

        /// <summary>
        /// Gets the existing conflicts, optionnally skipping the first ones.
        /// </summary>
        /// <param name="skippedConflictsCount">Number of conflicts to skip.</param>
        /// <returns>A copy of the internally maintained list of conflicts.</returns>
        public static AssemblyLoadConflict[] GetAssemblyConflicts( int skippedConflictsCount = 0 )
        {
            lock( _list )
            {
                return CopyConflicts( skippedConflictsCount );
            }
        }

        static AssemblyLoadConflict[] CopyConflicts( int skippedConflictsCount )
        {
            Debug.Assert( Monitor.IsEntered( _list ) );
            int len = _list.Count - skippedConflictsCount;
            if( len <= 0 ) return Array.Empty<AssemblyLoadConflict>();
            var result = new AssemblyLoadConflict[len];
            _list.CopyTo( skippedConflictsCount, result, 0, len );
            return result;
        }

        /// <summary>
        /// Disposable context returned by <see cref="TemporaryInstall"/>
        /// that exposes the 
        /// </summary>
        public class TemporaryInstaller : IDisposable
        {
            readonly int _conlictsCount;
            AssemblyLoadConflict[] _finalConflicts;

            internal TemporaryInstaller()
            {
                _conlictsCount = Install();
            }

            /// <summary>
            /// Gets the current or final conflicts.
            /// This may be called at any time: when this object is not yet disposed it calls
            /// the <see cref="GetAssemblyConflicts"/> method and once disposed it returns
            /// the conflicts list at the time of its disposal.
            /// </summary>
            public IReadOnlyList<AssemblyLoadConflict> Conflicts => _finalConflicts ?? GetAssemblyConflicts( _conlictsCount );

            /// <summary>
            /// Disposing this objects calls <see cref="Uninstall"/> and captures
            /// the <see cref="Conflicts"/> resolved during its lifetime.
            /// </summary>
            public void Dispose()
            {
                if( _finalConflicts == null )
                {
                    _finalConflicts = Uninstall( _conlictsCount );
                }
            }
        }

        /// <summary>
        /// Temporary installs the hook that will be uninstalled when the returned object will be disposed.
        /// </summary>
        /// <returns>The <see cref="TemporaryInstaller"/> to dispose when done.</returns>
        public static TemporaryInstaller TemporaryInstall() => new TemporaryInstaller();

        static Assembly CurrentDomain_AssemblyResolve( object sender, ResolveEventArgs args )
        {
            var wanted = new AssemblyName( args.Name );
            // Skips unversionned request and satellite assemblies.
            if( wanted.Version == null || string.IsNullOrWhiteSpace( wanted.CultureName ) ) return null;
            var resolved = Assembly.Load( new AssemblyName( wanted.Name ) );
            lock( _list )
            {
                _list.Add( new AssemblyLoadConflict( DateTime.UtcNow, args.RequestingAssembly?.GetName(), wanted, resolved?.GetName() ) );
            }
            return resolved;
        }
    }
}
