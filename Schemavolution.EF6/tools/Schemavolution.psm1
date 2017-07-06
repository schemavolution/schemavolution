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
		[string] $Assembly,
		[string] $Database,
		[string] $MasterConnectionString,
        [switch] $Force)

	$AbsoluteAssemblyPath = (Resolve-Path $Assembly).Path
	[Schemavolution.EF6.Commands.Commands]::EvolveDatabase($AbsoluteAssemblyPath, $Database, $MasterConnectionString, $Force)
    Write-Host "Your database is evolved"
}


Export-ModuleMember @( 'Evolve-Database' )