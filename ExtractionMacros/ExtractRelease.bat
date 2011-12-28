xcopy ..\CK.Context\bin\Release CertifiedRuntime\Release\ /y
xcopy ..\CK.Core\bin\Release CertifiedRuntime\Release\ /y
xcopy ..\CK.Interop\bin\Release CertifiedRuntime\Release\ /y
xcopy ..\CK.Plugin.Config\bin\Release CertifiedRuntime\Release\ /y
xcopy ..\CK.Plugin.Config.Model\bin\Release CertifiedRuntime\Release\ /y
xcopy ..\CK.Plugin.Discoverer\bin\Release CertifiedRuntime\Release\ /y
xcopy ..\CK.Plugin.Discoverer.Runner\bin\Release CertifiedRuntime\Release\ /y
xcopy ..\CK.Plugin.Host\bin\Release CertifiedRuntime\Release\ /y
xcopy ..\CK.Plugin.Host.Tests\bin\Release CertifiedRuntime\Release\ /y
xcopy ..\CK.Plugin.Model\bin\Release CertifiedRuntime\Release\ /y
xcopy ..\CK.Plugin.Runner\bin\Release CertifiedRuntime\Release\ /y
xcopy ..\CK.Reflection\bin\Release CertifiedRuntime\Release\ /y
xcopy ..\CK.SharedDic\bin\Release CertifiedRuntime\Release\ /y
xcopy ..\CK.Storage\bin\Release CertifiedRuntime\Release\ /y

del Runtime\Release\*.pdb
