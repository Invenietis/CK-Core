xcopy ..\CK.Context\bin\Release ..\Output\Release\ /y
xcopy ..\CK.Core\bin\Release ..\Output\Release\ /y
xcopy ..\CK.Interop\bin\Release ..\Output\Release\ /y
xcopy ..\CK.Plugin.Config\bin\Release ..\Output\Release\ /y
xcopy ..\CK.Plugin.Config.Model\bin\Release ..\Output\Release\ /y
xcopy ..\CK.Plugin.Discoverer\bin\Release ..\Output\Release\ /y
xcopy ..\CK.Plugin.Discoverer.Runner\bin\Release ..\Output\Release\ /y
xcopy ..\CK.Plugin.Host\bin\Release ..\Output\Release\ /y
xcopy ..\CK.Plugin.Host.Tests\bin\Release ..\Output\Release\ /y
xcopy ..\CK.Plugin.Model\bin\Release ..\Output\Release\ /y
xcopy ..\CK.Plugin.Runner\bin\Release ..\Output\Release\ /y
xcopy ..\CK.Reflection\bin\Release ..\Output\Release\ /y
xcopy ..\CK.SharedDic\bin\Release ..\Output\Release\ /y
xcopy ..\CK.Storage\bin\Release ..\Output\Release\ /y

del Runtime\Release\*.pdb
