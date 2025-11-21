
param (
    [Parameter(Mandatory=$true)]
    [string]
    $VersionSuffix
)

dotnet pack `
    --version-suffix "$VersionSuffix" `
    -p:ContinuousIntegrationBuild=true
