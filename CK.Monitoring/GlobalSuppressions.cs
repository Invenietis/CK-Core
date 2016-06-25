// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Code Analysis results, point to "Suppress Message", and click 
// "In Suppression File".
// You do not need to add suppressions to this file manually.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", 
    Scope = "member", 
    Target = "CK.Monitoring.GrandOutputHandlers.BinaryFile.#System.IDisposable.Dispose()",
    Justification="I want hide Dispose from the public interface: Close should be used (with its parameters)." )]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", 
    MessageId = "_writer", 
    Scope = "member", 
    Target = "CK.Monitoring.GrandOutputHandlers.BinaryFile.#System.IDisposable.Dispose()",
    Justification="Close does the job correctly." )]


