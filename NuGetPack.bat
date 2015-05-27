del *.nupkg
.\.nuget\NuGet.exe pack .\MicroUpsert\MicroUpsert.csproj -Build -Prop Configuration=Release
.\.nuget\NuGet.exe push *.nupkg
pause