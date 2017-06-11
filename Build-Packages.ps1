cd .\Schemavolution.Specification\
dotnet build
dotnet pack --include-symbols
cd ..\

cd .\Schemavolution.DDD\
dotnet build
dotnet pack --include-symbols
cd ..\

cd .\Schemavolution.EF6\
nuget pack -Build -Prop Configuration=Release
nuget pack -Symbols
cd ..\
