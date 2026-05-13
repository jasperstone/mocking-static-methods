using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public void StartSuite_ShouldLogWarning_WhenSuiteToolIsNotInstalled()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "AbpSuiteTests_" + Guid.NewGuid());
            Directory.CreateDirectory(tempDir);

            var originalHome = Environment.GetEnvironmentVariable("HOME");
            var originalUserProfile = Environment.GetEnvironmentVariable("USERPROFILE");

            Environment.SetEnvironmentVariable("HOME", tempDir);
            Environment.SetEnvironmentVariable("USERPROFILE", tempDir);

            try
            {
                var windowsToolPath = Path.Combine(tempDir, ".dotnet", "tools", "abp-suite.exe");
                var linuxToolPath = Path.Combine(tempDir, ".dotnet", "tools", "abp-suite");

                if (File.Exists(windowsToolPath))
                {
                    File.Delete(windowsToolPath);
                }

                if (File.Exists(linuxToolPath))
                {
                    File.Delete(linuxToolPath);
                }

                var suiteCommand = new SuiteCommand(null, null, null, null, null, null);
                var logger = new TestLogger<SuiteCommand>();
                suiteCommand.Logger = logger;

                var startSuiteMethod = typeof(SuiteCommand).GetMethod("StartSuite", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.NotNull(startSuiteMethod);

                var result = startSuiteMethod!.Invoke(suiteCommand, Array.Empty<object>());

                Assert.Null(result);
                var warningLog = Assert.Single(logger.Logs.Where(log => log.LogLevel == LogLevel.Warning));
                Assert.Equal("ABP Suite is not installed! To install it you can run the command: \"abp suite install\"", warningLog.Message);
            }
            finally
            {
                Environment.SetEnvironmentVariable("HOME", originalHome);
                Environment.SetEnvironmentVariable("USERPROFILE", originalUserProfile);

                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch
                {
                    // Ignore cleanup issues.
                }
            }
        }

        private sealed class TestLogger<T> : ILogger<T>
        {
            private readonly List<LogEntry> _logs = new();

            public IReadOnlyList<LogEntry> Logs => _logs;

            public IDisposable BeginScope<TState>(TState state) => NullDisposable.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var message = formatter != null ? formatter(state, exception) : state?.ToString();
                _logs.Add(new LogEntry(logLevel, eventId, message ?? string.Empty, exception));
            }

            public readonly record struct LogEntry(LogLevel LogLevel, EventId EventId, string Message, Exception Exception);
        }

        private sealed class NullDisposable : IDisposable
        {
            public static readonly NullDisposable Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
