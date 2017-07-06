$args = @(".\Schemavolution.sln", "/t:Clean", "/t:Build", "/p:Configuration=Release")
$msbuild = ${env:ProgramFiles(x86)} + "\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"
& $msbuild $args

cd .\Schemavolution.Specification\
dotnet pack -c Release --include-symbols
cd ..\

cd .\Schemavolution.DDD\
dotnet pack -c Release --include-symbols
cd ..\

Remove-Item .\Schemavolution.EF6\tools\*.dll
Move-Item .\Schemavolution.EF6.Commands\bin\Release\Schemavolution.EF6.Commands.dll .\Schemavolution.EF6\tools\
Move-Item .\Schemavolution.EF6.Commands\bin\Release\Schemavolution.EF6.dll .\Schemavolution.EF6\tools\
Move-Item .\Schemavolution.EF6.Commands\bin\Release\Schemavolution.Specification.dll .\Schemavolution.EF6\tools\

cd .\Schemavolution.EF6\
nuget pack -Build -Prop Configuration=Release -Symbols
cd ..\
