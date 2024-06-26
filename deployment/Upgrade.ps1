﻿# Copyright (c) Microsoft Corporation. All rights reserved.
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


Write-host "## Retrieved ConnectionString from KeyVault"
# Extract connection parameters
$dbHost  = (($ConnectionString -split ";")[0] -split "=")[-1]
$port = (($ConnectionString -split ";")[1] -split "=")[-1]
$database = (($ConnectionString -split ";")[2] -split "=")[-1]
$user = (($ConnectionString -split ";")[3] -split "=")[-1]
$password = (($ConnectionString -split ";")[4] -split "=")[-1]

Write-Output "##Parameters"

Write-Output $dbHost   

Write-Output $port 

Write-Output $database 

Write-Output $password 

Write-Output $ConnectionString
Set-Content -Path ../src/AdminSite/appsettings.Development.json -value "{`"ConnectionStrings`": {`"DefaultConnection`":`"$ConnectionString`"}}"

dotnet-ef migrations script `
    --idempotent `
    --context SaaSKitContext `
    --project ../src/DataAccess/DataAccess.csproj `
    --startup-project ../src/AdminSite/AdminSite.csproj `
    --output /home/sayali/script.sql
	


Write-host "## Generated migration script"	

Write-host "## !!!Attempting to upgrade database to migration compatibility.!!!"

$compatibilityScript = @"
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL,
    "ProductVersion" TEXT NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
SELECT '20221118045814_Baseline_v2', '6.0.1'
WHERE NOT EXISTS (SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20221118045814_Baseline_v2');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
SELECT '20221118203340_Baseline_v5', '6.0.1'
WHERE NOT EXISTS (SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20221118203340_Baseline_v5');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
SELECT '20221118211554_Baseline_v6', '6.0.1'
WHERE NOT EXISTS (SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20221118211554_Baseline_v6');
"@

# Execute compatibility script against database
# psql --host=$dbHost  --port=$port --username=$user --dbname=$database --command="$compatibilityScript"

# Execute migration script against database
psql --host=$dbHost  --port=$port --username=$user --dbname=$database --file="$Home/script.sql"

Write-host "## Ran migration against database"	

Remove-Item -Path ../src/AdminSite/appsettings.Development.json
Remove-Item -Path $Home/script.sql
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