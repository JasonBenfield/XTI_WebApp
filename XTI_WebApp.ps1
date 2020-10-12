Import-Module PowershellForXti -Force

$script:config = [PSCustomObject]@{
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
        $Label = @(),
        [string] $Body = ""
    )
    $script:config | New-XtiIssue @PsBoundParameters
}

function WebApp-Xti-StartIssue {
    param(
        [Parameter(Position=0)]
        [long]$IssueNumber = 0,
        $IssueBranchTitle = "",
        $AssignTo = ""
    )
    $script:config | Xti-StartIssue @PsBoundParameters
}

function WebApp-New-XtiVersion {
    param(
        [Parameter(Position=0)]
        [ValidateSet("major", "minor", "patch")]
        $VersionType = "minor",
        [ValidateSet("Development", "Production", "Staging", "Test")]
        $EnvName = "Production"
    )
    $script:config | New-XtiVersion @PsBoundParameters
}

function WebApp-New-XtiPullRequest {
    param(
        [Parameter(Position=0)]
        [string] $CommitMessage
    )
    $script:config | New-XtiPullRequest @PsBoundParameters
}

function WebApp-Xti-PostMerge {
    param(
    )
    $script:config | Xti-PostMerge @PsBoundParameters
}

function WebApp-Publish {
    param(
        [switch] $Dev
    )
    Xti-PublishPackage @PsBoundParameters
}
