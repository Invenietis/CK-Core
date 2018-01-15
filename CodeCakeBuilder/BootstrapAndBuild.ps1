$boostrap = Join-Path $PSScriptRoot "Bootstrap.ps1"
&$boostrap
$codeCakeBuilder = Join-Path $PSScriptRoot "bin/Release/CodeCakeBuilder.exe"
&$codeCakeBuilder

 
