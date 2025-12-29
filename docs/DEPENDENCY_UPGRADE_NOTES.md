# 依赖包升级说明

## 升级日期
2025-12-29

## 升级的依赖包

### HASS.Agent 主项目

| 包名 | 旧版本 | 新版本 | 变更说明 |
|------|--------|--------|----------|
| Microsoft.WindowsAppSDK | 1.4.240211001 | 1.6.250114007 | 性能改进和bug修复 |
| MQTTnet | 4.3.3.952 | 4.3.7.1207 | 性能改进和稳定性提升 |
| MQTTnet.Extensions.ManagedClient | 4.3.3.952 | 4.3.7.1207 | 同上 |
| Octokit | 10.0.0 | 14.0.0 | API更新和性能改进 |
| Serilog | 3.1.1 | 4.2.0 | 重大版本更新,包含性能改进 |
| Serilog.Sinks.Async | 1.5.0 | 2.1.0 | 性能改进 |
| Serilog.Sinks.File | 5.0.0 | 6.0.0 | bug修复和改进 |
| Syncfusion.Core.WinForms | 20.3.0.50 | 27.2.2 | 重大版本升级,新功能和改进 |
| Syncfusion.Licensing | 20.3.0.50 | 27.2.2 | 同上 |
| Syncfusion.Shared.Base | 20.3.0.50 | 27.2.2 | 同上 |
| Syncfusion.Tools.Windows | 20.3.0.50 | 27.2.2 | 同上 |
| System.Text.Json | (新增) | 9.0.0 | 新增,为未来迁移做准备 |

### HASS.Agent.Shared 项目

| 包名 | 旧版本 | 新版本 | 变更说明 |
|------|--------|--------|----------|
| MQTTnet | 4.3.3.952 | 4.3.7.1207 | 性能改进和稳定性提升 |
| Serilog | 3.1.1 | 4.2.0 | 重大版本更新,包含性能改进 |
| System.Text.Json | (新增) | 9.0.0 | 新增,为未来迁移做准备 |

## 重要变更说明

### 1. Syncfusion v20 → v27
这是主要的破坏性变更。Syncfusion v27 包含:
- 改进的高 DPI 支持
- 更好的性能
- 新的控件和功能
- API 变更

**需要注意**:
- 许可证密钥需要更新
- 部分API可能有破坏性变更
- 建议查看 [Syncfusion v27 发布说明](https://help.syncfusion.com/common/essential-studio/release-notes/v27.2.2)

### 2. Serilog v3 → v4
Serilog 4.0 是一个重大版本更新:
- 性能改进
- 更好的日志上下文支持
- 部分API变更

**需要注意**:
- 检查自定义 Sink 的兼容性
- 验证日志输出格式

### 3. 添加 System.Text.Json
添加此包是为未来从 Newtonsoft.Json 迁移做准备。目前两者共存:
- 新代码优先使用 System.Text.Json
- 旧代码继续使用 Newtonsoft.Json
- 逐步迁移以减少破坏性变更

## 测试清单

升级后需要测试的功能:

- [ ] 应用程序启动
- [ ] Home Assistant 连接
- [ ] MQTT 连接和消息传递
- [ ] 传感器数据上报
- [ ] 命令执行
- [ ] 通知显示
- [ ] 快速操作
- [ ] WebView 显示
- [ ] 配置保存和加载
- [ ] 卫星服务通信
- [ ] 多语言支持
- [ ] 高 DPI 显示

## 回滚计划

如果升级后出现严重问题:
1. 在 `.csproj` 文件中回退包版本
2. 运行 `dotnet restore`
3. 重新编译和测试

## 参考链接

- [Syncfusion 发行说明](https://help.syncfusion.com/common/essential-studio/release-notes)
- [MQTTnet 发行说明](https://github.com/chkr1011/MQTTnet/releases)
- [Serilog 发行说明](https://github.com/serilog/serilog/releases)
- [Octokit 发行说明](https://github.com/octokit/octokit.net/releases)
