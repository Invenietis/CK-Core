---
uid: home-page
---
CK-Core components
==================

> [!div class="alert alert-warning"]
> This documentation is in pre-release and is subject to change.

This repository is home to common tools, used in most of our software,
split in multiple components and packages on [NuGet](https://www.nuget.org):

- [CK.Core](xref:articles-ck-core "CK.Core") ([NuGet package](https://www.nuget.org/packages/CK.Core/)), a common core of utilities used in the other components (such as `CKTrait`, `CKException` or `TemporaryFile`)
- [CK.ActivityMonitor](xref:articles-ck-activitymonitor "CK.ActivityMonitor") ([NuGet package](https://www.nuget.org/packages/CK.ActivityMonitor/)), containing the `ActivityMonitor` logging component logic (formerly part of `CK.Core`)
- [CK.ActivityMonitor.StandardSender](xref:articles-ck-activitymonitor-standardsender "CK.ActivityMonitor.StandardSender") ([NuGet package](https://www.nuget.org/packages/CK.ActivityMonitor.StandardSender/)), standard extensions like `IActivityMonitor.Trace().Send()` used for easier logging at development time (formerly part of `CK.Core`)
- [CK.Monitoring](xref:articles-ck-monitoring "CK.Monitoring") ([NuGet package](https://www.nuget.org/packages/CK.Monitoring/)), with `ActivityMonitor`-related utilities for writing and reading text and/or binary log sinks with the `GrandOutput`
- [CK.Reflection](xref:articles-ck-reflection "CK.Reflection") ([NuGet package](https://www.nuget.org/packages/CK.Reflection/)), containing various reflection-related utilities
