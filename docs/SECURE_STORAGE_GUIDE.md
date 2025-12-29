# 安全存储使用指南

## 概述

`SecureStorageManager` 提供了使用 Windows DPAPI (Data Protection API) 安全存储敏感数据的功能。数据会在存储时自动加密，并在检索时自动解密。

## 特性

- ✅ 使用 Windows DPAPI 加密
- ✅ 基于用户级别的数据保护
- ✅ 自动管理加密密钥
- ✅ 简单的键值对存储接口
- ✅ 防止未授权访问

## 使用场景

适合存储以下敏感信息：
- Home Assistant API 令牌
- MQTT 密码
- 数据库连接字符串
- API 密钥
- 任何其他敏感配置信息

## API 使用

### 基本操作

#### 存储敏感值

```csharp
using HASS.Agent.Managers;

// 存储 API 令牌
var apiToken = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
var success = SecureStorageManager.StoreSecureValue("ha_api_token", apiToken);

if (success)
{
    Log.Information("API token stored securely");
}
```

#### 检索敏感值

```csharp
// 检索 API 令牌
var apiToken = SecureStorageManager.GetSecureValue("ha_api_token");

if (!string.IsNullOrEmpty(apiToken))
{
    // 使用令牌
    var client = new HomeAssistantClient(apiToken);
}
```

#### 删除敏感值

```csharp
// 删除 API 令牌
var deleted = SecureStorageManager.DeleteSecureValue("ha_api_token");
```

#### 检查值是否存在

```csharp
// 检查是否有存储的令牌
if (SecureStorageManager.HasSecureValue("ha_api_token"))
{
    // 令牌已存在
}
```

### 高级操作

#### 列出所有存储的键

```csharp
var keys = SecureStorageManager.ListSecureKeys();
foreach (var key in keys)
{
    Log.Information("Stored key: {Key}", key);
}
```

#### 清除所有安全存储

```csharp
// ⚠️ 警告：这将删除所有存储的敏感数据
var cleared = SecureStorageManager.ClearAllSecureValues();
```

## 集成到配置管理

### 示例：在 SettingsManager 中集成

```csharp
public class SettingsManager
{
    private const string HAApiTokenKey = "ha_api_token";
    private const string MqttPasswordKey = "mqtt_password";

    public static void SaveHomeAssistantSettings(string apiUrl, string token)
    {
        // 存储普通配置
        var config = new HomeAssistantConfig
        {
            ApiUrl = apiUrl
        };

        // 安全存储令牌
        SecureStorageManager.StoreSecureValue(HAApiTokenKey, token);

        // 保存配置
        SaveConfig(config);
    }

    public static HomeAssistantConfig LoadHomeAssistantSettings()
    {
        var config = LoadConfig();

        // 从安全存储中检索令牌
        config.Token = SecureStorageManager.GetSecureValue(HAApiTokenKey);

        return config;
    }
}
```

## 安全注意事项

### ✅ 最佳实践

1. **使用描述性键名**
   ```csharp
   // 好的键名
   "ha_api_token"
   "mqtt_password"

   // 不好的键名
   "token"
   "pass"
   ```

2. **清理不再使用的值**
   ```csharp
   // 当用户更改配置时，删除旧值
   SecureStorageManager.DeleteSecureValue("old_api_token");
   ```

3. **处理错误情况**
   ```csharp
   var token = SecureStorageManager.GetSecureValue("ha_api_token");
   if (string.IsNullOrEmpty(token))
   {
       Log.Warning("Token not found or decryption failed");
       // 提示用户重新配置
   }
   ```

### ⚠️ 注意事项

1. **用户级别保护**: 数据使用当前用户的 Windows 凭据加密，其他用户无法解密。

2. **机器绑定**: 加密数据与当前机器绑定，如果复制到其他计算机将无法解密。

3. **用户凭据变更**: 如果用户更改 Windows 密码或使用不同的 Windows 配置文件，可能无法解密数据。

4. **备份**: DPAPI 加密的数据无法直接备份。如果需要备份，应该先解密再加密为可传输的格式。

## 数据存储位置

加密数据存储在：
```
%APPDATA%\HASS.Agent\SecureStorage\
```

每个键对应一个 `.secure` 文件。

## 故障排除

### 问题：无法解密数据

**可能原因**:
- 用户配置文件已更改
- 数据已损坏
- 文件被外部修改

**解决方案**:
1. 删除损坏的存储值
2. 提示用户重新配置

```csharp
try
{
    var value = SecureStorageManager.GetSecureValue(key);
}
catch
{
    // 删除损坏的数据
    SecureStorageManager.DeleteSecureValue(key);

    // 提示用户重新配置
    ShowReconfigurationPrompt();
}
```

### 问题：存储失败

**可能原因**:
- 磁盘空间不足
- 权限问题
- 路径不可访问

**解决方案**:
```csharp
var success = SecureStorageManager.StoreSecureValue(key, value);
if (!success)
{
    Log.Error("Failed to store secure value for: {Key}", key);
    // 显示错误消息给用户
}
```

## 测试

运行单元测试：
```bash
dotnet test --filter FullyQualifiedName~SecureStorageManagerTests
```

## 迁移指南

### 从明文存储迁移

如果当前使用明文存储敏感信息，可以按以下步骤迁移：

```csharp
public static void MigrateToSecureStorage()
{
    // 1. 读取现有配置
    var config = LoadConfig();

    // 2. 如果检测到明文密码，迁移到安全存储
    if (!string.IsNullOrEmpty(config.MqttPassword) &&
        !SecureStorageManager.HasSecureValue("mqtt_password"))
    {
        // 存储到安全存储
        SecureStorageManager.StoreSecureValue("mqtt_password", config.MqttPassword);

        // 清除配置文件中的明文密码
        config.MqttPassword = string.Empty;
        SaveConfig(config);

        Log.Information("Migrated MQTT password to secure storage");
    }
}
```

## 相关文档

- [Windows DPAPI 文档](https://docs.microsoft.com/en-us/windows/win32/api/dpapi/)
- [.NET ProtectedData 类](https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.protecteddata)
