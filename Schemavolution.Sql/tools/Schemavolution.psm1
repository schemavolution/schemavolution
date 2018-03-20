param($toolsPath)

<#
.SYNOPSIS
    Applies the project genome to the database.

.DESCRIPTION
    Updates the database to the current genome by applying any pending genes.

.PARAMETER Force
    Unapplies any genes that are not in the target genome. This may result in data loss!!!

.EXAMPLE
	Evolve-Database
	# Update the database to the current genome

.EXAMPLE
	Evolve-Database -Force
	# Update the database to the current genome, rolling back any genes that are no
    longer in the current genome.
#>
function Evolve-Database
{
    [CmdletBinding()]
    param (
		[string] $Assembly = "",
		[string] $Database = "",
		[string] $MasterConnectionString = "",
        [switch] $Force)

	try
	{
		If ($Assembly -eq "")
		{
			$GenomeProject = Get-Project
			$ProjectPath = $GenomeProject.Properties.Item("FullPath").Value
			$OutputPath = $GenomeProject.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value
			$OutputFileName = $GenomeProject.Properties.Item("OutputFileName").Value
			$AssemblyPath = [io.path]::Combine($ProjectPath, $OutputPath, $OutputFileName)
			$AbsoluteAssemblyPath = Resolve-Path $AssemblyPath

			$Configuration = $DTE.Solution.SolutionBuild.ActiveConfiguration.Name
			$DTE.Solution.SolutionBuild.BuildProject($Configuration, $GenomeProject.UniqueName, $True)

			If ($DTE.Solution.SolutionBuild.LastBuildInfo)
			{
				Throw "The project $($GenomeProject.Name) failed to build."
			}
		}
		Else
		{
			$AbsoluteAssemblyPath = (Resolve-Path $Assembly).Path
		}

		If (($Database -eq "") -or ($MasterConnectionString -eq ""))
		{
			$StartupProjectPath = $DTE.Solution.SolutionBuild.StartupProjects[0]
			$StartupProject = $DTE.Solution.Projects | ?{ $_.FullName.EndsWith($StartupProjectPath) }
			$StartupProjectPath = $StartupProject.Properties.Item("FullPath").Value
			$WebConfigPath = Join-Path $StartupProjectPath "web.config"
			If ((Test-Path $WebConfigPath) -eq $False)
			{
				Throw "The startup project should have a web.config file containing one connection string. $WebConfigPath does not exist."
			}
			[xml]$WebConfig = Get-Content $WebConfigPath
			$ConnectionString = $WebConfig.configuration.connectionStrings.add.connectionString
			If ($ConnectionString -eq $Null)
			{
				Throw "The startup project should have a web.config file containing one connection string. $WebConfigPath does not have a connection string."
			}
			If ($ConnectionString -is [Array])
			{
				Throw "The startup project should have a web.config file containing one connection string. $WebConfigPath contains more than one."
			}
			$Builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder $ConnectionString

			If ($Database -eq "")
			{
				$Database = $Builder["Initial Catalog"]
			}

			If ($MasterConnectionString -eq "")
			{
				$Builder["Initial Catalog"] = "master"
				$MasterConnectionString = $Builder.ConnectionString
			}
		}
		
		If ($Force -eq $True)
		{
			$Args = @($AbsoluteAssemblyPath, $Database, $MasterConnectionString, "-f")
		}
		Else
		{
			$Args = @($AbsoluteAssemblyPath, $Database, $MasterConnectionString)
		}
		$Command = Join-Path $toolsPath "schemav.exe"
		$Command = Resolve-Path $Command
		& $Command $Args
	}
	catch
	{
		Write-Host $_.Exception.Message -ForegroundColor Red
	}
}


Export-ModuleMember @( 'Evolve-Database' )