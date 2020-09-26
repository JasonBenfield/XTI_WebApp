param ([string] $versionType='Minor')
$versionApp = $env:XTI_Tools + "\XTI_VersionApp\XTI_VersionApp.exe"
& $versionApp --ManageVersion:Command=New --ManageVersion:NewVersion:App=Fake --ManageVersion:NewVersion:Type=$versionType --ManageVersion:NewVersion:RepoOwner=JasonBenfield --ManageVersion:NewVersion:RepoName=FakeWebApp