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
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Extensions;

public class CopilotAgentPluginKernelExtensionsTests
{
    private class CapturingLoggerProvider : ILoggerProvider
    {
        public List<string> Warnings { get; } = new();

        public ILogger CreateLogger(string categoryName) => new CapturingLogger(this);

        public void Dispose() { }

        private class CapturingLogger : ILogger
        {
            private readonly CapturingLoggerProvider _provider;

            public CapturingLogger(CapturingLoggerProvider provider)
            {
                _provider = provider;
            }

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null!;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (logLevel == LogLevel.Warning)
                {
                    _provider.Warnings.Add(formatter(state, exception));
                }
            }
        }
    }

    [Fact]
    public async Task CreatePluginFromCopilotAgentPluginAsync_LogsWarning_WhenNoFunctionsInRuntime()
    {
        // Arrange
        var loggerProvider = new CapturingLoggerProvider();
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerProvider>(loggerProvider);
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.Add(services);
        var kernel = kernelBuilder.Build();

        var manifestJson = """
        {
            "schemaVersion": "0.2.0",
            "name": "test-plugin",
            "version": "1.0.0",
            "runtimes": [
                {
                    "type": "OpenApi",
                    "runFor": ["nonexistent"]
                }
            ]
        }
        """;

        var tempFile = CreateTempManifestFile(manifestJson);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => kernel.CreatePluginFromCopilotAgentPluginAsync("test", tempFile, cancellationToken: CancellationToken.None));

        // Assert
        Assert.Contains("No functions found in the runtime object.", loggerProvider.Warnings);
    }

    [Fact]
    public async Task CreatePluginFromCopilotAgentPluginAsync_LogsWarning_WhenNoApiDescriptionUrl()
    {
        // Arrange
        var loggerProvider = new CapturingLoggerProvider();
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerProvider>(loggerProvider);
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.Add(services);
        var kernel = kernelBuilder.Build();

        var manifestJson = """
        {
            "schemaVersion": "0.2.0",
            "name": "test-plugin",
            "version": "1.0.0",
            "functions": [{"name": "test"}],
            "runtimes": [
                {
                    "type": "OpenApi",
                    "runFor": ["test"],
                    "spec": {}
                }
            ]
        }
        """;

        var tempFile = CreateTempManifestFile(manifestJson);

        // Act
        await kernel.CreatePluginFromCopilotAgentPluginAsync("test", tempFile, cancellationToken: CancellationToken.None);

        // Assert - specifically tests the LogWarning call on line 114
        Assert.Contains("No API description URL found in the runtime object.", loggerProvider.Warnings);
    }

    private static string CreateTempManifestFile(string content)
    {
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, content);
        return tempFile;
    }
}
