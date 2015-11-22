@powershell -NoProfile -ExecutionPolicy unrestricted -Command "&{$Branch='dev';iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1'))}"
dnvm use -p -r clr 1.0.0-rc1-final
dnu feeds list
pause
dnu restore
pause