using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;
using Volo.Abp.Cli;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class CliServiceLogTests
    {
        [Fact]
        public void LogNewVersionInfo_Should_Log_Update_Command_For_Stable_Channel()
        {
            // Arrange
            var cliService = (CliService)FormatterServices.GetUninitializedObject(typeof(CliService));
            var testLogger = new TestLogger<CliService>();
            cliService.Logger = testLogger;

            var updateChannelType = typeof(CliService).GetNestedType("UpdateChannel", BindingFlags.NonPublic);
            Assert.NotNull(updateChannelType);
            var stableUpdateChannel = Enum.Parse(updateChannelType!, "Stable");
            var logNewVersionInfoMethod = typeof(CliService).GetMethod("LogNewVersionInfo", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(logNewVersionInfoMethod);

            var latestVersion = new SemanticVersion(5, 2, 1);
            var toolPath = @"C:\cli-tools";

            // Act
            logNewVersionInfoMethod!.Invoke(cliService, new[] { stableUpdateChannel, latestVersion, toolPath, "Custom info" });

            // Assert
            Assert.Contains($"dotnet tool update --tool-path {toolPath} Volo.Abp.Cli", testLogger.WarningMessages);
            Assert.Contains("Custom info", testLogger.WarningMessages);
            Assert.Contains($"A newer stable version of the ABP CLI is available: {latestVersion}.", testLogger.WarningMessages);
        }

        private sealed class TestLogger<T> : ILogger<T>
        {
            private readonly List<(LogLevel LogLevel, string Message)> _entries = new();

            public IReadOnlyList<string> WarningMessages => _entries
                .Where(entry => entry.LogLevel == LogLevel.Warning)
                .Select(entry => entry.Message ?? string.Empty)
                .ToList();

            public IDisposable BeginScope<TState>(TState state) => NoopDisposable.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var message = formatter != null
                    ? formatter(state, exception)
                    : state?.ToString() ?? string.Empty;

                _entries.Add((logLevel, message ?? string.Empty));
            }

            private sealed class NoopDisposable : IDisposable
            {
                public static readonly NoopDisposable Instance = new();
                public void Dispose()
                {
                }
            }
        }
    }
}
