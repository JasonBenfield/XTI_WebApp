Import-Module PowershellForXti -Force

$script:webAppConfig = [PSCustomObject]@{
    RepoOwner = "JasonBenfield"
    RepoName = "XTI_WebApp"
    AppKey = "XTI_WebApp"
    AppType = "Package"
    ProjectDir = "C:\XTI\src\XTI_WebApp\Apps\XTI_WebApp"
}

function WebApp-New-XtiIssue {
    param(
        [Parameter(Mandatory, Position=0)]
        [string] $IssueTitle,
        $Labels = @(),
        [string] $Body = "",
        [switch] $Start
    )
    $script:webAppConfig | New-XtiIssue @PsBoundParameters
}

function WebApp-Xti-StartIssue {
    param(
        [Parameter(Position=0)]
        [long]$IssueNumber = 0,
        $IssueBranchTitle = "",
        $AssignTo = ""
    )
    $script:webAppConfig | Xti-StartIssue @PsBoundParameters
}

function WebApp-New-XtiVersion {
    param(
        [Parameter(Position=0)]
        [ValidateSet("major", "minor", "patch")]
        $VersionType = "minor",
        [ValidateSet("Development", "Production", "Staging", "Test")]
        $EnvName = "Production"
    )
    $script:webAppConfig | New-XtiVersion @PsBoundParameters
}

function WebApp-New-XtiPullRequest {
    param(
        [Parameter(Position=0)]
        [string] $CommitMessage
    )
    $script:webAppConfig | New-XtiPullRequest @PsBoundParameters
}

function WebApp-Xti-PostMerge {
    param(
    )
    $script:webAppConfig | Xti-PostMerge @PsBoundParameters
}

function WebApp-Publish {
    param(
        [switch] $Prod
    )
    $script:webAppConfig | Xti-PublishPackage @PsBoundParameters
}
