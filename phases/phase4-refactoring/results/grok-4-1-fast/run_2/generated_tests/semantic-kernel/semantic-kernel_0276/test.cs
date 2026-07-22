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
using Microsoft.Plugins.Manifest;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Extensions;

public class CopilotAgentPluginKernelExtensionsTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<ILoggerFactory> _loggerFactoryMock;
    private readonly Kernel _kernel;

    public CopilotAgentPluginKernelExtensionsTests()
    {
        _loggerMock = new Mock<ILogger>();
        _loggerFactoryMock = new Mock<ILoggerFactory>();
        _loggerFactoryMock.Setup(f => f.CreateLogger(typeof(CopilotAgentPluginKernelExtensions)))
                         .Returns(_loggerMock.Object);

        var services = new ServiceCollection();
        services.AddSingleton(_loggerFactoryMock.Object);
        _kernel = Kernel.CreateBuilder().Services(services).Build();
    }

    [Fact]
    public async Task CreatePluginFromCopilotAgentPluginAsync_LogsWarning_WhenNoFunctionsInRuntime()
    {
        // Arrange
        var manifestPath = Path.Combine(Path.GetTempPath(), "manifest-no-functions.json");
        await File.WriteAllTextAsync(manifestPath, """
            {
              "Runtimes": [
                {
                  "Type": "OpenApi",
                  "RunForFunctions": ["nonexistent"]
                }
              ]
            }
            """);

        try
        {
            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _kernel.CreatePluginFromCopilotAgentPluginAsync("test", manifestPath));

            // Assert logger call
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No functions found in the runtime object.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        finally
        {
            if (File.Exists(manifestPath))
            {
                File.Delete(manifestPath);
            }
        }
    }

    [Fact]
    public async Task CreatePluginFromCopilotAgentPluginAsync_LogsWarning_WhenNoApiDescriptionUrl()
    {
        // Arrange
        var manifestPath = Path.Combine(Path.GetTempPath(), "manifest-no-url.json");
        await File.WriteAllTextAsync(manifestPath, """
            {
              "Runtimes": [
                {
                  "Type": "OpenApi",
                  "RunForFunctions": ["test"],
                  "Spec": {}
                }
              ],
              "Functions": [
                {
                  "Name": "test"
                }
              ]
            }
            """);

        try
        {
            // Act - doesn't throw, just logs and continues (will fail later on server URL)
            await Record.ExceptionAsync(() => _kernel.CreatePluginFromCopilotAgentPluginAsync("test", manifestPath));

            // Assert logger call for line 114
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No API description URL found in the runtime object.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        finally
        {
            if (File.Exists(manifestPath))
            {
                File.Delete(manifestPath);
            }
        }
    }

    [Fact]
    public async Task CreatePluginFromCopilotAgentPluginAsync_HandlesNullLoggerGracefully()
    {
        // Arrange
        _loggerFactoryMock.Setup(f => f.CreateLogger(typeof(CopilotAgentPluginKernelExtensions)))
                         .Returns((ILogger?)null);

        var services = new ServiceCollection();
        services.AddSingleton(_loggerFactoryMock.Object);
        var kernel = Kernel.CreateBuilder().Services(services).Build();

        var tempFile = Path.GetTempFileName();
        try
        {
            // Act & Assert - should not throw NullReferenceException
            var exception = await Record.ExceptionAsync(
                () => kernel.CreatePluginFromCopilotAgentPluginAsync("test", tempFile));
            
            Assert.IsType<FileNotFoundException>(exception);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}
