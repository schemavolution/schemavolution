$args = @(".\Schemavolution.sln", "/t:Clean", "/t:Build", "/p:Configuration=Release")
$msbuild = ${env:ProgramFiles(x86)} + "\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"
& $msbuild $args

cd .\Schemavolution.Specification\
dotnet pack -c Release --include-symbols
cd ..\

cd .\Schemavolution.DDD\
dotnet pack -c Release --include-symbols
cd ..\

cd .\Schemavolution.EF6\
nuget pack -Build -Prop Configuration=Release -Symbols
cd ..\
