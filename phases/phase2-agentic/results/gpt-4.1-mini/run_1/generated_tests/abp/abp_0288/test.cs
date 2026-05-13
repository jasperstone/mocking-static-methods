using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class SuiteCommandTests
    {
        private class TestSuiteCommand : SuiteCommand
        {
            public List<string> LoggedMessages = new();

            public TestSuiteCommand()
                : base(
                    nuGetIndexUrlService: null!,
                    packageVersionCheckerService: null!,
                    cmdHelper: null!,
                    authService: null!,
                    cliHttpClientFactory: null!,
                    suiteAppSettingsService: null!)
            {
                // Override Logger with a mock that captures LogInformation calls
                var loggerMock = new Mock<ILogger<SuiteCommand>>();
                loggerMock.Setup(l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                    .Callback<LogLevel, EventId, object, Exception?, Func<object, Exception?, string>>(
                        (level, eventId, state, exception, formatter) =>
                        {
                            var message = formatter(state, exception);
                            LoggedMessages.Add(message);
                        });
                Logger = loggerMock.Object;
            }

            // Expose KillSuite for testing
            public void CallKillSuite()
            {
                KillSuite();
            }

            // Override GetProcessesRelatedWithSuite to control test behavior
            private List<Process> _processesToReturn = new();
            public void SetProcessesToReturn(IEnumerable<Process> processes)
            {
                _processesToReturn = processes.ToList();
            }

            protected override IEnumerable<Process> GetProcessesRelatedWithSuite()
            {
                return _processesToReturn;
            }
        }

        [Fact]
        public void KillSuite_LogsInformation_WhenProcessesKilled()
        {
            // Arrange
            var suiteCommand = new TestSuiteCommand();

            // Create mock processes that track Kill calls
            var killedProcesses = new List<Mock<Process>>();
            var processes = new List<Process>();

            for (int i = 0; i < 2; i++)
            {
                var mockProcess = new Mock<Process>();
                mockProcess.Setup(p => p.Kill()).Callback(() => { });
                killedProcesses.Add(mockProcess);
                processes.Add(mockProcess.Object);
            }

            suiteCommand.SetProcessesToReturn(processes);

            // Act
            suiteCommand.CallKillSuite();

            // Assert
            // We expect "Suite closed." logged once per process killed
            Assert.Equal(processes.Count, suiteCommand.LoggedMessages.Count(m => m == "Suite closed."));
        }

        [Fact]
        public void KillSuite_LogsInformation_WhenExceptionThrown()
        {
            // Arrange
            var suiteCommand = new TestSuiteCommand();

            // Setup GetProcessesRelatedWithSuite to throw exception
            suiteCommand.SetProcessesToReturn(new List<Process>());

            // Override GetProcessesRelatedWithSuite to throw
            var exceptionMessage = "Test exception";
            var suiteCommandMock = new Mock<TestSuiteCommand>();
            suiteCommandMock.CallBase = true;
            suiteCommandMock.Setup(s => s.GetProcessesRelatedWithSuite()).Throws(new InvalidOperationException(exceptionMessage));
            suiteCommandMock.Object.Logger = suiteCommand.Logger;

            // Act
            suiteCommandMock.Object.CallKillSuite();

            // Assert
            Assert.Contains(suiteCommandMock.Object.LoggedMessages, m => m.Contains("Cannot close Suite.") && m.Contains(exceptionMessage));
        }
    }
}
