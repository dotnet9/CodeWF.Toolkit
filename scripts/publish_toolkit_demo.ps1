[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("win-x64", "linux-x64", "linux-arm64", "osx-x64", "osx-arm64")]
    [string] $RuntimeIdentifier,

    [string] $Version = "",
    [string] $BuildArtifactsPath = "",
    [string] $OutputDirectory = ""
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

# 默认输出保持与 CI 一致；本机可显式指定仓库外的空目录。
# 全平台 NativeAOT：完整反射元数据保全 Prism/DryIoc，单线程 ILC 更稳。
if ($OutputDirectory) {
    $outputPath = [IO.Path]::GetFullPath($OutputDirectory)
    # 外部目录不自动清理，防止覆盖调用方已有文件，同时避免旧产物混入验证。
    if ((Test-Path -LiteralPath $outputPath) -and
        (-not (Test-Path -LiteralPath $outputPath -PathType Container) -or
         (Get-ChildItem -LiteralPath $outputPath -Force | Select-Object -First 1))) {
        throw "自定义发布输出目录必须为空：$outputPath"
    }
} else {
    $outputPath = [IO.Path]::GetFullPath((Join-Path $repositoryRoot "artifacts/publish/$RuntimeIdentifier/CodeWF.Toolkit.Demo.Avalonia"))
    $allowedOutputRoot = [IO.Path]::GetFullPath((Join-Path $repositoryRoot 'artifacts/publish')) + [IO.Path]::DirectorySeparatorChar
    if (-not $outputPath.StartsWith($allowedOutputRoot, [StringComparison]::OrdinalIgnoreCase)) {
        throw "发布输出目录超出仓库产物范围：$outputPath"
    }
    if (Test-Path -LiteralPath $outputPath) {
        Remove-Item -LiteralPath $outputPath -Recurse -Force
    }
}

$ilcArgs = @(
    "-p:PublishAot=true",
    "-p:PublishTrimmed=true",
    "-p:PublishSingleFile=false",
    "-p:DebugSymbols=true",
    "-p:DebugType=portable",
    "-p:IlcGenerateCompleteTypeMetadata=true",
    "-p:IlcTrimMetadata=false",
    "-p:IlcSingleThreaded=true",
    "-p:Version=$Version"
)
# macOS 保留符号（ld_classic 不支持压缩调试段）
if ($BuildArtifactsPath) { $ilcArgs += "-p:ArtifactsPath=$([IO.Path]::GetFullPath($BuildArtifactsPath))" }
if ($RuntimeIdentifier.StartsWith("osx-", [StringComparison]::OrdinalIgnoreCase)) { $ilcArgs += "-p:StripSymbols=false" }

Write-Host "发布 CodeWF.Toolkit.Demo.Avalonia $RuntimeIdentifier (Version=$Version)"
& dotnet publish $projectPath -c Release -r $RuntimeIdentifier -m:1 @ilcArgs -o $outputPath
if ($LASTEXITCODE -ne 0) { throw "publish failed for $RuntimeIdentifier" }

if (-not (Test-Path -LiteralPath $outputPath -PathType Container)) {
    throw "Publish output missing: $outputPath"
}

Write-Host "已发布到 $outputPath"
