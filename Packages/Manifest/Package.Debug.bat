call ExtractDebug.bat

md ..\Packages\Debug

nuget pack Debug/CK.Core.Debug.nuspec -o ..\Packages\Debug
nuget pack Debug/CK.Context.Debug.nuspec -o ..\Packages\Debug
nuget pack Debug/CK.Interop.Debug.nuspec -o ..\Packages\Debug
nuget pack Debug/CK.Keyboard.Debug.nuspec -o ..\Packages\Debug
nuget pack Debug/CK.Keyboard.Model.Debug.nuspec -o ..\Packages\Debug
nuget pack Debug/CK.Plugin.Config.Debug.nuspec -o ..\Packages\Debug
nuget pack Debug/CK.Plugin.Config.Model.Debug.nuspec -o ..\Packages\Debug
nuget pack Debug/CK.Plugin.Discoverer.Debug.nuspec -o ..\Packages\Debug
nuget pack Debug/CK.Plugin.Discoverer.Model.Debug.nuspec -o ..\Packages\Debug
nuget pack Debug/CK.Plugin.Discoverer.Runner.Debug.nuspec -o ..\Packages\Debug
nuget pack Debug/CK.Plugin.Host.Debug.nuspec -o ..\Packages\Debug
nuget pack Debug/CK.Plugin.Host.Model.Debug.nuspec -o ..\Packages\Debug
nuget pack Debug/CK.Plugin.Model.Debug.nuspec -o ..\Packages\Debug
nuget pack Debug/CK.Plugin.Runner.Debug.nuspec -o ..\Packages\Debug
nuget pack Debug/CK.Reflection.Debug.nuspec -o ..\Packages\Debug
nuget pack Debug/CK.SharedDic.Debug.nuspec -o ..\Packages\Debug
nuget pack Debug/CK.Storage.Debug.nuspec -o ..\Packages\Debug