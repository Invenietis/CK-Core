xcopy ..\..\CK.Core\bin\Release ..\Runtime\Release\ /y
xcopy ..\..\CK.Context\bin\Release ..\Runtime\Release\ /y
xcopy ..\..\CK.Interop\bin\Release ..\Runtime\Release\ /y
xcopy ..\..\CK.Keyboard\bin\Release ..\Runtime\Release\ /y
xcopy ..\..\CK.Keyboard.Model\bin\Release ..\Runtime\Release\ /y
xcopy ..\..\CK.Plugin.Config\bin\Release ..\Runtime\Release\ /y
xcopy ..\..\CK.Plugin.Config.Model\bin\Release ..\Runtime\Release\ /y
xcopy ..\..\CK.Plugin.Discoverer\bin\Release ..\Runtime\Release\ /y
xcopy ..\..\CK.Plugin.Discoverer.Runner\bin\Release ..\Runtime\Release\ /y
xcopy ..\..\CK.Plugin.Host\bin\Release ..\Runtime\Release\ /y
xcopy ..\..\CK.Plugin.Host.Tests\bin\Release ..\Runtime\Release\ /y
xcopy ..\..\CK.Plugin.Model\bin\Release ..\Runtime\Release\ /y
xcopy ..\..\CK.Plugin.Runner\bin\Release ..\Runtime\Release\ /y
xcopy ..\..\CK.Reflection\bin\Release ..\Runtime\Release\ /y
xcopy ..\..\CK.SharedDic\bin\Release ..\Runtime\Release\ /y
xcopy ..\..\CK.Storage\bin\Release ..\Runtime\Release\ /y

del ..\Runtime\Release\*.pdb
