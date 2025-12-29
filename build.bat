@echo off
REM HASS.Agent .NET 8.0 编译脚本
REM 使用方法: 双击运行或在命令行执行

echo ========================================
echo HASS.Agent .NET 8.0 编译脚本
echo ========================================
echo.

REM 检查 .NET SDK 是否安装
echo [1/5] 检查 .NET SDK 版本...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [错误] 未找到 .NET SDK!
    echo.
    echo 请先安装 .NET 8.0 SDK:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    pause
    exit /b 1
)

for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo [信息] 检测到 .NET SDK 版本: %DOTNET_VERSION%
echo.

REM 检查版本是否 >= 8.0
echo %DOTNET_VERSION% | findstr /r "^8\." >nul
if errorlevel 1 (
    echo [警告] 当前 .NET SDK 版本可能低于 8.0
    echo [警告] 建议安装 .NET 8.0 SDK 或更高版本
    echo.
    choice /C YN /M "是否继续编译?"
    if errorlevel 2 exit /b 1
)

echo.
echo [2/5] 恢复 NuGet 包...
dotnet restore "src\HASS.Agent.sln"
if errorlevel 1 (
    echo [错误] 恢复 NuGet 包失败!
    pause
    exit /b 1
)
echo [成功] NuGet 包恢复完成
echo.

echo [3/5] 编译主程序 (HASS.Agent)...
dotnet build "src\HASS.Agent\HASS.Agent\HASS.Agent.csproj" -c Release -p:Platform=x64
if errorlevel 1 (
    echo [错误] 编译主程序失败!
    pause
    exit /b 1
)
echo [成功] 主程序编译完成
echo.

echo [4/5] 编译卫星服务 (HASS.Agent.Satellite.Service)...
dotnet build "src\HASS.Agent\HASS.Agent.Satellite.Service\HASS.Agent.Satellite.Service.csproj" -c Release -p:Platform=x64
if errorlevel 1 (
    echo [错误] 编译卫星服务失败!
    pause
    exit /b 1
)
echo [成功] 卫星服务编译完成
echo.

echo [5/5] 运行单元测试...
dotnet test "tests\HASS.Agent.Tests\HASS.Agent.Tests.csproj" --no-build
if errorlevel 1 (
    echo [警告] 部分单元测试失败，请检查
    echo.
) else (
    echo [成功] 所有单元测试通过
    echo.
)

echo ========================================
echo 编译完成!
echo ========================================
echo.
echo 输出位置:
echo 主程序:     src\HASS.Agent\HASS.Agent\bin\Release\net8.0-windows10.0.19041.0\win-x64\
echo 卫星服务:   src\HASS.Agent\HASS.Agent.Satellite.Service\bin\Release\net8.0-windows10.0.19041.0\win-x64\
echo.
echo 如需创建安装包，请运行 Inno Setup 脚本:
echo src\HASS.Agent.Installer\InstallerScript.iss
echo.

pause
