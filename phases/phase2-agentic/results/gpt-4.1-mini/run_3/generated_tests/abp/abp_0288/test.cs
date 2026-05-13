using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
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
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()))
                    .Callback<LogLevel, EventId, object, Exception, Delegate>((level, id, state, ex, formatter) =>
                    {
                        var message = formatter.DynamicInvoke(state, ex) as string;
                        if (message != null)
                        {
                            LoggedMessages.Add(message);
                        }
                    });
                Logger = loggerMock.Object;
            }

            // Expose KillSuite for testing
            public void CallKillSuite()
            {
                base.KillSuite();
            }

            // Override GetProcessesRelatedWithSuite to control test behavior
            public void SetProcesses(IEnumerable<Process> processes)
            {
                _testProcesses = processes;
            }

            private IEnumerable<Process>? _testProcesses;

            protected override IEnumerable<Process> GetProcessesRelatedWithSuite()
            {
                if (_testProcesses != null)
                {
                    return _testProcesses;
                }
                return base.GetProcessesRelatedWithSuite();
            }
        }

        [Fact]
        public void KillSuite_LogsInformationOnSuccessfulKill()
        {
            // Arrange
            var suiteCommand = new TestSuiteCommand();

            var mockProcess = new Mock<Process>();
            mockProcess.Setup(p => p.Kill());

            suiteCommand.SetProcesses(new[] { mockProcess.Object });

            // Act
            suiteCommand.CallKillSuite();

            // Assert
            Assert.Contains("Suite closed.", suiteCommand.LoggedMessages);
            mockProcess.Verify(p => p.Kill(), Times.Once);
        }

        [Fact]
        public void KillSuite_LogsInformationOnException()
        {
            // Arrange
            var suiteCommand = new TestSuiteCommand();

            var mockProcess = new Mock<Process>();
            mockProcess.Setup(p => p.Kill()).Throws(new InvalidOperationException("Test exception"));

            suiteCommand.SetProcesses(new[] { mockProcess.Object });

            // Act
            suiteCommand.CallKillSuite();

            // Assert
            Assert.Contains(suiteCommand.LoggedMessages, msg => msg.StartsWith("Cannot close Suite.") && msg.Contains("Test exception"));
        }
    }
}
