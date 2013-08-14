using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("CK.Monitoring")]
[assembly: AssemblyDescription("ActivityMonitor related implementations.")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("dde3882c-4796-40c3-b767-bb39f0082187")]

// Allow CK.Monitoring.Tests assembly to acces to internals of CK.Monitoring.
[assembly: InternalsVisibleTo( "CK.Monitoring.Tests" )]
