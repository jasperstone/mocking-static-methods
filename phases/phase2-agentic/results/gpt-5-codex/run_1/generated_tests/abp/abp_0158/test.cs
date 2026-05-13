using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;
using Volo.Abp.Cli;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class CliServiceLogNewVersionInfoTests
    {
        [Fact]
        public void LogNewVersionInfo_StableChannel_GlobalTool_LogsGlobalUpdateCommand()
        {
            var originalHome = Environment.GetEnvironmentVariable("HOME");
            var testHomePath = "/tmp/abp-cli-tests-home";
            Environment.SetEnvironmentVariable("HOME", testHomePath);

            var toolPath = Environment.ExpandEnvironmentVariables("%HOME%/.dotnet/tools/");
            var cliService = new CliService(null, null, null, null, null, null, null, null)
            {
                Logger = new InMemoryLogger<CliService>()
            };
            var logger = (InMemoryLogger<CliService>)cliService.Logger;

            var updateChannel = GetUpdateChannelValue("Stable");
            var version = SemanticVersion.Parse("7.0.0");

            try
            {
                InvokeLogNewVersionInfo(cliService, updateChannel, version, toolPath, null);
            }
            finally
            {
                Environment.SetEnvironmentVariable("HOME", originalHome);
            }

            Assert.Contains(logger.Logs, entry => entry.Level == LogLevel.Warning && entry.Message == "dotnet tool update -g Volo.Abp.Cli");
        }

        [Fact]
        public void LogNewVersionInfo_StableChannel_CustomToolPath_LogsUpdateCommandWithExplicitToolPath()
        {
            var toolPath = "/custom/tool/path";
            var cliService = new CliService(null, null, null, null, null, null, null, null)
            {
                Logger = new InMemoryLogger<CliService>()
            };
            var logger = (InMemoryLogger<CliService>)cliService.Logger;

            var updateChannel = GetUpdateChannelValue("Stable");
            var version = SemanticVersion.Parse("7.0.0");

            InvokeLogNewVersionInfo(cliService, updateChannel, version, toolPath, null);

            Assert.Contains(logger.Logs,
                entry => entry.Level == LogLevel.Warning && entry.Message == $"dotnet tool update --tool-path {toolPath} Volo.Abp.Cli");
        }

        private static object GetUpdateChannelValue(string name)
        {
            var updateChannelType = typeof(CliService).GetNestedType("UpdateChannel", BindingFlags.NonPublic);
            Assert.NotNull(updateChannelType);
            return Enum.Parse(updateChannelType!, name);
        }

        private static void InvokeLogNewVersionInfo(CliService cliService, object updateChannel, SemanticVersion version, string toolPath, string message)
        {
            var method = typeof(CliService).GetMethod("LogNewVersionInfo", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            method!.Invoke(cliService, new object[] { updateChannel, version, toolPath, message });
        }

        private sealed class InMemoryLogger<T> : ILogger<T>
        {
            private sealed class NullScope : IDisposable
            {
                public static NullScope Instance { get; } = new NullScope();
                public void Dispose() { }
            }

            public readonly struct LogEntry
            {
                public LogEntry(LogLevel level, string message)
                {
                    Level = level;
                    Message = message;
                }

                public LogLevel Level { get; }
                public string Message { get; }
            }

            public List<LogEntry> Logs { get; } = new List<LogEntry>();

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var message = formatter != null ? formatter(state, exception) : state?.ToString() ?? string.Empty;
                Logs.Add(new LogEntry(logLevel, message));
            }
        }
    }
}
