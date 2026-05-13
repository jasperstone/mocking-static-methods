using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

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
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            loggerMock.Setup(l => l.Log(
                It.IsAny<LogLevel>(),
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

        // Override GetProcessesRelatedWithSuite to simulate processes
        private List<Process> _processesToReturn = new();
        public void SetProcessesToReturn(List<Process> processes)
        {
            _processesToReturn = processes;
        }

        protected override IEnumerable<Process> GetProcessesRelatedWithSuite()
        {
            return _processesToReturn;
        }
    }

    [Fact]
    public void KillSuite_LogsInformationOnEachProcessKilled()
    {
        // Arrange
        var suiteCommand = new TestSuiteCommand();

        var processMock1 = new Mock<Process>();
        processMock1.Setup(p => p.ProcessName).Returns("abp-suite");
        processMock1.Setup(p => p.Kill());

        var processMock2 = new Mock<Process>();
        processMock2.Setup(p => p.ProcessName).Returns("abp-suite-helper");
        processMock2.Setup(p => p.Kill());

        var processes = new List<Process> { processMock1.Object, processMock2.Object };
        suiteCommand.SetProcessesToReturn(processes);

        // Act
        suiteCommand.CallKillSuite();

        // Assert
        Assert.Contains("Suite closed.", suiteCommand.LoggedMessages);
        Assert.Equal(2, suiteCommand.LoggedMessages.FindAll(m => m == "Suite closed.").Count);
        processMock1.Verify(p => p.Kill(), Times.Once);
        processMock2.Verify(p => p.Kill(), Times.Once);
    }

    [Fact]
    public void KillSuite_LogsInformationWhenExceptionThrown()
    {
        // Arrange
        var suiteCommand = new TestSuiteCommand();

        var processMock = new Mock<Process>();
        processMock.Setup(p => p.ProcessName).Returns("abp-suite");
        processMock.Setup(p => p.Kill()).Throws(new InvalidOperationException("Test exception"));

        var processes = new List<Process> { processMock.Object };
        suiteCommand.SetProcessesToReturn(processes);

        // Act
        suiteCommand.CallKillSuite();

        // Assert
        Assert.Single(suiteCommand.LoggedMessages);
        Assert.StartsWith("Cannot close Suite.", suiteCommand.LoggedMessages[0]);
        Assert.Contains("Test exception", suiteCommand.LoggedMessages[0]);
    }
}
