$ErrorActionPreference = "Stop"

$projectDirectory = $PSScriptRoot
$sourceFile = Join-Path $projectDirectory "KeyboardTester.cs"
$manifestFile = Join-Path $projectDirectory "app.manifest"
$releaseDirectory = Join-Path $projectDirectory "release"
$outputFile = Join-Path $releaseDirectory "KeyboardTester.exe"
$temporaryOutput = Join-Path $releaseDirectory ".KeyboardTester.new.exe"
$selfTestLog = Join-Path $projectDirectory "self-test-result.txt"

$compilerCandidates = @(
    "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe",
    "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
)
$compiler = $compilerCandidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1

if (-not $compiler) {
    throw "未找到 Windows .NET Framework C# 编译器 (csc.exe)。"
}

if (-not (Test-Path -LiteralPath $releaseDirectory)) {
    New-Item -ItemType Directory -Path $releaseDirectory | Out-Null
}

& $compiler `
    /nologo `
    /target:winexe `
    /optimize+ `
    /win32manifest:$manifestFile `
    /out:$temporaryOutput `
    /reference:System.Windows.Forms.dll `
    /reference:System.Drawing.dll `
    /reference:System.Core.dll `
    $sourceFile

if ($LASTEXITCODE -ne 0) {
    throw "编译失败，退出代码：$LASTEXITCODE"
}

Move-Item -LiteralPath $temporaryOutput -Destination $outputFile -Force

$testProcess = Start-Process `
    -FilePath $outputFile `
    -ArgumentList "--self-test" `
    -WorkingDirectory $projectDirectory `
    -WindowStyle Hidden `
    -Wait `
    -PassThru

if ($testProcess.ExitCode -ne 0) {
    $details = if (Test-Path -LiteralPath $selfTestLog) {
        Get-Content -Raw -LiteralPath $selfTestLog
    } else {
        "未生成自检日志。"
    }
    throw "程序自检失败：`n$details"
}

Write-Host "构建完成：$outputFile"
Write-Host "自检通过：$selfTestLog"
