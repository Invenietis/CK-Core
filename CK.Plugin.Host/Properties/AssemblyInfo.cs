using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle( "CK.Plugin.Host" )]
[assembly: AssemblyDescription( "" )]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany( "Invenietis" )]
[assembly: AssemblyProduct( "CK.Plugin.Hosting" )]
[assembly: AssemblyCopyright( "Copyright © Invenietis 2011" )]
[assembly: AssemblyTrademark( "" )]
[assembly: AssemblyCulture( "" )]
[assembly: InternalsVisibleTo( "CKProxyAssembly" )]
[assembly: InternalsVisibleTo( "CK.Plugin.Host.Tests" )]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible( false )]
[assembly: CLSCompliant(true) ]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid( "f6208078-5cca-44ed-9ae4-1414643efce0" )]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion( "1.0.0.0" )]
[assembly: AssemblyFileVersion( "1.0.0.0" )]
