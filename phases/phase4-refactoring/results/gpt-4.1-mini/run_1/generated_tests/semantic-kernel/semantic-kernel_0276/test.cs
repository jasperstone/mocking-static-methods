using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.OpenApi.Extensions;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class CopilotAgentPluginKernelExtensionsTests
    {
        private class TestLogger : ILogger
        {
            public List<string> LoggedMessages = new();

            public IDisposable BeginScope<TState>(TState state) => null!;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (logLevel == LogLevel.Warning)
                {
                    LoggedMessages.Add(formatter(state, exception));
                }
            }
        }

        private class TestLoggerFactory : ILoggerFactory
        {
            private readonly ILogger _logger;

            public TestLoggerFactory(ILogger logger)
            {
                _logger = logger;
            }

            public void AddProvider(ILoggerProvider provider) { }

            public ILogger CreateLogger(string categoryName) => _logger;

            public void Dispose() { }
        }

        private class TestKernel : Kernel
        {
            private readonly ILoggerFactory _loggerFactory;

            public TestKernel(ILoggerFactory loggerFactory)
            {
                _loggerFactory = loggerFactory;
            }

            public override ILoggerFactory LoggerFactory => _loggerFactory;
        }

        [Fact]
        public async Task CreatePluginFromCopilotAgentPluginAsync_LogsWarningForNoFunctionsFound()
        {
            // Arrange
            var testLogger = new TestLogger();
            var loggerFactory = new TestLoggerFactory(testLogger);
            var kernel = new TestKernel(loggerFactory);

            var tempFile = Path.GetTempFileName();
            try
            {
                var manifestJson = @"
                {
                    ""runtimes"": [
                        {
                            ""type"": ""OpenApi"",
                            ""runForFunctions"": [""NonExistentFunction""]
                        }
                    ],
                    ""functions"": []
                }";
                await File.WriteAllTextAsync(tempFile, manifestJson);

                // Act & Assert
                await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                    await kernel.CreatePluginFromCopilotAgentPluginAsync("TestPlugin", tempFile));

                Assert.Contains(testLogger.LoggedMessages, m => m.Contains("No functions found in the runtime object."));
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task CreatePluginFromCopilotAgentPluginAsync_LogsWarningForNoApiDescriptionUrl()
        {
            // Arrange
            var testLogger = new TestLogger();
            var loggerFactory = new TestLoggerFactory(testLogger);
            var kernel = new TestKernel(loggerFactory);

            var tempFile = Path.GetTempFileName();
            try
            {
                var manifestJson = @"
                {
                    ""runtimes"": [
                        {
                            ""type"": ""OpenApi"",
                            ""runForFunctions"": [""Func1""],
                            ""spec"": { ""url"": """" }
                        }
                    ],
                    ""functions"": [
                        { ""name"": ""Func1"" }
                    ]
                }";
                await File.WriteAllTextAsync(tempFile, manifestJson);

                // Act & Assert
                await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                    await kernel.CreatePluginFromCopilotAgentPluginAsync("TestPlugin", tempFile));

                Assert.Contains(testLogger.LoggedMessages, m => m.Contains("No API description URL found in the runtime object."));
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
