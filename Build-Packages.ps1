cd .\Schemavolution.Specification\
dotnet pack -c Release --include-symbols
cd ..\

cd .\Schemavolution.DDD\
dotnet pack -c Release --include-symbols
cd ..\

cd .\Schemavolution.EF6\
nuget pack -Build -Prop Configuration=Release -Symbols
cd ..\
