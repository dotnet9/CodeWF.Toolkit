[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("win-x64", "linux-x64", "linux-arm64", "osx-x64", "osx-arm64")]
    [string] $RuntimeIdentifier,

    [string] $Version = ""
)

$ErrorActionPreference = "Stop"

$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..")).Path
$projectPath = Join-Path $repositoryRoot "demo/CodeWF.Toolkit.Demo.Avalonia/CodeWF.Toolkit.Demo.Avalonia.csproj"

if ([string]::IsNullOrWhiteSpace($Version)) {
    $props = Join-Path $repositoryRoot "Directory.Build.props"
    $match = Select-String -LiteralPath $props -Pattern '<Version>([^<]+)</Version>' | Select-Object -First 1
    if ($match) { $Version = $match.Matches[0].Groups[1].Value.Trim() }
    if ([string]::IsNullOrWhiteSpace($Version)) { $Version = "0.0.0" }
}

# 输出目录固定为 artifacts/publish/<rid>/CodeWF.Toolkit.Demo.Avalonia，保证本地与 CI 各处路径一致。
# 全平台 NativeAOT：完整反射元数据保全 Prism/DryIoc，单线程 ILC 更稳。
$outputPath = Join-Path $repositoryRoot "artifacts/publish/$RuntimeIdentifier/CodeWF.Toolkit.Demo.Avalonia"
if (Test-Path -LiteralPath $outputPath) {
    Remove-Item -LiteralPath $outputPath -Recurse -Force
}

$ilcArgs = @(
    "-p:PublishAot=true",
    "-p:PublishTrimmed=true",
    "-p:PublishSingleFile=false",
    "-p:IlcGenerateCompleteTypeMetadata=true",
    "-p:IlcTrimMetadata=false",
    "-p:IlcSingleThreaded=true",
    "-p:Version=$Version"
)
# macOS 保留符号（ld_classic 不支持压缩调试段）
if ($RuntimeIdentifier.StartsWith("osx-", [StringComparison]::OrdinalIgnoreCase)) { $ilcArgs += "-p:StripSymbols=false" }

Write-Host "发布 CodeWF.Toolkit.Demo.Avalonia $RuntimeIdentifier (Version=$Version)"
& dotnet publish $projectPath -c Release -r $RuntimeIdentifier @ilcArgs -o $outputPath
if ($LASTEXITCODE -ne 0) { throw "publish failed for $RuntimeIdentifier" }

if (-not (Test-Path -LiteralPath $outputPath -PathType Container)) {
    throw "Publish output missing: $outputPath"
}

Write-Host "已发布到 $outputPath"
