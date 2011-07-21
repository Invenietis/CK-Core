call ExtractRelease.bat

md ..\Packages\Release

nuget pack Release/CK.Context.Release.nuspec -o ..\Packages\Release
nuget pack Release/CK.Core.Release.nuspec -o ..\Packages\Release
nuget pack Release/CK.Interop.Release.nuspec -o ..\Packages\Release
nuget pack Release/CK.Keyboard.Release.nuspec -o ..\Packages\Release
nuget pack Release/CK.Keyboard.Model.Release.nuspec -o ..\Packages\Release
nuget pack Release/CK.Plugin.Config.Release.nuspec -o ..\Packages\Release
nuget pack Release/CK.Plugin.Config.Model.Release.nuspec -o ..\Packages\Release
nuget pack Release/CK.Plugin.Discoverer.Release.nuspec -o ..\Packages\Release
nuget pack Release/CK.Plugin.Discoverer.Model.Release.nuspec -o ..\Packages\Release
nuget pack Release/CK.Plugin.Discoverer.Runner.Release.nuspec -o ..\Packages\Release
nuget pack Release/CK.Plugin.Host.Release.nuspec -o ..\Packages\Release
nuget pack Release/CK.Plugin.Host.Model.Release.nuspec -o ..\Packages\Release
nuget pack Release/CK.Plugin.Model.Release.nuspec -o ..\Packages\Release
nuget pack Release/CK.Plugin.Runner.Release.nuspec -o ..\Packages\Release
nuget pack Release/CK.Reflection.Release.nuspec -o ..\Packages\Release
nuget pack Release/CK.SharedDic.Release.nuspec -o ..\Packages\Release
nuget pack Release/CK.Storage.Release.nuspec -o ..\Packages\Release
