using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class CopilotAgentPluginKernelExtensionsTests
    {
        private class TestLogger : ILogger
        {
            public List<string> Warnings { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => null!;

            public bool IsEnabled(LogLevel logLevel) => logLevel == LogLevel.Warning;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (logLevel == LogLevel.Warning)
                {
                    Warnings.Add(formatter(state, exception));
                }
            }
        }

        private class TestLoggerProvider : ILoggerProvider
        {
            private readonly TestLogger _logger = new();

            public ILogger CreateLogger(string categoryName) => _logger;

            public void Dispose() { }

            public List<string> Warnings => _logger.Warnings;
        }

        [Fact]
        public async Task CreatePluginFromCopilotAgentPluginAsync_LogsWarningWhenNoApiDescriptionUrl()
        {
            // Arrange
            var loggerProvider = new TestLoggerProvider();
            var loggerFactory = new LoggerFactory(new[] { loggerProvider });

            // Create a service provider with the logger factory
            var services = new ServiceCollection()
                .AddSingleton<ILoggerFactory>(loggerFactory)
                .BuildServiceProvider();

            // Create a Kernel instance with the service provider
            var kernel = new Kernel(services);

            // Create a temporary plugin file with an OpenApi runtime with functions but no spec url
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, @"
{
  ""runtimes"": [
    {
      ""type"": ""OpenApi"",
      ""runForFunctions"": [""Function1""],
      ""spec"": { }
    }
  ],
  ""functions"": [
    { ""name"": ""Function1"" }
  ]
}
");

                // Act & Assert
                var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                {
                    await kernel.CreatePluginFromCopilotAgentPluginAsync("TestPlugin", tempFile);
                });

                // Assert the warning was logged
                Assert.Contains("No API description URL found in the runtime object.", loggerProvider.Warnings);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}
