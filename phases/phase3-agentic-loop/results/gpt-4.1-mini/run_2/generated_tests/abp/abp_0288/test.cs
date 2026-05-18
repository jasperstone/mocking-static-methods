using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class SuiteCommandTests
    {
        [Fact]
        public void KillSuite_LogsInformation_WhenProcessesKilled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SuiteCommand>>();
            var suiteCommandWithProcesses = new SuiteCommandWithProcesses(new List<ProcessStub> { new ProcessStub() });
            suiteCommandWithProcesses.Logger = mockLogger.Object;

            // Act
            var killSuiteMethod = typeof(SuiteCommand).GetMethod("KillSuite", BindingFlags.NonPublic | BindingFlags.Instance);
            killSuiteMethod.Invoke(suiteCommandWithProcesses, null);

            // Assert
            Assert.True(suiteCommandWithProcesses.ProcessKilled);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Suite closed."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(1));
        }

        [Fact]
        public void KillSuite_LogsInformation_WhenExceptionThrown()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SuiteCommand>>();
            var suiteCommandWithException = new SuiteCommandWithException();
            suiteCommandWithException.Logger = mockLogger.Object;

            // Act
            var killSuiteMethod = typeof(SuiteCommand).GetMethod("KillSuite", BindingFlags.NonPublic | BindingFlags.Instance);
            killSuiteMethod.Invoke(suiteCommandWithException, null);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().StartsWith("Cannot close Suite.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class SuiteCommandWithProcesses : SuiteCommand
        {
            public bool ProcessKilled { get; private set; }

            private readonly IEnumerable<ProcessStub> _processes;

            public SuiteCommandWithProcesses(IEnumerable<ProcessStub> processes) : base(null, null, null, null, null, null)
            {
                _processes = processes;
            }

            // Override GetProcessesRelatedWithSuite to return stub processes
            private new IEnumerable<Process> GetProcessesRelatedWithSuite()
            {
                return _processes;
            }

            // Override KillSuite to call Kill on stub processes and set flag
            private new void KillSuite()
            {
                try
                {
                    foreach (var suiteProcess in GetProcessesRelatedWithSuite())
                    {
                        suiteProcess.Kill();
                        ProcessKilled = true;
                        Logger.LogInformation("Suite closed.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogInformation("Cannot close Suite." + ex.Message);
                }
            }
        }

        private class SuiteCommandWithException : SuiteCommand
        {
            public SuiteCommandWithException() : base(null, null, null, null, null, null)
            {
            }

            private new IEnumerable<Process> GetProcessesRelatedWithSuite()
            {
                throw new Exception("Test exception");
            }
        }

        private class ProcessStub : Process
        {
            public bool KillCalled { get; private set; }

            public void Kill()
            {
                KillCalled = true;
            }

            public string ProcessName => "abp-suite";
        }
    }
}
