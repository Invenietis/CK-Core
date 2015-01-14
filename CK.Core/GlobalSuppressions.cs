#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\GlobalSuppressions.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Code Analysis results, point to "Suppress Message", and click 
// "In Suppression File".
// You do not need to add suppressions to this file manually.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", 
    Scope = "member", 
    Target = "CK.Core.CKSortedArrayList`1.#System.Collections.Generic.ICollection`1<!0>.IsReadOnly",
    Justification = "A CKSortedArrayList is always NOT read only. Avoid interface pollution."
    )]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", 
    Scope = "member", 
    Target = "CK.Core.FIFOBuffer`1.#CK.Core.ICKWritableCollector`1<!0>.Add(!0)",
    Justification = "Add is Push for the public interface."
    )]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", 
    Scope = "member", 
    Target = "CK.Core.CKSortedArrayList`1.#System.Collections.Generic.IList`1<!0>.Insert(System.Int32,!0)",
    Justification = "Insert MUST NOT be called. DoInsert is available for specialized types."
    )]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", 
    Scope = "member", 
    Target = "CK.Core.CKSortedArrayList`1.#System.Collections.Generic.IList`1<!0>.Item[System.Int32]",
    Justification = "Protected DoSet is here to hide the IList indexed setter."
    )]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", 
    Scope = "member", 
    Target = "CK.Core.ActivityMonitor+Group.#System.IDisposable.Dispose()",
    Justification = "A group is exposed as a IDisposable. Dispose should not be called directly."
    )]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", 
    Scope = "member", 
    Target = "CK.Core.ActivityMonitor.#CK.Core.Impl.IActivityMonitorImpl.InitializeTopicAndAutoTags(System.String,CK.Core.CKTrait,System.String,System.Int32)",
    Justification = "This method must only be called by Clients, not specialized types."
    )]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", 
    Scope = "member", 
    Target = "CK.Core.ActivityMonitor.#CK.Core.Impl.IActivityMonitorImpl.SetClientMinimalFilterDirty()",
    Justification = "This method must only be called by Clients, not specialized types."
    )]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", 
    Scope = "member", 
    Target = "CK.Core.ActivityMonitor.#CK.Core.Impl.IActivityMonitorImpl.OnClientMinimalFilterChanged(CK.Core.LogFilter,CK.Core.LogFilter)",
    Justification = "This method must only be called by Clients, not specialized types."
    )]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", 
    Scope = "member", 
    Target = "CK.Core.ActivityMonitorTextHelperClient.#CK.Core.IActivityMonitorClient.OnUnfilteredLog(CK.Core.ActivityMonitorLogData)",
    Justification = "Calls are relayed to protected methods."
    )]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", 
    Scope = "member", 
    Target = "CK.Core.ActivityMonitorTextHelperClient.#CK.Core.IActivityMonitorClient.OnOpenGroup(CK.Core.IActivityLogGroup)",
    Justification = "Calls are relayed to protected methods." 
    )]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", 
    Scope = "member", 
    Target = "CK.Core.ActivityMonitorTextHelperClient.#CK.Core.IActivityMonitorClient.OnGroupClosing(CK.Core.IActivityLogGroup,System.Collections.Generic.List`1<CK.Core.ActivityLogGroupConclusion>&)",
    Justification = "Calls are relayed to protected methods." 
    )]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", 
    Scope = "member", 
    Target = "CK.Core.ActivityMonitorTextHelperClient.#CK.Core.IActivityMonitorClient.OnGroupClosed(CK.Core.IActivityLogGroup,System.Collections.Generic.IReadOnlyList`1<CK.Core.ActivityLogGroupConclusion>)",
    Justification = "Calls are relayed to protected methods." 
    )]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", 
    Scope = "member", 
    Target = "CK.Core.ActivityMonitorTextHelperClient.#CK.Core.IActivityMonitorClient.OnTopicChanged(System.String,System.String,System.Int32)",
    Justification = "Calls are relayed to protected methods."
)]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", 
    Scope = "member", 
    Target = "CK.Core.ActivityMonitorTextHelperClient.#CK.Core.IActivityMonitorClient.OnAutoTagsChanged(CK.Core.CKTrait)",
    Justification = "Calls are relayed to protected methods." 
    )]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Usage", "CA2243:AttributeStringLiteralsShouldParseCorrectly",
    Justification = "InformalVersion is not a 'normal' Version."
    )]
