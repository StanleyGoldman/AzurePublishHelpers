[CmdletBinding()]
Param (
  [Parameter(Mandatory=$True,Position=0)]
  [string]$publishProfilePath,
  [Parameter(Mandatory=$True,Position=1)]
  [string]$publishPath,
  [Parameter(Mandatory=$True,Position=2)]
  [string]$package
)

Import-Module AzurePublishHelpers

$azurePubXml = New-Object AzurePublishHelpers.AzurePubXmlHelper
$publishProfileFullPath = Resolve-Path $publishProfilePath
$publishProfile = $azurePubXml.GetPublishProfile($publishProfileFullPath)
Write-Host Using PublishProfile: $publishProfile.ConnectionName

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

Set-AzureSubscription -SubscriptionName $publishProfile.ConnectionName

$existingDeployment = Get-AzureDeployment -ServiceName $publishProfile.HostedServiceName -Slot $publishProfile.DeploymentSlot
if ($existingDeployment -ne $null)
{
	Set-AzureDeployment -Status -ServiceName $publishProfile.HostedServiceName -Slot $publishProfile.DeploymentSlot -NewStatus "Suspended"

	Wait-ForDeploymentInState $publishProfile.HostedServiceName $publishProfile.DeploymentSlot "Suspended"

	Remove-AzureDeployment -ServiceName $publishProfile.HostedServiceName -Slot $publishProfile.DeploymentSlot -Force
}

$buildLabel = $publishProfile.DeploymentLabel
if ($publishProfile.AppendTimestampToDeploymentLabel)
{
    $a = Get-Date
    $buildLabel = $buildLabel + "-" + $a.ToShortDateString() + "-" + $a.ToShortTimeString()
}

$packagePath = $publishPath + $package
$configurationPath = [string]::Format("{0}ServiceConfiguration.{1}.cscfg", $publishPath, $publishProfile.ServiceConfiguration)

New-AzureDeployment -ServiceName $publishProfile.HostedServiceName -Package $packagePath -Configuration $configurationPath -Slot $publishProfile.DeploymentSlot -Label $buildLabel

Wait-ForDeploymentInState $publishProfile.HostedServiceName $publishProfile.DeploymentSlot "Running"