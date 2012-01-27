xcopy ..\..\CK.Context\bin\Release ..\Release\ /y
xcopy ..\..\CK.Core\bin\Release ..\Release\ /y
xcopy ..\..\CK.Interop\bin\Release ..\Release\ /y
xcopy ..\..\CK.Plugin.Config\bin\Release ..\Release\ /y
xcopy ..\..\CK.Plugin.Config.Model\bin\Release ..\Release\ /y
xcopy ..\..\CK.Plugin.Discoverer\bin\Release ..\Release\ /y
xcopy ..\..\CK.Plugin.Discoverer.Runner\bin\Release ..\Release\ /y
xcopy ..\..\CK.Plugin.Host\bin\Release ..\Release\ /y
xcopy ..\..\CK.Plugin.Host.Tests\bin\Release ..\Release\ /y
xcopy ..\..\CK.Plugin.Model\bin\Release ..\Release\ /y
xcopy ..\..\CK.Plugin.Runner\bin\Release ..\Release\ /y
xcopy ..\..\CK.Reflection\bin\Release ..\Release\ /y
xcopy ..\..\CK.SharedDic\bin\Release ..\Release\ /y
xcopy ..\..\CK.Storage\bin\Release ..\Release\ /y

del ..\Release\*.pdb
