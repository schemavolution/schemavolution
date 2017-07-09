function PushAllPackages(
	[string] $Directory)
{
	$Specification = Join-Path $Directory "*.nupkg"
	$Packages = (Get-Item $Specification).Name | ?{ $_.EndsWith("symbols.nupkg")-eq $False }
	Foreach ($Package in $Packages)
	{
		$PackageFile = Join-Path $Directory $Package
		$Args = @("push", $PackageFile, "-Source", "https://nuget.org/")
		& nuget.exe $Args
	}
}

PushAllPackages "Schemavolution.Specification\bin\Release\"
PushAllPackages "Schemavolution.EF6\"
PushAllPackages "Schemavolution.DDD\bin\Release\"
