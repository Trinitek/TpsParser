
param (
    [Parameter]
    [string]
    $VersionSuffix
)

dotnet pack `
    --version-suffix "$VersionSuffix" `
    -p:ContinuousIntegrationBuild=true
