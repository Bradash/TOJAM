$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$testDirectory = Join-Path $repositoryRoot 'Temp\ItemPairingTests'
New-Item -ItemType Directory -Path $testDirectory -Force | Out-Null
$projectPath = Join-Path $testDirectory 'ItemPairingTests.csproj'
@'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <NuGetAudit>false</NuGetAudit>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="../../Assets/Scripts/ItemPairingPlanner.cs" />
    <Compile Include="../../Tests/ItemPairingTests.cs" />
  </ItemGroup>
</Project>
'@ | Set-Content -LiteralPath $projectPath
dotnet restore $projectPath --ignore-failed-sources --verbosity quiet
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
dotnet run --project $projectPath --configuration Release --no-restore
exit $LASTEXITCODE
