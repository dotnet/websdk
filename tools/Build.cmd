@echo off

call %~dp0\EnsurePublishEnv.cmd

copy \\aspnetci\share\tools\Microsoft.Web.MsBuildTasks2.dll %PublishTools% /y
copy \\aspnetci\share\tools\7za.exe %PublishTools% /y

dotnet restore3 %PublishRoot%\src\Microsoft.DotNetCore.Publish.Tasks\Microsoft.DotNetCore.Publish.Tasks.csproj /p:SkipInvalidConfigurations=true
msbuild %PublishRoot%\dirs.proj /p:configuration=Release;SkipInvalidConfigurations=true /t:Build;Sign