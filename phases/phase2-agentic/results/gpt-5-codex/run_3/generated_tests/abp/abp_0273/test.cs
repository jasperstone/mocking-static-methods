using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class SuiteCommandTests
    {
        [Fact]
        public void ShowSuiteManualInstallCommand_Should_Log_Manual_Install_Instructions()
        {
            // Arrange
            var logger = new TestLogger<SuiteCommand>();
            var command = new SuiteCommand(
                currentDirectoryLocator: null,
                loginCommand: null,
                serviceScopeFactory: null,
                nuGetIndexUrlService: null,
                jsonSerializer: null,
                cancellationTokenProvider: null,
                currentUser: null,
                serviceFilterExecutor: null,
                logger: logger);

            var method = typeof(SuiteCommand).GetMethod("ShowSuiteManualInstallCommand", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);

            // Act
            method!.Invoke(command, null);

            // Assert
            Assert.Contains(logger.Messages, entry =>
                entry.Level == LogLevel.Information &&
                entry.Message == "You can also run the following command to install ABP Suite.");

            Assert.Contains(logger.Messages, entry =>
                entry.Level == LogLevel.Information &&
                entry.Message == "dotnet tool install -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json");
        }

        private sealed class TestLogger<T> : ILogger<T>
        {
            public List<(LogLevel Level, string Message)> Messages { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var message = formatter != null ? formatter(state, exception) : state?.ToString();
                Messages.Add((logLevel, message ?? string.Empty));
            }

            private sealed class NullScope : IDisposable
            {
                public static NullScope Instance { get; } = new NullScope();

                public void Dispose()
                {
                }
            }
        }
    }
}
