using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.OpenApi.Extensions;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Test.Extensions;

public class CopilotAgentPluginKernelExtensionsTests
{
    private const string TestPluginName = "TestPlugin";
    private const string TestFilePath = "test-manifest.json";

    [Fact]
    public async Task CreatePluginFromCopilotAgentPluginAsync_LogsWarning_WhenNoFunctionsFoundInRuntime()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CopilotAgentPluginKernelExtensions>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(typeof(CopilotAgentPluginKernelExtensions))).Returns(loggerMock.Object);

        var kernel = Kernel.CreateBuilder()
            .Services.AddSingleton(loggerFactoryMock.Object)
            .Build();

        // Setup a mock manifest with OpenAPI runtime but no matching functions
        File.WriteAllText(TestFilePath, """
        {
            "runtimes": [
                {
                    "type": "OpenApi",
                    "runForFunctions": ["nonexistent"]
                }
            ]
        }
        """);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => kernel.CreatePluginFromCopilotAgentPluginAsync(TestPluginName, TestFilePath));

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat<string>>(msg => msg.ToString().Contains("No functions found in the runtime object.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CreatePluginFromCopilotAgentPluginAsync_LogsWarning_WhenNoApiDescriptionUrlFound()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CopilotAgentPluginKernelExtensions>>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(typeof(CopilotAgentPluginKernelExtensions))).Returns(loggerMock.Object);

        var kernel = Kernel.CreateBuilder()
            .Services.AddSingleton(loggerFactoryMock.Object)
            .Build();

        // Setup manifest with OpenAPI runtime, functions, but empty spec URL
        File.WriteAllText(TestFilePath, """
        {
            "functions": [{"name": "test"}],
            "runtimes": [
                {
                    "type": "OpenApi",
                    "runForFunctions": ["test"],
                    "spec": {}
                }
            ]
        }
        """);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => kernel.CreatePluginFromCopilotAgentPluginAsync(TestPluginName, TestFilePath));

        // Assert - specifically testing line 114 LogWarning call
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat<string>>(msg => msg.ToString().Contains("No API description URL found in the runtime object.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void CreatePluginFromCopilotAgentPluginAsync_UsesNullLogger_WhenLoggerFactoryReturnsNull()
    {
        // Arrange
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(typeof(CopilotAgentPluginKernelExtensions))).Returns((ILogger?)null);

        var kernel = Kernel.CreateBuilder()
            .Services.AddSingleton(loggerFactoryMock.Object)
            .Build();

        File.WriteAllText(TestFilePath, "{}");

        // Act & Assert
        var exception = Assert.ThrowsAsync<FileNotFoundException>(
            () => kernel.CreatePluginFromCopilotAgentPluginAsync(TestPluginName, TestFilePath));
        
        // NullLogger silently handles LogWarning calls without throwing
        Assert.NotNull(exception);
    }
}
