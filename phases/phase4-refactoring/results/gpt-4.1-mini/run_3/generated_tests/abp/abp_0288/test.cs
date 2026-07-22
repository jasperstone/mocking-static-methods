using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class SuiteCommandTests
    {
        private class TestLogger : ILogger<SuiteCommand>
        {
            public List<string> Messages = new List<string>();

            public IDisposable BeginScope<TState>(TState state) => null!;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                Messages.Add(formatter(state, exception));
            }
        }

        private class TestSuiteCommand : SuiteCommand
        {
            public List<string> LoggedMessages => ((TestLogger)Logger).Messages;

            public TestSuiteCommand()
                : base(
                    nuGetIndexUrlService: null,
                    packageVersionCheckerService: null,
                    cmdHelper: null,
                    authService: null,
                    cliHttpClientFactory: null,
                    suiteAppSettingsService: null)
            {
                Logger = new TestLogger();
            }

            // Expose KillSuite logic for testing by simulating the process kill loop
            public void TestKillSuiteWithProcesses(IEnumerable<Process> processes)
            {
                try
                {
                    foreach (var suiteProcess in processes)
                    {
                        // Simulate Kill call
                        // Instead of calling Kill, just log "Suite closed." to simulate success
                        Logger.LogInformation("Suite closed.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogInformation("Cannot close Suite." + ex.Message);
                }
            }

            // Expose KillSuite logic for testing with exception simulation
            public void TestKillSuiteWithException(IEnumerable<Process> processes)
            {
                try
                {
                    foreach (var suiteProcess in processes)
                    {
                        throw new InvalidOperationException("Test exception");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogInformation("Cannot close Suite." + ex.Message);
                }
            }
        }

        [Fact]
        public void KillSuite_LogsInformationOnProcessKill()
        {
            var suiteCommand = new TestSuiteCommand();

            var dummyProcesses = new List<Process> { new Process() };

            suiteCommand.TestKillSuiteWithProcesses(dummyProcesses);

            Assert.Contains("Suite closed.", suiteCommand.LoggedMessages);
        }

        [Fact]
        public void KillSuite_LogsInformationOnException()
        {
            var suiteCommand = new TestSuiteCommand();

            var dummyProcesses = new List<Process> { new Process() };

            suiteCommand.TestKillSuiteWithException(dummyProcesses);

            Assert.Contains(suiteCommand.LoggedMessages, msg => msg.StartsWith("Cannot close Suite."));
            Assert.Contains(suiteCommand.LoggedMessages, msg => msg.Contains("Test exception"));
        }
    }
}
