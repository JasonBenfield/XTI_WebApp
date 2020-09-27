param (
    [string] $appKey="Fake",
    [string] $envName="Production",
    [string] $projectDir
)

$ErrorActionPreference = "Stop"

function Xti-Publish {
    param(
        $appKey, 
        $envName,
        $versionID,
        $WebAppUserName,
        $WebAppPassword
    )

    $ErrorActionPreference = "Stop"

    Import-Module WebAdministration
    Import-Module CredentialManager
    
    function New-XtiWebApp {

        param (
            $AppKey,
            $EnvName,
            $Version,
            $WebAppUserName,
            $WebAppPassword
        )
    
        $appFolder = "c:\XTI\WebApps\$EnvName\$AppKey"
        $targetFolder = "$appFolder\$Version"
        if ( -not (Test-Path -Path $targetFolder -PathType Container) ) {
            New-Item -ItemType Directory -Path $targetFolder
        }
        $appPoolName = "Xti_$($EnvName)_$($AppKey)_$($Version)"
        $appPoolPath = "IIS:\AppPools\$appPoolName"
        
        if (-not (Test-Path $appPoolPath)) {
            New-WebAppPool -Name $appPoolName -Force
            Set-ItemProperty $appPoolPath -name processModel -value @{userName=$webAppUserName;password=$webAppPassword;identitytype=3}
        }
        if($EnvName -eq "Production") {
            $siteName = "WebApps"
        }
        else {
            $siteName = $EnvName
        }
        if(-not (Test-Path "IIS:\Sites\$siteName\$AppKey")) {
            New-WebVirtualDirectory -Site $siteName -Name $AppKey -PhysicalPath $appFolder
        }
        if((Get-WebApplication -Name $Version -Site "$siteName\$AppKey") -eq $null) {
            New-WebApplication -Name $Version -Site "$siteName\$AppKey" -PhysicalPath $targetFolder -ApplicationPool $appPoolName -Force
        }
    }

    New-XtiWebApp -AppKey $appKey -EnvName $envName -WebAppUserName $WebAppUserName -WebAppPassword $WebAppPassword -Version "Current"
    if($envName -eq "Production") { 
        New-XtiWebApp -AppKey $appKey -EnvName $envName -WebAppUserName $WebAppUserName -WebAppPassword $WebAppPassword -Version "V$versionID"
    }
}

$webAppCred = Get-StoredCredential -Target xti_webapp
$webAppUserName = [string]$webAppCred.UserName
$BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($webAppCred.Password)
$webAppPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)

if ($envName -eq "Production" -or $envName -eq "Staging") {
    $cred = Get-StoredCredential -Target xti_productionmachine_admin
    $userName = [string]$cred.UserName
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($cred.Password)
    $password = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)

    if($envName -eq "Production") {
        $branch = (git rev-parse --abbrev-ref HEAD) | Out-String
        $versionApp = $env:XTI_Tools + "\XTI_VersionApp\XTI_VersionApp.exe"
        & $versionApp --ManageVersion:Command=BeginPublish --ManageVersion:PublishVersion:Branch=$branch
        $versionID = [int]($branch -split '/')[-1]
    }
    else {
        $versionID = 0
    }

    $parameters = @{
        ComputerName = $env:XTI_ProductionMachine
        Credential = $cred
        ScriptBlock = ${Function:Xti-Publish}
        ArgumentList = $appKey, $envName, $versionID, $webAppUserName, $webAppPassword
    }
    Invoke-Command @parameters
    if($envName -eq "Production"){
        dotnet publish ./Apps/FakeWebApp /p:PublishProfile=$envName /p:DeployIisAppPath=WebApps/Fake/V${versionID} /p:Password=$password
        dotnet publish ./Apps/FakeWebApp /p:PublishProfile=$envName /p:Password=$password
        & $versionApp --ManageVersion:Command=EndPublish --ManageVersion:PublishVersion:Branch=$branch
    }
    else{
        dotnet publish ./Apps/FakeWebApp /p:PublishProfile=$envName /p:Password=$password
    }
}
else {
    Xti-Publish -appKey $appKey -envName $envName -versionID 0 -WebAppUserName $WebAppUserName -WebAppPassword $WebAppPassword
    dotnet publish ./Apps/FakeWebApp /p:PublishProfile=$envName
}

