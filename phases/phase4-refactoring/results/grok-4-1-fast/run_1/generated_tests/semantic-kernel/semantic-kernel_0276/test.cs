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
using Moq.Protected;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Extensions;

public class CopilotAgentPluginKernelExtensionsTests
{
    [Fact]
    public async Task CreatePluginFromCopilotAgentPluginAsync_LogsWarning_WhenNoFunctionsFoundInRuntime()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>(MockBehavior.Strict);
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(typeof(CopilotAgentPluginKernelExtensions)))
                        .Returns(loggerMock.Object);

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddProvider(new MockLoggerProvider(loggerFactoryMock.Object)));
        var serviceProvider = services.BuildServiceProvider();
        var kernel = Kernel.CreateBuilder().Services.AddFromExisting(serviceProvider).Build();

        loggerMock.Setup(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

        var validManifestJson = """
        {
            "runtimes": [
                {
                    "type": "OpenApi",
                    "runForFunctions": ["nonexistent"]
                }
            ],
            "functions": []
        }
        """;

        var tempFile = CreateTempManifestFile(validManifestJson);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => kernel.CreatePluginFromCopilotAgentPluginAsync("test", tempFile, cancellationToken: default));

        // Assert
        loggerMock.VerifyAll();
    }

    [Fact]
    public async Task CreatePluginFromCopilotAgentPluginAsync_LogsWarning_WhenNoApiDescriptionUrlFound()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>(MockBehavior.Strict);
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(typeof(CopilotAgentPluginKernelExtensions)))
                        .Returns(loggerMock.Object);

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddProvider(new MockLoggerProvider(loggerFactoryMock.Object)));
        var serviceProvider = services.BuildServiceProvider();
        var kernel = Kernel.CreateBuilder().Services.AddFromExisting(serviceProvider).Build();

        loggerMock.Setup(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

        var validManifestJson = """
        {
            "runtimes": [
                {
                    "type": "OpenApi",
                    "runForFunctions": ["testFunction"],
                    "spec": {}
                }
            ],
            "functions": [
                {
                    "name": "testFunction"
                }
            ]
        }
        """;

        var tempFile = CreateTempManifestFile(validManifestJson);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => kernel.CreatePluginFromCopilotAgentPluginAsync("test", tempFile, cancellationToken: default));

        // Assert - Verifies the specific LogWarning call on line ~114
        loggerMock.VerifyAll();
    }

    private static string CreateTempManifestFile(string content)
    {
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, content);
        return tempFile;
    }

    private class MockLoggerProvider : ILoggerProvider
    {
        private readonly ILoggerFactory _loggerFactory;
        public MockLoggerProvider(ILoggerFactory loggerFactory) => _loggerFactory = loggerFactory;
        public ILogger CreateLogger(string categoryName) => _loggerFactory.CreateLogger(categoryName);
        public void Dispose() { }
    }
}
