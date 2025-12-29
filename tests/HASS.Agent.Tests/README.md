# HASS.Agent 单元测试

## 概述
此项目包含 HASS.Agent 的单元测试。

## 技术栈
- **xUnit**: 测试框架
- **Moq**: Mock 框架
- **FluentAssertions**: 断言库

## 运行测试

### Visual Studio
1. 打开 Test Explorer (测试 > Test Explorer)
2. 点击 "Run All" 运行所有测试

### 命令行
```bash
# 运行所有测试
dotnet test

# 运行特定测试文件
dotnet test --filter FullyQualifiedName~ComponentStatusTests

# 运行测试并生成代码覆盖率报告
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## 测试结构

```
Tests/
├── Enums/              # 枚举类型测试
├── Extensions/         # 扩展方法测试
├── Functions/          # 工具函数测试
├── Managers/           # 管理器测试
├── Commands/           # 命令测试
└── Sensors/            # 传感器测试
```

## 编写测试

### 测试命名规范
- 测试类: `{ClassName}Tests`
- 测试方法: `{MethodName}_{Scenario}_{ExpectedResult}`

### 示例

```csharp
[Fact]
public void MethodName_ValidInput_ShouldReturnExpectedValue()
{
    // Arrange
    var input = "test";

    // Act
    var result = _sut.MethodName(input);

    // Assert
    result.Should().Be("expected");
}

[Theory]
[InlineData("input1", "output1")]
[InlineData("input2", "output2")]
public void MethodName_VariousInputs_ShouldReturnExpectedResults(string input, string expected)
{
    // Arrange
    // ...

    // Act
    var result = _sut.MethodName(input);

    // Assert
    result.Should().Be(expected);
}
```

## 测试覆盖率目标
- **整体覆盖率**: >= 70%
- **关键模块覆盖率**: >= 85% (Managers, Commands, Sensors)

## CI/CD 集成
测试会在每次 Pull Request 时自动运行，确保代码质量。

## 常见问题

### Q: 测试运行失败怎么办？
A: 检查是否有依赖的服务未启动，或环境配置不正确。

### Q: 如何调试测试？
A: 在 Visual Studio 中右键测试方法，选择 "Debug Test"。

### Q: 如何添加新测试？
A: 在相应的文件夹下创建新的测试类，遵循现有的测试模式。
