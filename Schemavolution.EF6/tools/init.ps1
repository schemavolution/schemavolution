param($installPath, $toolsPath, $package, $project)

if (Get-Module | ?{ $_.Name -eq 'Schemavolution' })
{
    Remove-Module Schemavolution
}

Import-Module (Join-Path $toolsPath Schemavolution.psd1) 3> $Null -ArgumentList $toolsPath
