using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Commands.Services.Tests
{
    public class DotnetEfToolManagerTests
    {
        [Fact]
        public async Task BeSureInstalledAsync_InstallsAndLogsWhenToolNotInstalled()
        {
            var cmdHelper = new TestCmdHelper
            {
                RunCmdAndGetOutputResult = "some-other-tool"
            };
            var logger = new TestLogger<DotnetEfToolManager>();

            var manager = new DotnetEfToolManager(cmdHelper)
            {
                Logger = logger
            };

            await manager.BeSureInstalledAsync();

            Assert.Contains(cmdHelper.RunCmdAndGetOutputCommands, command => command == "dotnet tool list -g");
            Assert.Contains(cmdHelper.RunCmdCommands, command => command == "dotnet tool install --global dotnet-ef");
            Assert.Contains(logger.Logs,
                log => log.Level == LogLevel.Information && log.Message == "Installing dotnet-ef tool...");
            Assert.Contains(logger.Logs,
                log => log.Level == LogLevel.Information && log.Message == "dotnet-ef tool is installed.");
        }

        [Fact]
        public async Task BeSureInstalledAsync_DoesNothingWhenToolAlreadyInstalled()
        {
            var cmdHelper = new TestCmdHelper
            {
                RunCmdAndGetOutputResult = "some-tool dotnet-ef"
            };
            var logger = new TestLogger<DotnetEfToolManager>();

            var manager = new DotnetEfToolManager(cmdHelper)
            {
                Logger = logger
            };

            await manager.BeSureInstalledAsync();

            Assert.Contains(cmdHelper.RunCmdAndGetOutputCommands, command => command == "dotnet tool list -g");
            Assert.Empty(cmdHelper.RunCmdCommands);
            Assert.DoesNotContain(logger.Logs,
                log => log.Level == LogLevel.Information && log.Message == "Installing dotnet-ef tool...");
        }

        private sealed class TestCmdHelper : ICmdHelper
        {
            public List<string> RunCmdCommands { get; } = new();
            public List<string> RunCmdAndGetOutputCommands { get; } = new();
            public string RunCmdAndGetOutputResult { get; set; } = string.Empty;

            public void Open(string pathOrUrl) => throw new NotImplementedException();

            public void Run(string file, string arguments) => throw new NotImplementedException();

            public string GetArguments(string command, int? delaySeconds = null) => throw new NotImplementedException();

            public string GetFileName() => throw new NotImplementedException();

            public void RunCmd(string command, string workingDirectory = null)
            {
                RunCmdCommands.Add(command);
            }

            public Process RunCmdAndGetProcess(string command, string workingDirectory = null) => throw new NotImplementedException();

            public void RunCmd(string command, out int exitCode, string workingDirectory = null) => throw new NotImplementedException();

            public string RunCmdAndGetOutput(string command, string workingDirectory = null)
            {
                RunCmdAndGetOutputCommands.Add(command);
                return RunCmdAndGetOutputResult;
            }

            public string RunCmdAndGetOutput(string command, out bool isExitCodeSuccessful, string workingDirectory = null) =>
                throw new NotImplementedException();

            public string RunCmdAndGetOutput(string command, out int exitCode, string workingDirectory = null) =>
                throw new NotImplementedException();

            public void RunCmdAndExit(string command, string workingDirectory = null, int? delaySeconds = null) =>
                throw new NotImplementedException();
        }

        private sealed class TestLogger<T> : ILogger<T>
        {
            public List<(LogLevel Level, string Message)> Logs { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
                Func<TState, Exception, string> formatter)
            {
                var message = formatter != null ? formatter(state, exception) : state?.ToString();
                Logs.Add((logLevel, message));
            }

            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new NullScope();

                public void Dispose()
                {
                }
            }
        }
    }
}
