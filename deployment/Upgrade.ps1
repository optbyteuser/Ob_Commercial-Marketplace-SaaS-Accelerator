# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See LICENSE file in the project root for license information.

#
# Powershell script to deploy the resources - Customer portal, Publisher portal and the Azure SQL Database
#

Param(  
   [string][Parameter(Mandatory)]$WebAppNamePrefix, # Prefix used for creating web applications
   [string][Parameter(Mandatory)]$ResourceGroupForDeployment # Name of the resource group to deploy the resources
)

Function String-Between
{
	[CmdletBinding()]
	Param(
		[Parameter(Mandatory=$true)][String]$Source,
		[Parameter(Mandatory=$true)][String]$Start,
		[Parameter(Mandatory=$true)][String]$End
	)
	$sIndex = $Source.indexOf($Start) + $Start.length
	$eIndex = $Source.indexOf($End, $sIndex)
	return $Source.Substring($sIndex, $eIndex-$sIndex)
}

$ErrorActionPreference = "Stop"
$WebAppNameAdmin=$WebAppNamePrefix+"-admin"
$WebAppNamePortal=$WebAppNamePrefix+"-portal"
$KeyVault=$WebAppNamePrefix+"-kv"

#### THIS SECTION DEPLOYS CODE AND DATABASE CHANGES
Write-host "#### Deploying new database ####" 
$ConnectionString = az keyvault secret show `
	--vault-name $KeyVault `
	--name "DefaultConnection" `
	--query "{value:value}" `
	--output tsv

# Append Timeout parameter
$ConnectionString += ";Timeout=30"

Write-host "## Retrieved ConnectionString from KeyVault"
Write-Output $ConnectionString
Set-Content -Path ../src/AdminSite/appsettings.Development.json -value "{`"ConnectionStrings`": {`"DefaultConnection`":`"$ConnectionString`"}}"

dotnet-ef migrations script `
    --idempotent `
    --context SaaSKitContext `
    --project ../src/DataAccess/DataAccess.csproj `
    --startup-project ../src/AdminSite/AdminSite.csproj `
    --output script.sql
	
Write-host "## Generated migration script"	

Write-host "## !!!Attempting to upgrade database to migration compatibility.!!!"
# Load the Npgsql assembly
Add-Type -Path "$HOME/Npgsql/npgsql-4.0.16/npgsql-4.0.16/src/Npgsql/bin/Debug/net45/Npgsql.dll"

$compatibilityScript = @"
DO $$ 
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = '__EFMigrationsHistory') THEN
        -- No __EFMigrations table means Database has not been upgraded to support EF Migrations
        CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
            "MigrationId" TEXT NOT NULL,
            "ProductVersion" TEXT NOT NULL,
            PRIMARY KEY ("MigrationId")
        );

        IF (SELECT VersionNumber FROM DatabaseVersionHistory ORDER BY CreateBy DESC LIMIT 1) = '2.10' THEN
            INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") 
            VALUES ('20221118045814_Baseline_v2', '6.0.1');
        END IF;

        IF (SELECT VersionNumber FROM DatabaseVersionHistory ORDER BY CreateBy DESC LIMIT 1) = '5.00' THEN
            INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")  
            VALUES ('20221118045814_Baseline_v2', '6.0.1'), ('20221118203340_Baseline_v5', '6.0.1');
        END IF;

        IF (SELECT VersionNumber FROM DatabaseVersionHistory ORDER BY CreateBy DESC LIMIT 1) = '6.10' THEN
            INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")  
            VALUES ('20221118045814_Baseline_v2', '6.0.1'), ('20221118203340_Baseline_v5', '6.0.1'), ('20221118211554_Baseline_v6', '6.0.1');
        END IF;
    END IF;
END $$;
"@

# Create a Npgsql connection object
$connection = New-Object Npgsql.NpgsqlConnection($ConnectionString)

Write-host "##Connection Created Successfully"
Write-Output $connection

# Open the connection
$connection.Open()

Write-host "##Connection Opened Successfully"

# Create a Npgsql command object
$command = New-Object Npgsql.NpgsqlCommand($compatibilityScript, $connection)

Write-host "##Command created Successfully"
Write-Output $command 

# Execute the script
$command.ExecuteNonQuery()

Write-host "## Ran compatibility script against database"

# Read the SQL script content
$sqlScriptPath = "script.sql"
$sqlScriptContent = Get-Content -Path $sqlScriptPath -Raw

# Create a Npgsql command object for the main SQL script content
$command = New-Object Npgsql.NpgsqlCommand($sqlScriptContent, $connection)

# Execute the script
$command.ExecuteNonQuery()

# Close the connection
$connection.Close()

Write-host "## Ran migration against database"	

Remove-Item -Path ../src/AdminSite/appsettings.Development.json
Remove-Item -Path script.sql
Write-host "#### Database Deployment complete ####"
Write-host "#### Deploying new code ####" 

dotnet publish ../src/AdminSite/AdminSite.csproj -v q -c release -o ../Publish/AdminSite/
Write-host "## Admin Portal built" 
dotnet publish ../src/MeteredTriggerJob/MeteredTriggerJob.csproj -v q -c release -o ../Publish/AdminSite/app_data/jobs/triggered/MeteredTriggerJob --runtime win-x64 --self-contained true 
Write-host "## Metered Scheduler to Admin Portal Built"
dotnet publish ../src/CustomerSite/CustomerSite.csproj -v q -c release -o ../Publish/CustomerSite
Write-host "## Customer Portal Built" 

Compress-Archive -Path ../Publish/CustomerSite/* -DestinationPath ../Publish/CustomerSite.zip -Force
Compress-Archive -Path ../Publish/AdminSite/* -DestinationPath ../Publish/AdminSite.zip -Force
Write-host "## Code packages prepared." 

Write-host "## Deploying code to Admin Portal"
az webapp deploy `
	--resource-group $ResourceGroupForDeployment `
	--name $WebAppNameAdmin `
	--src-path "../Publish/AdminSite.zip" `
	--type zip
Write-host "## Deployed code to Admin Portal"

Write-host "## Deploying code to Customer Portal"
az webapp deploy `
	--resource-group $ResourceGroupForDeployment `
	--name $WebAppNamePortal `
	--src-path "../Publish/CustomerSite.zip"  `
	--type zip
Write-host "## Deployed code to Customer Portal"

Remove-Item -Path ../Publish -recurse -Force
Write-host "#### Code deployment complete ####" 
Write-host ""
Write-host "#### Warning!!! ####"
Write-host "#### If the upgrade is to >=7.5.0, MeterScheduler feature is pre-enabled and changed to DB config instead of the App Service configuration. Please update the IsMeteredBillingEnabled value accordingly in the Admin portal -> Settings page. ####"
Write-host "#### "