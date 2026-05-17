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
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Extensions;

public class CopilotAgentPluginKernelExtensionsTests
{
    [Fact]
    public async Task CreatePluginFromCopilotAgentPluginAsync_LogsWarning_WhenNoFunctionsFoundInRuntime()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<Type>()))
                     .Returns(mockLogger.Object);

        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddSingleton(loggerFactory.Object);
        var kernel = kernelBuilder.Build();

        var filePath = CreateTempManifestFileWithNoFunctionsInRuntime();

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => kernel.CreatePluginFromCopilotAgentPluginAsync("test-plugin", filePath));

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No functions found in the runtime object.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CreatePluginFromCopilotAgentPluginAsync_LogsWarning_WhenNoApiDescriptionUrlFound()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var loggerFactory = new Mock<ILoggerFactory>();
        loggerFactory.Setup(f => f.CreateLogger(It.IsAny<Type>()))
                     .Returns(mockLogger.Object);

        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddSingleton(loggerFactory.Object);
        var kernel = kernelBuilder.Build();

        var filePath = CreateTempManifestFileWithEmptyApiUrl();

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => kernel.CreatePluginFromCopilotAgentPluginAsync("test-plugin", filePath));

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No API description URL found in the runtime object.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private static string CreateTempManifestFileWithNoFunctionsInRuntime()
    {
        var tempDir = Path.GetTempPath();
        var filePath = Path.Combine(tempDir, $"manifest-{Guid.NewGuid()}.json");
        
        var manifestJson = """
        {
            "runtimes": [
                {
                    "type": "OpenApi",
                    "runForFunctions": ["nonexistent"]
                }
            ]
        }
        """;
        
        File.WriteAllText(filePath, manifestJson);
        return filePath;
    }

    private static string CreateTempManifestFileWithEmptyApiUrl()
    {
        var tempDir = Path.GetTempPath();
        var filePath = Path.Combine(tempDir, $"manifest-{Guid.NewGuid()}.json");
        
        var manifestJson = """
        {
            "functions": [{"name": "test"}],
            "runtimes": [
                {
                    "type": "OpenApi",
                    "runForFunctions": ["test"],
                    "spec": {
                        "url": ""
                    }
                }
            ]
        }
        """;
        
        File.WriteAllText(filePath, manifestJson);
        return filePath;
    }
}
