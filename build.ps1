#!/bin/bash
# HASS.Agent .NET 8.0 编译脚本 (PowerShell)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "HASS.Agent .NET 8.0 编译脚本" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 检查 .NET SDK 是否安装
Write-Host "[1/5] 检查 .NET SDK 版本..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "[错误] 未找到 .NET SDK!" -ForegroundColor Red
    Write-Host ""
    Write-Host "请先安装 .NET 8.0 SDK:" -ForegroundColor Yellow
    Write-Host "https://dotnet.microsoft.com/download/dotnet/8.0"
    Write-Host ""
    Read-Host "按 Enter 键退出"
    exit 1
}

Write-Host "[信息] 检测到 .NET SDK 版本: $dotnetVersion" -ForegroundColor Green
Write-Host ""

# 检查版本是否 >= 8.0
if ($dotnetVersion -notmatch "^8\.") {
    Write-Host "[警告] 当前 .NET SDK 版本可能低于 8.0" -ForegroundColor Yellow
    Write-Host "[警告] 建议安装 .NET 8.0 SDK 或更高版本" -ForegroundColor Yellow
    Write-Host ""
    $continue = Read-Host "是否继续编译? (Y/N)"
    if ($continue -ne "Y" -and $continue -ne "y") {
        exit 1
    }
}

# 恢复 NuGet 包
Write-Host "[2/5] 恢复 NuGet 包..." -ForegroundColor Yellow
dotnet restore "src\HASS.Agent.sln"
if ($LASTEXITCODE -ne 0) {
    Write-Host "[错误] 恢复 NuGet 包失败!" -ForegroundColor Red
    Read-Host "按 Enter 键退出"
    exit 1
}
Write-Host "[成功] NuGet 包恢复完成" -ForegroundColor Green
Write-Host ""

# 编译主程序
Write-Host "[3/5] 编译主程序 (HASS.Agent)..." -ForegroundColor Yellow
dotnet build "src\HASS.Agent\HASS.Agent\HASS.Agent.csproj" -c Release -p:Platform=x64
if ($LASTEXITCODE -ne 0) {
    Write-Host "[错误] 编译主程序失败!" -ForegroundColor Red
    Read-Host "按 Enter 键退出"
    exit 1
}
Write-Host "[成功] 主程序编译完成" -ForegroundColor Green
Write-Host ""

# 编译卫星服务
Write-Host "[4/5] 编译卫星服务 (HASS.Agent.Satellite.Service)..." -ForegroundColor Yellow
dotnet build "src\HASS.Agent\HASS.Agent.Satellite.Service\HASS.Agent.Satellite.Service.csproj" -c Release -p:Platform=x64
if ($LASTEXITCODE -ne 0) {
    Write-Host "[错误] 编译卫星服务失败!" -ForegroundColor Red
    Read-Host "按 Enter 键退出"
    exit 1
}
Write-Host "[成功] 卫星服务编译完成" -ForegroundColor Green
Write-Host ""

# 运行单元测试
Write-Host "[5/5] 运行单元测试..." -ForegroundColor Yellow
dotnet test "tests\HASS.Agent.Tests\HASS.Agent.Tests.csproj" --no-build
if ($LASTEXITCODE -ne 0) {
    Write-Host "[警告] 部分单元测试失败，请检查" -ForegroundColor Yellow
    Write-Host ""
} else {
    Write-Host "[成功] 所有单元测试通过" -ForegroundColor Green
    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "编译完成!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "输出位置:" -ForegroundColor Yellow
Write-Host "主程序:     src\HASS.Agent\HASS.Agent\bin\Release\net8.0-windows10.0.19041.0\win-x64\"
Write-Host "卫星服务:   src\HASS.Agent\HASS.Agent.Satellite.Service\bin\Release\net8.0-windows10.0.19041.0\win-x64\"
Write-Host ""
Write-Host "如需创建安装包，请运行 Inno Setup 脚本:" -ForegroundColor Yellow
Write-Host "src\HASS.Agent.Installer\InstallerScript.iss"
Write-Host ""

Read-Host "按 Enter 键退出"
