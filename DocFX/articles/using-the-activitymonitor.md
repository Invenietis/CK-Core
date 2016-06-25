---
uid: articles-using-the-activitymonitor
---
Using the ActivityMonitor
=========================

The [ActivityMonitor](xref:CK.Core.ActivityMonitor "CK.Core.ActivityMonitor") serves as a *logger*, and is the object you send logs to.

It is meant to follow and log a chain of logical operations in one thread at a time (but not necessarily always on the same thread), 
which we call an *activity*. 
You can spawn as many [ActivityMonitor](xref:CK.Core.ActivityMonitor "CK.Core.ActivityMonitor") instances as you like.

It is actually meant to be used with extension methods on [IActivityMonitor](xref:CK.Core.IActivityMonitor "CK.Core.ActivityMonitor"), like the ones contained in [CK.ActivityMonitor.StandardSender](xref:articles-ck-activitymonitor-standardsender). These extension methods translate the call to [IActivityMonitor.UnfilteredLog(ActivityMonitorLogData)](xref:CK.Core.IActivityMonitor.UnfilteredLog(CK.Core.ActivityMonitorLogData)).


> [!div class="alert alert-info"]
> The code in this article uses both the [CK.ActivityMonitor](https://www.nuget.org/packages/CK.ActivityMonitor/) and  [CK.ActivityMonitor.StandardSender](https://www.nuget.org/packages/CK.ActivityMonitor.StandardSender/) NuGet packages.<br/>The [CK.ActivityMonitor.StandardSender](https://www.nuget.org/packages/CK.ActivityMonitor.StandardSender/) package contains everyday extension methods used to log information: [Trace()](xref:CK.Core.ActivityMonitorSenderExtension.Trace(CK.Core.IActivityMonitor,System.Int32,System.String)), [Info()](xref:CK.Core.ActivityMonitorSenderExtension.Info(CK.Core.IActivityMonitor,System.Int32,System.String)), [Warn()](xref:CK.Core.ActivityMonitorSenderExtension.Warn(CK.Core.IActivityMonitor,System.Int32,System.String)), [Error()](xref:CK.Core.ActivityMonitorSenderExtension.Error(CK.Core.IActivityMonitor,System.Int32,System.String)) and [Fatal()](xref:CK.Core.ActivityMonitorSenderExtension.Fatal(CK.Core.IActivityMonitor,System.Int32,System.String))

Logging with the ActivityMonitor
--------------------------------

Use the [Trace()](xref:CK.Core.ActivityMonitorSenderExtension.Trace(CK.Core.IActivityMonitor,System.Int32,System.String)), [Info()](xref:CK.Core.ActivityMonitorSenderExtension.Info(CK.Core.IActivityMonitor,System.Int32,System.String)), [Warn()](xref:CK.Core.ActivityMonitorSenderExtension.Warn(CK.Core.IActivityMonitor,System.Int32,System.String)), [Error()](xref:CK.Core.ActivityMonitorSenderExtension.Error(CK.Core.IActivityMonitor,System.Int32,System.String)) and [Fatal()](xref:CK.Core.ActivityMonitorSenderExtension.Fatal(CK.Core.IActivityMonitor,System.Int32,System.String)) extension methods from [CK.ActivityMonitor.StandardSender](xref:articles-ck-activitymonitor-standardsender) to prepare a special object (an [IActivityMonitorLineSender](xref:CK.Core.IActivityMonitorLineSender)), then call [Send()](xref:CK.Core.ActivityMonitorSendExtension.Send(CK.Core.IActivityMonitorGroupSender,System.String)) on it.

```csharp
IActivityMonitor m = new ActivityMonitor();
m.Trace().Send("My trace message");
m.Info().Send("My info message");
m.Warn().Send("My warn message");
m.Error().Send("My error message");
m.Fatal().Send("My fatal message");
```

> [!div class="alert alert-info"]
> **Why does sending take *two* calls, and not one? Why not eg. `m.Info("My message")`?**
> The first call uses optional parameters containing caller information. These optional parameters are resolved at compile time, and replaced with information about the file and line that called the method. More information: [(MSDN) Caller Information (C#)](https://msdn.microsoft.com/en-us/library/mt653988.aspx)

### Composite formatting and syntax

The call to the [Send()](xref:CK.Core.ActivityMonitorSendExtension.Send(CK.Core.IActivityMonitorGroupSender,System.String)) extension methods use composite format strings (the same syntax used by `string.Format()`).

```csharp
// Output: My info message 1 2 3
m.Info().Send("My info message {0} {1} {2}", 1, 2, 3);
```

More information: [(MSDN) Composite Formatting](https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx)

### Exception logging

You can log an exception by passing it to a [Send()](xref:CK.Core.ActivityMonitorSendExtension.Send(CK.Core.IActivityMonitorGroupSender,System.String)) method, before the message.

```csharp
Exception e;
m.Error().Send(e, "My error");
```

Alternatively, you can log only the exception itself. The log entry's message will be the exception's message.

```csharp
Exception e;
m.Error().Send(e);
```

### Tags ([CKTrait](xref:CK.Core.CKTrait))

You can add a tag ([CKTrait](xref:CK.Core.CKTrait)) to the log entry by passing it to a [Send()](xref:CK.Core.ActivityMonitorSendExtension.Send(CK.Core.IActivityMonitorGroupSender,System.String)) method, before the message.

```
CKTrait tag = ActivityMonitor.Tags.Register( "MyTag" );
m.Info().Send(tag, "Message");
```

> [!div class="alert alert-danger"]
> This documentation is not written yet. Help us by improving it!

ActivityMonitor output
----------------------

### Output per ActivityMonitor: [IActivityMonitorClient](xref:CK.Core.IActivityMonitorClient)

> [!div class="alert alert-danger"]
> This documentation is not written yet. Help us by improving it!

### Centralized output for all ActivityMonitor instances: [GrandOutput](xref:CK.Monitoring.GrandOutput)

> [!div class="alert alert-warning"]
> The [GrandOutput](xref:CK.Monitoring.GrandOutput) is part of [CK.Monitoring](xref:articles-ck-monitoring), and is not part of CK.ActivityMonitor.

> [!div class="alert alert-danger"]
> This documentation is not written yet. Help us by improving it!


Thread safety and correlations
-------------

The [ActivityMonitor](xref:CK.Core.ActivityMonitor "CK.Core.ActivityMonitor") class itself is **not thread-safe**.

If your activity spawns multiple threads, you can get a token from the parent monitor, and use it to create *dependent monitors*: 

```csharp
ActivityMonitor parentMonitor = new ActivityMonitor();

// In your main ActivityMonitor thread:
ActivityMonitor.DependentToken token = parentMonitor.DependentActivity().CreateToken();

// In other threads:
using( var childMonitor = token.CreateDependentMonitor() )
{
    childMonitor.Trace().Send( "Whatever" );
}
```

This child monitor will be linked to its parent monitor through log entries that will appear in both of them.