## AzurePublishHelpers for Azure 1.7 SDK

Technologies
MSBuild, Windows Azure, Visual Studio 2010
Topics
Windows Azure, ALM, Build & Deployment
Last Updated
12/5/2011
License
Apache License, Version 2.0
View this sample online
Introduction

Once you install the Windows Azure SDK on your development machine, it's easy to deploy your application to Windows Azure. However while this approach is fine for individual developers, project teams often prefer to build and deploy their code centrally on a build server. This improves the predictability of the build and deployment process, and also allows you to complete other tasks as a part of your build, such as running unit tests, deploying a database of versioning assemblies.

This sample includes scripts and tools that make it easy for you to set up centralised build and deployment of Windows Azure applications. The sample uses MSBuild and PowerShell, and assumes you are using Team Foundation Server for source control and build (although you should be able to use this sample as a starting point if you are using other ALM tools).
Sample Contents

This sample contains the following files and projects:

    AzureDeploy.targets: An MSBuild Targets file which will automatically package your Windows Azure projects and call a PowerShell script to deploy it to the cloud.
    AzureDeploy.ps1: A PowerShell script to deploy a packaged Windows Azure application to the cloud.
    ImportPublishSettings.exe: A command-line tool which can import Windows Azure credentials from a .publishsettings file so it can be used on a build server
    AzurePublishHelpers.dll: A library of helper functions used by AzureDeploy.ps1 and ImportPublishSettings.exe. 

Setting up your Build

To set up your build server:

    Install Windows Azure SDK 1.6 (http://www.microsoft.com/windowsazure/sdk)
    Install Windows Azure Platform PowerShell Cmdlets (http://wappowershell.codeplex.com)
    Compile the code in this sample (ImportPublishSettings.exe and AzurePublishHelpers.dll)
     Copy both files to C:\Build on your build server
    Install AzurePublishHelpers.dll as a PowerShell cmdlet by copying to: C:\Windows\System32\WindowsPowerShell\v1.0\Modules\AzurePublishHelpers (or C:\Windows\SysWOW64\WindowsPowerShell\v1.0\Modules\AzurePublishHelpers if you are running a 32-bit build on a 64-bit machine)
    Copy AzureDeploy.ps1 to "C:\Build" (see comments in the file about changes if your build runs as NETWORK SERVICE)
    Copy AzureDeploy.targets to C:\Program Files\MSBuild\Microsoft\VisualStudio\v10.0\Windows Azure Tools\1.6\ImportAfter (32-bit OS) or C:\Program Files (x86)\MSBuild\Microsoft\VisualStudio\v10.0\Windows Azure Tools\1.6\ImportAfter (64-bit OS)
    Go to http://code.msdn.microsoft.com/Windows-Azure-Build-8cee065d/https://windows.azure.com/download/publishprofile.aspx to download a publish profile for your Windows Azure subscription.
    Run ImportPublishSettings.exe to import the subscription details and certificate onto the build server.
    If you're running the build as NETWORK SERVICE or similar non-user account:
        Make sure you specify LocalMachine as the certificate store and a custom Windows Azure Connections file
        Use the Certificates MMC snap-in to grant access to the certificate private key to the build account 

To set up your Windows Azure project ready for the build:

    Open Visual Studio and open/create a new Windows Azure Project for your application
    Right-click on the Windows Azure Project (.ccproj) in Solution Explorer and choose Publish
    Configure all of the publish details for your project (specify the hosted service name, deployment slot, etc) and save this to an .azurePubXml file in the project. Cancel out of the dialog (i.e. you do not need to publish to Windows Azure from Visual Studio).
    Check your solution into TFS source control. 

To set up your build definition in TFS:

    Open Visual Studio, connect to TFS via Team Explorer and set up a new Build Definition
    Configure the build definition details as appropriate for your project
    On the Process tab, Expand the Advanced group, and enter the following under "MSBuild Arguments":
         /p:AzurePublishProfile="myServiceProduction.azurePubxml"
       (or whatever you called your publish profile name)
    Save the build, and kick it off! 

More Information

For more details on this sample, please see Automated Build and Deployment with Windows Azure SDK 1.6.
