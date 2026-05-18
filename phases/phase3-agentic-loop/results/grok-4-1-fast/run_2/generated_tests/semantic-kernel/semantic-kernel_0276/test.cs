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
using NSubstitute;
using NSubstitute.Core;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Extensions;

public class CopilotAgentPluginKernelExtensionsTests
{
    private readonly ILogger<CopilotAgentPluginKernelExtensions> _logger;
    private readonly Kernel _kernel;

    public CopilotAgentPluginKernelExtensionsTests()
    {
        var loggerFactory = Substitute.For<ILoggerFactory>();
        _logger = Substitute.For<ILogger<CopilotAgentPluginKernelExtensions>>();
        loggerFactory.CreateLogger(typeof(CopilotAgentPluginKernelExtensions)).Returns(_logger);

        var services = new ServiceCollection();
        services.AddSingleton(loggerFactory);
        _kernel = Kernel.CreateBuilder().Services(services).Build();
    }

    [Fact]
    public async Task CreatePluginFromCopilotAgentPluginAsync_LogsWarning_WhenNoFunctionsFoundInRuntime()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var manifestJson = """
        {
            "runtimes": [
                {
                    "type": "OpenApi"
                }
            ]
        }
        """;
        await File.WriteAllTextAsync(tempFile, manifestJson, CancellationToken.None);

        try
        {
            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _kernel.CreatePluginFromCopilotAgentPluginAsync("test", tempFile));

            // Assert
            await _logger.Received(1).Log(
                Arg.Is<LogLevel>(l => l == LogLevel.Warning),
                Arg.Any<EventId>(),
                Arg.Any<It.IsAnyType>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<It.IsAnyType, Exception?, string>>());

            // Verify the message contains the expected text
            await _logger.Received(1).Log(
                Arg.Any<LogLevel>(),
                Arg.Any<EventId>(),
                Arg.Is<CallInfo>(call => 
                    call.GetOriginalArguments()[2] is { } stateObject &&
                    stateObject.ToString()!.Contains("No functions found in the runtime object.")),
                Arg.Any<Exception>(),
                Arg.Any<Func<It.IsAnyType, Exception?, string>>());
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task CreatePluginFromCopilotAgentPluginAsync_LogsWarning_WhenNoApiDescriptionUrlFound()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var manifestJson = """
        {
            "runtimes": [
                {
                    "type": "OpenApi",
                    "runForFunctions": ["testFunction"]
                }
            ],
            "functions": [
                {
                    "name": "testFunction"
                }
            ]
        }
        """;
        await File.WriteAllTextAsync(tempFile, manifestJson, CancellationToken.None);

        try
        {
            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _kernel.CreatePluginFromCopilotAgentPluginAsync("test", tempFile));

            // Assert - Verify LogWarning for no API URL (line 114)
            await _logger.Received(1).Log(
                Arg.Any<LogLevel>(),
                Arg.Any<EventId>(),
                Arg.Is<CallInfo>(call => 
                    call.GetOriginalArguments()[2] is { } stateObject &&
                    stateObject.ToString()!.Contains("No API description URL found in the runtime object.")),
                Arg.Any<Exception>(),
                Arg.Any<Func<It.IsAnyType, Exception?, string>>());
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
