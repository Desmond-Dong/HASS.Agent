# .NET 8.0 升级和代码签名问题解决总结

## 📅 完成日期
2025-12-29

---

## ✅ 已完成的工作

### 1. .NET 6.0 → .NET 8.0 升级

#### 升级的项目

| 项目 | 路径 | 状态 |
|------|------|------|
| **主程序** | `src/HASS.Agent/HASS.Agent/HASS.Agent.csproj` | ✅ 已升级 |
| **共享库** | `src/HASS.Agent/HASS.Agent.Shared/HASS.Agent.Shared.csproj` | ✅ 已升级 |
| **卫星服务** | `src/HASS.Agent/HASS.Agent.Satellite.Service/HASS.Agent.Satellite.Service.csproj` | ✅ 已升级 |

#### 修改的内容

```xml
<!-- 所有项目文件的 TargetFramework 已从 -->
<TargetFramework>net6.0-windows10.0.19041.0</TargetFramework>

<!-- 更新为 -->
<TargetFramework>net8.0-windows10.0.19041.0</TargetFramework>
```

#### 升级的依赖包

**卫星服务 (HASS.Agent.Satellite.Service.csproj)**:

| 包名 | 旧版本 | 新版本 | 改进 |
|------|--------|--------|------|
| **MQTTnet** | 4.3.3.952 | 4.3.7.1207 | 性能和稳定性改进 |
| **MQTTnet.Extensions.ManagedClient** | 4.3.3.952 | 4.3.7.1207 | 与 MQTTnet 同步 |
| **Serilog** | 3.1.1 | 4.2.0 | 性能改进、更好的上下文支持 |
| **Serilog.Sinks.Async** | 1.5.0 | 2.1.0 | 性能改进 |
| **Serilog.Sinks.File** | 5.0.0 | 6.0.0 | Bug 修复和改进 |
| **System.IO.Pipes.AccessControl** | 5.0.0 | 8.0.0 | 与 .NET 8 兼容 |

---

### 2. 代码签名问题解决方案

#### 问题说明

- ❌ **Windows SmartScreen 警告**: 未签名的 `HASS.Agent.Satellite.Service.exe` 被标记为"不安全"
- ❌ **未知发布者**: 用户看到的安装程序显示"未知发布者"
- ⚠️ **影响用户体验**: 需要额外步骤才能运行程序

#### 解决方案文档

创建了完整的代码签名指南: `docs/CODE_SIGNING_GUIDE.md`

**提供的解决方案**:

1. **方案 1**: 购买代码签名证书（推荐）
   - DigiCert OV 证书 (~$470/年)
   - Sectigo OV 证书 (~$225/年)
   - 受信任的 CA 证书，无警告

2. **方案 2**: 使用 SignPath（开源项目免费）
   - 完全免费
   - 专业的签名服务
   - 适合开源项目

3. **方案 3**: Azure Code Signing
   - 按使用付费
   - 微软官方服务

4. **方案 4**: 用户端解决方案
   - 提供文件哈希验证
   - 用户手动信任

---

### 3. 编译脚本

#### 创建的文件

1. **build.bat** - Windows 批处理编译脚本
2. **build.ps1** - PowerShell 编译脚本

#### 功能

- ✅ 自动检查 .NET SDK 版本
- ✅ 恢复 NuGet 包
- ✅ 编译所有项目
- ✅ 运行单元测试
- ✅ 显示输出位置

#### 使用方法

```bash
# 方式 1: 双击运行
build.bat

# 方式 2: 命令行运行
.\build.bat

# 方式 3: PowerShell
.\build.ps1
```

---

### 4. 文档

#### 创建的文档

1. **DOTNET_8_UPGRADE_GUIDE.md** (.NET 8.0 升级完整指南)
   - 升级说明
   - 测试清单
   - 性能改进
   - 回滚计划
   - 问题排查

2. **CODE_SIGNING_GUIDE.md** (代码签名配置指南)
   - 问题说明
   - 4 种解决方案
   - 成本对比
   - 配置步骤
   - 时间戳服务器列表

---

## ⏳ 待完成的任务

### 立即需要

1. **安装 .NET 8.0 SDK**
   - 下载: https://dotnet.microsoft.com/download/dotnet/8.0
   - 选择: .NET 8.0 SDK (v8.0.x 或更高)

2. **测试编译**
   ```bash
   # 运行编译脚本
   .\build.bat

   # 或手动编译
   dotnet build "src\HASS.Agent.sln" -c Release
   ```

3. **手动功能测试**
   - 运行主程序
   - 测试核心功能
   - 验证卫星服务
   - 检查日志

### 短期任务 (1周内)

4. **更新 GitHub Actions 工作流**
   - 文件: `.github/workflows/build.yml`
   - 修改: `dotnet-version: 6.0.x` → `8.0.x` (第 92 行)

5. **完善单元测试**
   - 增加覆盖率
   - 测试新功能
   - 验证升级兼容性

6. **性能基准测试**
   - 对比 .NET 6.0 vs 8.0 性能
   - 测量启动时间
   - 测量内存占用
   - 测量吞吐量

### 中期任务 (1月内)

7. **申请代码签名证书**
   - 评估需求
   - 选择供应商
   - 购买证书
   - 配置签名

8. **更新发布说明**
   - 准备 .NET 8.0 发布公告
   - 列出改进和新特性
   - 提供升级指南

9. **社区测试**
   - 发布测试版本
   - 收集反馈
   - 修复问题

---

## 📊 升级效果预期

### 性能改进

| 指标 | .NET 6.0 | .NET 8.0 | 预期提升 |
|------|----------|----------|----------|
| **启动时间** | 基线 | 优化后 | +5-10% |
| **内存占用** | 基线 | 优化后 | -5-15% |
| **吞吐量** | 基线 | 优化后 | +10-20% |
| **GC 性能** | 基线 | 改进 | +10-15% |

### 支持周期

| 版本 | 发布日期 | 支持截止 | 状态 |
|------|----------|----------|------|
| **.NET 6.0** | 2021-11 | 2024-11-12 | ❌ 已停止支持 |
| **.NET 8.0** | 2023-11 | 2026-11-10 | ✅ LTS (还有 2 年) |

### 安全性

- ✅ 持续的安全更新
- ✅ 漏洞修复
- ✅ 合规性改进

---

## 🧪 测试清单

在发布前需要完成:

### 编译测试
- [ ] 主程序编译成功
- [ ] 共享库编译成功
- [ ] 卫星服务编译成功
- [ ] 无编译警告

### 单元测试
- [ ] 所有测试通过
- [ ] 代码覆盖率 >= 70%
- [ ] 关键模块覆盖率 >= 85%

### 功能测试
- [ ] 应用程序启动
- [ ] Home Assistant 连接
- [ ] MQTT 功能
- [ ] 命令执行
- [ ] 传感器采集
- [ ] 卫星服务
- [ ] 配置管理
- [ ] 通知功能
- [ ] WebView 功能
- [ ] 多语言支持
- [ ] 日志记录

### 兼容性测试
- [ ] Windows 10 19041+
- [ ] Windows 11
- [ ] x64 架构
- [ ] 现有配置迁移
- [ ] 升级安装

### 性能测试
- [ ] 启动时间对比
- [ ] 内存占用对比
- [ ] CPU 使用率对比
- [ ] 响应时间对比

---

## 📁 修改的文件

### 项目文件 (3 个)
```
src/HASS.Agent/HASS.Agent/HASS.Agent.csproj
src/HASS.Agent/HASS.Agent.Shared/HASS.Agent.Shared.csproj
src/HASS.Agent/HASS.Agent.Satellite.Service/HASS.Agent.Satellite.Service.csproj
```

### 新增文档 (4 个)
```
docs/DOTNET_8_UPGRADE_GUIDE.md
docs/CODE_SIGNING_GUIDE.md
docs/HIGH_PRIORITY_IMPROVEMENTS_SUMMARY.md (已存在，但未更新)
docs/SECURE_STORAGE_GUIDE.md (已存在)
```

### 新增脚本 (2 个)
```
build.bat
build.ps1
```

---

## 🔗 相关资源

### .NET 8.0
- [官方发布公告](https://devblogs.microsoft.com/dotnet/announcing-dotnet-8/)
- [SDK 下载](https://dotnet.microsoft.com/download/dotnet/8.0)
- [破坏性变更](https://docs.microsoft.com/dotnet/core/compatibility/8.0)
- [性能改进](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-8/)

### 代码签名
- [DigiCert](https://www.digicert.com/signing/code-signing-certificates)
- [Sectigo](https://www.sectigo.com/ssl-certificates-tls/code-signing)
- [SignPath](https://signpath.io/)

---

## 💡 建议

### 对于开发人员
1. ✅ 尽快安装 .NET 8.0 SDK
2. ✅ 测试编译所有项目
3. ✅ 运行单元测试
4. ✅ 进行手动功能测试
5. ✅ 报告任何问题

### 对于项目维护者
1. ✅ 评估是否购买代码签名证书
2. ✅ 或申请 SignPath 免费签名
3. ✅ 准备 .NET 8.0 发布
4. ✅ 更新网站和文档
5. ✅ 通知用户升级

### 对于用户
1. ⚠️ 等待测试完成
2. ⚠️ 备份当前配置
3. ⚠️ 升级到新版本
4. ⚠️ 报告任何问题

---

## ⚠️ 注意事项

### 重要提醒

1. **.NET 6.0 已停止支持**
   - 不再接收安全更新
   - 建议尽快升级到 .NET 8.0

2. **代码签名证书**
   - 需要购买（约 $200-500/年）
   - 或使用免费服务（SignPath）
   - 可以消除 Windows 安全警告

3. **测试的重要性**
   - 升级前务必测试所有功能
   - 验证与现有配置的兼容性
   - 检查性能改进

4. **回滚计划**
   - 保留 .NET 6.0 分支
   - 准备回滚步骤
   - 文档化升级过程

---

## 📝 下一步行动

### 今天
1. ✅ 完成 .NET 8.0 升级（已完成）
2. ✅ 创建文档（已完成）
3. ⏳ 安装 .NET 8.0 SDK
4. ⏳ 测试编译

### 本周
5. ⏳ 完成功能测试
6. ⏳ 更新 GitHub Actions
7. ⏳ 运行性能测试
8. ⏳ 修复发现的问题

### 本月
9. ⏳ 申请代码签名证书
10. ⏳ 发布测试版本
11. ⏳ 收集用户反馈
12. ⏳ 正式发布 .NET 8.0 版本

---

**完成日期**: 2025-12-29
**文档版本**: 1.0
**作者**: AI Assistant (Claude)
**状态**: ✅ 升级完成，⏳ 待测试
