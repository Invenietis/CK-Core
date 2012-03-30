using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("CK.Plugin.Config")]
[assembly: AssemblyDescription("")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany("Microsoft")]
[assembly: AssemblyProduct("CK.Plugin.Config")]
[assembly: AssemblyCopyright("Copyright © Microsoft 2010")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("a86e4ea4-0a73-4b95-8f02-efd9f12face5")]

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
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// Allow CK.Tests assembly to acces to Internals of CK.Plugin.Config.
// Here to ease the set up of NUnit tests.
[assembly: InternalsVisibleTo( "CK.Plugin.Config.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001002badda7c6774254194bd7d7b264aa7be4622e8a0105acfe1b2edc239b3389a317e008862dd5c62b61298042874b8bf08c4ad18a71dcbae5234066d3f6ef159bc9f8014c89d5be68f4d5b59af4169f15784af3eb2fa02e312e480ea123f383c09bab56a016b46519cc830fa17bd6ccff7260cc8d20ece42745cef70b98e3c70d9" )]


[assembly: AssemblyDelaySign( false )]
[assembly: AssemblyKeyFile( @"..\..\..\..\Env\GoodKey.snk" )]
[assembly: AssemblyKeyName( "" )]