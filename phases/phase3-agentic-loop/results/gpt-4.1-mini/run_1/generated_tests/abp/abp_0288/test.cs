using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public void KillSuite_LogsInformationOnEachProcessKilled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommandTestable(loggerMock.Object);

            var processMock1 = new Mock<Process>();
            processMock1.Setup(p => p.ProcessName).Returns("abp-suite-1");
            processMock1.Setup(p => p.Kill());

            var processMock2 = new Mock<Process>();
            processMock2.Setup(p => p.ProcessName).Returns("abp-suite-2");
            processMock2.Setup(p => p.Kill());

            var processes = new List<Process> { processMock1.Object, processMock2.Object };

            suiteCommand.SetProcesses(processes);

            // Act
            suiteCommand.InvokeKillSuite();

            // Assert
            processMock1.Verify(p => p.Kill(), Times.Once);
            processMock2.Verify(p => p.Kill(), Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Suite closed."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));
        }

        [Fact]
        public void KillSuite_LogsInformationOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommandTestable(loggerMock.Object);

            suiteCommand.SetThrowOnGetProcesses(new InvalidOperationException("Test exception"));

            // Act
            suiteCommand.InvokeKillSuite();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().StartsWith("Cannot close Suite.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // A minimal subclass to override GetProcessesRelatedWithSuite and expose KillSuite for testing
        private class SuiteCommandTestable : SuiteCommand
        {
            private IEnumerable<Process> _processesToReturn;
            private Exception _exceptionToThrow;
            private readonly ILogger<SuiteCommand> _logger;

            public SuiteCommandTestable(ILogger<SuiteCommand> logger)
                : base(null, null, null, null, null, null)
            {
                Logger = logger;
                _logger = logger;
            }

            public void SetProcesses(IEnumerable<Process> processes)
            {
                _processesToReturn = processes;
                _exceptionToThrow = null;
            }

            public void SetThrowOnGetProcesses(Exception ex)
            {
                _exceptionToThrow = ex;
                _processesToReturn = null;
            }

            public void InvokeKillSuite()
            {
                KillSuite();
            }

            protected override IEnumerable<Process> GetProcessesRelatedWithSuite()
            {
                if (_exceptionToThrow != null)
                {
                    throw _exceptionToThrow;
                }

                if (_processesToReturn != null)
                {
                    return _processesToReturn;
                }

                return base.GetProcessesRelatedWithSuite();
            }
        }
    }
}
