# 代码签名配置指南

## 概述

HASS.Agent 的安装包包含 `HASS.Agent.Satellite.Service.exe`（Windows 服务程序），Windows 可能会将其标记为"不安全"或显示未知发布者警告。这是因为该程序未经代码签名证书签名。

## 问题说明

### 为什么会出现警告?

Windows SmartScreen 使用以下因素评估应用程序的安全性：

1. **代码签名证书** - 应用程序是否使用受信任的证书签名
2. **下载频率** - 其他用户是否下载了此应用程序
3. **数字签名** - 是否有有效的数字签名
4. **安全信誉** - 应用程序的安全历史记录

### 影响范围

- ✅ **不影响功能**: 程序可以正常运行
- ⚠️ **用户警告**: 首次运行时显示警告
- ⚠️ **浏览器警告**: 下载时可能被标记

## 解决方案

### 方案 1: 使用代码签名证书（推荐用于正式发布）

#### 购买代码签名证书

从受信任的证书颁发机构（CA）购买：

- **DigiCert** - https://www.digicert.com/signing/code-signing-certificates
- **Sectigo** - https://www.sectigo.com/ssl-certificates-tls/code-signing
- **GlobalSign** - https://www.globalsign.com/en/code-signing-certificate
- **SSL.com** - https://www.ssl.com/certificates/code-signing/

**成本**: 约 $200-500/年

#### 配置代码签名

1. **转换证书为 PFX 格式**

```powershell
# 如果您有 .p12 或 .pfx 证书
# 确保密码已设置
```

2. **编码证书为 Base64**（用于 GitHub Secrets）

```powershell
$pfx_cert = Get-Content '.\YourCertificate.pfx' -Encoding Byte
[System.Convert]::ToBase64String($pfx_cert) | Out-File 'Certificate_Encoded.txt'
```

3. **配置 GitHub Secrets**

在 GitHub 仓库设置中添加以下 Secrets:

- `BASE64_ENCODED_PFX` - 粘贴 `Certificate_Encoded.txt` 的内容
- `BASE64_ENCODED_PFX_PASSWORD` - 证书密码

4. **更新 GitHub Actions**

项目已包含代码签名步骤（在 `.github/workflows/build.yml` 中）:

```yaml
- name: Decode the pfx
  run: |
    $pfxBytes = [System.Convert]::FromBase64String("${{ secrets.BASE64_ENCODED_PFX }}")
    $certificatePath = [IO.Path]::GetFullPath(".\HASS.Agent.Installer.pfx")
    [IO.File]::WriteAllBytes($certificatePath, $pfxBytes)

- name: Sign the installer
  run: |
    $certificatePassword = ConvertTo-SecureString "${{ secrets.BASE64_ENCODED_PFX_PASSWORD }}" -AsPlainText -Force
    $certificate = Get-PfxCertificate -FilePath ".\HASS.Agent.Installer.pfx" -Password $certificatePassword
    Set-AuthenticodeSignature -FilePath ".\bin\HASS.Agent.Installer.exe" -Certificate $certificate -HashAlgorithm SHA256 -TimestampServer http://timestamp.digicert.com
```

### 方案 2: 本地代码签名（用于个人使用）

如果您有代码签名证书，可以在本地签名：

```powershell
# 安装 .NET 8.0 SDK
# 恢复依赖并编译
dotnet restore "src\HASS.Agent\HASS.Agent\HASS.Agent.csproj"
dotnet restore "src\HASS.Agent\HASS.Agent.Satellite.Service\HASS.Agent.Satellite.Service.csproj"

# 编译主程序
dotnet publish "src\HASS.Agent\HASS.Agent\HASS.Agent.csproj" -c Release -f net8.0-windows10.0.19041.0 -o "publish\HASS.Agent" --no-self-contained -r win-x64 -p:Platform=x64

# 编译卫星服务
dotnet publish "src\HASS.Agent\HASS.Agent.Satellite.Service\HASS.Agent.Satellite.Service.csproj" -c Release -f net8.0-windows10.0.19041.0 -o "publish\Satellite.Service" --no-self-contained -r win-x64 -p:Platform=x64

# 签名可执行文件
$cert = Get-PfxCertificate -FilePath "YourCertificate.pfx"
$certPassword = ConvertTo-SecureString "YourPassword" -AsPlainText -Force

# 签名主程序
Set-AuthenticodeSignature -FilePath "publish\HASS.Agent\HASS.Agent.exe" -Certificate $cert -HashAlgorithm SHA256 -TimestampServer http://timestamp.digicert.com

# 签名卫星服务
Set-AuthenticodeSignature -FilePath "publish\Satellite.Service\HASS.Agent.Satellite.Service.exe" -Certificate $cert -HashAlgorithm SHA256 -TimestampServer http://timestamp.digicert.com
```

### 方案 3: 免费代码签名（仅用于测试）

#### 使用 SignPath（开源项目免费）

SignPath 为开源项目提供免费的代码签名服务：

- **网站**: https://signpath.io/
- **开源申请**: https://signpath.io/for-open-source-projects/

#### 使用 Azure Code Signing（预览版）

微软提供免费的 Azure 代码签名服务（目前为预览版）:

- **文档**: https://docs.microsoft.com/azure/security/fundamentals/code-signing

### 方案 4: 用户端解决方案（无证书）

如果无法获得代码签名证书，用户可以：

1. **点击"更多信息"** → **"仍要运行"**
2. **添加到 Windows Defender 排除项**
3. **使用组策略允许未签名的应用程序**
4. **从 GitHub Releases 下载验证哈希**

#### 验证文件哈希

发布时提供 SHA256 哈希值：

```powershell
# 生成哈希
certutil -hashfile HASS.Agent.Installer.exe SHA256

# 用户验证哈希
# 在下载的文件上运行相同命令，比较输出
```

## 推荐策略

### 对于正式发布

✅ **购买代码签名证书**（推荐）
- 受信任的 CA 证书
- 无 SmartScreen 警告
- 提升用户信任度

✅ **使用 SignPath（如果符合条件）**
- 开源项目免费
- 专业的签名服务

### 对于测试/开发

⚠️ **使用自签名证书**
```powershell
# 创建自签名证书（仅用于本地测试）
New-SelfSignedCertificate -Type CodeSigningCert -Subject "CN=HASS.Agent Test" -CertStoreLocation Cert:\LocalMachine\My
```

⚠️ **提供哈希验证**
- 在 Releases 中提供 SHA256
- 用户可以验证文件完整性

## 成本对比

| 方案 | 成本 | 信任级别 | 推荐用途 |
|------|------|----------|----------|
| **DigiCert OV 证书** | ~$470/年 | ⭐⭐⭐⭐⭐ | 商业项目 |
| **Sectigo OV 证书** | ~$225/年 | ⭐⭐⭐⭐ | 个人/商业 |
| **SignPath（开源）** | 免费 | ⭐⭐⭐⭐ | 开源项目 |
| **Azure Code Signing** | 按使用付费 | ⭐⭐⭐⭐ | Azure 用户 |
| **自签名证书** | 免费 | ⭐ | 本地测试 |
| **无签名** | 免费 | ⭐ | 不推荐 |

## 时间戳服务器

代码签名必须使用时间戳服务器以确保签名在证书过期后仍然有效：

| 提供商 | URL |
|--------|-----|
| **DigiCert** | http://timestamp.digicert.com |
| **Sectigo** | http://timestamp.sectigo.com |
| **GlobalSign** | http://timestamp.globalsign.com |
| **SSL.com** | http://ts.ssl.com |

## 总结

### 最佳实践

1. ✅ **正式发布**: 购买代码签名证书
2. ✅ **开源项目**: 申请 SignPath 免费签名
3. ⚠️ **测试版本**: 提供文件哈希验证
4. ⚠️ **个人使用**: 使用自签名证书或忽略警告

### 当前状态

- ❌ **HASS.Agent 未配置代码签名**
- ⚠️ **Windows 会显示未知发布者警告**
- ⚠️ **SmartScreen 可能阻止下载**
- ✅ **程序功能完全正常**

### 下一步

1. 评估是否购买代码签名证书
2. 或者申请 SignPath 开源项目免费签名
3. 更新 GitHub Actions 配置以使用证书
4. 测试签名后的安装包

## 相关资源

- [Microsoft Code Signing Documentation](https://docs.microsoft.com/windows/win32/seccrypto/cryptography-tools)
- [DigiCert Code Signing Guide](https://www.digicert.com/kb/code-signing/)
- [SignPath for Open Source](https://signpath.io/for-open-source-projects/)
- [.NET Publisher Identity Guide](https://docs.microsoft.com/dotnet/core/deployment/self-contained-windows-runtime#publish-for-windows)

---

**最后更新**: 2025-12-29
**文档版本**: 1.0
