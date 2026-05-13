using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class ProjectCreationCommandBaseTests
    {
        [Fact]
        public async Task GetProjectBuildArgsAsync_LogsVersion_WhenVersionOptionProvided()
        {
            var versionKey = $"{CommandLineArgs.ArgumentPrefix}{ProjectCreationCommandBase.Options.Version.Long}";
            var options = new Dictionary<string, string>
            {
                [versionKey] = "1.0.0",
                [ProjectCreationCommandBase.Options.Web.Short] = "true"
            };
            var args = new CommandLineArgs("new", new Dictionary<string, string>(), options);
            var logger = new TestLogger();
            var command = new TestProjectCreationCommand(new TestConnectionStringProvider());
            command.SetLoggerInstance(logger);

            await command.ExecuteGetProjectBuildArgsAsync(args, "app", "MyProject");

            var versionLogs = logger.Entries.Where(entry => entry.Level == LogLevel.Information && entry.Message == "Version: 1.0.0").ToList();
            Assert.Single(versionLogs);
        }

        [Fact]
        public async Task GetProjectBuildArgsAsync_DoesNotLogVersion_WhenVersionOptionMissing()
        {
            var options = new Dictionary<string, string>
            {
                [ProjectCreationCommandBase.Options.Web.Short] = "true"
            };
            var args = new CommandLineArgs("new", new Dictionary<string, string>(), options);
            var logger = new TestLogger();
            var command = new TestProjectCreationCommand(new TestConnectionStringProvider());
            command.SetLoggerInstance(logger);

            await command.ExecuteGetProjectBuildArgsAsync(args, "app", "MyProject");

            Assert.DoesNotContain(logger.Entries, entry => entry.Message.StartsWith("Version:", StringComparison.Ordinal));
        }

        private sealed class TestProjectCreationCommand : ProjectCreationCommandBase
        {
            public TestProjectCreationCommand(IConnectionStringProvider connectionStringProvider)
                : base(connectionStringProvider, null, null, null, null, null, null, null, null, null, null, null)
            {
            }

            public Task<ProjectBuildArgs> ExecuteGetProjectBuildArgsAsync(CommandLineArgs commandLineArgs, string template, string projectName)
                => GetProjectBuildArgsAsync(commandLineArgs, template, projectName);

            public void SetLoggerInstance(ILogger logger)
            {
                Logger = logger;
            }
        }

        private sealed class TestConnectionStringProvider : IConnectionStringProvider
        {
            public Task<string> GetConnectionString(DatabaseManagementSystem databaseManagementSystem)
                => Task.FromResult("DefaultConnection");
        }

        private sealed class TestLogger : ILogger
        {
            public List<(LogLevel Level, string Message)> Entries { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var message = formatter != null ? formatter(state, exception) : state?.ToString() ?? string.Empty;
                Entries.Add((logLevel, message));
            }

            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new();

                public void Dispose()
                {
                }
            }
        }
    }
}
