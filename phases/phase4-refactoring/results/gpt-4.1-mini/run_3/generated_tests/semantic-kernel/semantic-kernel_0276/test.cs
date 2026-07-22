using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.OpenApi.Extensions;
using Moq;
using Xunit;

public class CopilotAgentPluginKernelExtensionsTests
{
    [Fact]
    public async Task CreatePluginFromCopilotAgentPluginAsync_LogsWarningWhenNoFunctionsFound()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);

        var services = new ServiceCollection().BuildServiceProvider();

        // Create a real Kernel instance and replace its LoggerFactory with the mock
        var kernel = Kernel.Builder.Build();
        kernel.LoggerFactory = loggerFactoryMock.Object;

        string pluginName = "TestPlugin";
        string filePath = "testfile.json";

        // Create a minimal manifest file that has an OpenApi runtime but no functions matching runtime
        string manifestJson = @"
        {
            ""runtimes"": [
                {
                    ""type"": ""OpenApi"",
                    ""runForFunctions"": [""func1""],
                    ""spec"": { ""url"": ""http://example.com/api"" }
                }
            ],
            ""functions"": []
        }";

        File.WriteAllText(filePath, manifestJson);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await CopilotAgentPluginKernelExtensions.CreatePluginFromCopilotAgentPluginAsync(
                kernel, pluginName, filePath));

        // Verify the logger was called with the expected warning message
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No functions found in the runtime object.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public async Task CreatePluginFromCopilotAgentPluginAsync_LogsWarningWhenNoApiDescriptionUrl()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);

        var services = new ServiceCollection().BuildServiceProvider();

        // Create a real Kernel instance and replace its LoggerFactory with the mock
        var kernel = Kernel.Builder.Build();
        kernel.LoggerFactory = loggerFactoryMock.Object;

        string pluginName = "TestPlugin";
        string filePath = "testfile.json";

        // Create a minimal manifest file that has an OpenApi runtime with empty URL and functions matching runtime
        string manifestJson = @"
        {
            ""runtimes"": [
                {
                    ""type"": ""OpenApi"",
                    ""runForFunctions"": [""func1""],
                    ""spec"": { ""url"": """" }
                }
            ],
            ""functions"": [
                { ""name"": ""func1"" }
            ]
        }";

        File.WriteAllText(filePath, manifestJson);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await CopilotAgentPluginKernelExtensions.CreatePluginFromCopilotAgentPluginAsync(
                kernel, pluginName, filePath));

        // Verify the logger was called with the expected warning message
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No API description URL found in the runtime object.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);

        // Cleanup
        File.Delete(filePath);
    }
}
