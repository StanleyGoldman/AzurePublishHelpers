[CmdletBinding()]
Param (
  [Parameter(Mandatory=$True,Position=0)]
  [string]$publishProfilePath,
  [Parameter(Mandatory=$True,Position=1)]
  [string]$publishPath,
  [Parameter(Mandatory=$True,Position=2)]
  [string]$package,
  [Parameter(Mandatory=$False,Position=3)]
  [string]$buildTag
)
$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Xml.Linq

function Get-PublishProfile{
	Param (
	  [Parameter(Mandatory=$True,Position=0)]
	  [string]$xmlFile
	)

	if (! [System.IO.File]::Exists($xmlFile))
    {
        return $null;
    }

	$doc = [xml] (Get-Content $xmlFile)
	$propertyGroup = $doc.Project.PropertyGroup

	$result = New-Object PSObject -Property @{
		Credentials = $propertyGroup.AzureCredentials 
        HostedServiceName = $propertyGroup.AzureHostedServiceName 
        HostedServiceLabel = $propertyGroup.AzureHostedServiceLabel 
        Slot = $propertyGroup.AzureSlot 
        EnableIntelliTrace = ($propertyGroup.AzureEnableIntelliTrace -eq "True") 
        EnableProfiling = ($propertyGroup.AzureEnableProfiling -eq "True") 
        EnableWebDeploy = ($propertyGroup.AzureEnableWebDeploy -eq "True") 
        StorageAccountName = $propertyGroup.AzureStorageAccountName 
        StorageAccountLabel = $propertyGroup.AzureStorageAccountLabel 
        DeploymentLabel = $propertyGroup.AzureDeploymentLabel 
        SolutionConfiguration = $propertyGroup.AzureSolutionConfiguration 
        ServiceConfiguration = $propertyGroup.AzureServiceConfiguration 
        AppendTimestampToDeploymentLabel = ($propertyGroup.AzureAppendTimestampToDeploymentLabel -eq "True")
        DeploymentReplacementMethod = $propertyGroup.AzureDeploymentReplacementMethod 
        DeleteDeploymentOnFailure = ($propertyGroup.AzureDeleteDeploymentOnFailure -eq "True")
        FallbackToDeleteAndRecreateIfUpgradeFails = ($propertyGroup.AzureFallbackToDeleteAndRecreateIfUpgradeFails -eq "True")
        EnableRemoteDesktop = ($propertyGroup.AzureEnableRemoteDesktop -eq "True")
	}

	return $result
}

$publishProfileFullPath = Resolve-Path $publishProfilePath
$publishProfile = Get-PublishProfile $publishProfileFullPath
Write-Host Using PublishProfile: $publishProfile.Credentials

if ((Get-Module | ?{$_.Name -eq "Azure"}) -eq $null)
{
	Import-Module 'C:\Program Files (x86)\Microsoft SDKs\Windows Azure\PowerShell\Azure\Azure.psd1'
}

function Is-DeploymentInState{
	Param (
	  [Parameter(Mandatory=$True,Position=0)]
	  [string]$serviceName,
	  [Parameter(Mandatory=$True,Position=1)]
	  [string]$slot,
	  [Parameter(Mandatory=$True,Position=2)]
	  [string]$status
	)
	
	$deployment = Get-AzureDeployment -ServiceName $serviceName -Slot $slot
	if($deployment -ne $null)
	{
		return $deployment.Status -eq $status
	}
	
	return $False;
}

function Wait-ForDeploymentInState {
	Param (
	  [Parameter(Mandatory=$True,Position=0)]
	  [string]$serviceName,
	  [Parameter(Mandatory=$True,Position=1)]
	  [string]$slot,
	  [Parameter(Mandatory=$True,Position=2)]
	  [string]$status
	)

    while ( -not $(Is-DeploymentInState $serviceName $slot $status) ) {
		Write-Host "Waiting"
        Start-Sleep -s 15
    }
}

Set-AzureSubscription -SubscriptionName $publishProfile.Credentials

$existingDeployment = Get-AzureDeployment -ServiceName $publishProfile.HostedServiceName -Slot $publishProfile.Slot
if ($existingDeployment -ne $null)
{
	Set-AzureDeployment -Status -ServiceName $publishProfile.HostedServiceName -Slot $publishProfile.Slot -NewStatus "Suspended"

	Wait-ForDeploymentInState $publishProfile.HostedServiceName $publishProfile.Slot "Suspended"

	Remove-AzureDeployment -ServiceName $publishProfile.HostedServiceName -Slot $publishProfile.Slot -Force
}

$buildLabelArray = @($publishProfile.DeploymentLabel)
if (! [string]::IsNullOrEmpty($buildTag))
{
	$buildLabelArray += $buildTag
}
if ($publishProfile.AppendTimestampToDeploymentLabel)
{
    $a = Get-Date
    $buildLabelArray += ($a.ToShortDateString() + "-" + $a.ToShortTimeString())
}

$buildLabel = [string]::Join(" - ", $buildLabelArray)

$packagePath = $publishPath + $package
$configurationPath = [string]::Format("{0}ServiceConfiguration.{1}.cscfg", $publishPath, $publishProfile.ServiceConfiguration)

New-AzureDeployment -ServiceName $publishProfile.HostedServiceName -Package $packagePath -Configuration $configurationPath -Slot $publishProfile.Slot -Label $buildLabel

Wait-ForDeploymentInState $publishProfile.HostedServiceName $publishProfile.Slot "Running"