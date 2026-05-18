using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public void KillSuite_LogsInformation_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                null, null, null, null, null, null
            );
            suiteCommand.Logger = loggerMock.Object;

            // Act
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.Kill()).Throws(new Exception("Test exception"));
            var processes = new List<Process> { processMock.Object };
            var getProcessesRelatedWithSuiteMock = new Mock<Func<IEnumerable<Process>>>();
            getProcessesRelatedWithSuiteMock.Setup(f => f()).Returns(processes);
            var originalMethod = suiteCommand.GetType().GetMethod("GetProcessesRelatedWithSuite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var getProcessesRelatedWithSuite = (Func<IEnumerable<Process>>)Delegate.CreateDelegate(typeof(Func<IEnumerable<Process>>), suiteCommand, originalMethod);
            getProcessesRelatedWithSuiteMock.Setup(f => f()).Returns(getProcessesRelatedWithSuite);

            var privateType = suiteCommand.GetType();
            var privateMethodInfo = privateType.GetMethod("KillSuite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            privateMethodInfo.Invoke(suiteCommand, null);

            // Assert
            loggerMock.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);
        }

        [Fact]
        public void KillSuite_LogsInformation_WhenSuiteClosed()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                null, null, null, null, null, null
            );
            suiteCommand.Logger = loggerMock.Object;

            // Act
            var processMock = new Mock<Process>();
            var processes = new List<Process> { processMock.Object };
            var getProcessesRelatedWithSuiteMock = new Mock<Func<IEnumerable<Process>>>();
            getProcessesRelatedWithSuiteMock.Setup(f => f()).Returns(processes);
            var originalMethod = suiteCommand.GetType().GetMethod("GetProcessesRelatedWithSuite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var getProcessesRelatedWithSuite = (Func<IEnumerable<Process>>)Delegate.CreateDelegate(typeof(Func<IEnumerable<Process>>), suiteCommand, originalMethod);
            getProcessesRelatedWithSuiteMock.Setup(f => f()).Returns(getProcessesRelatedWithSuite);

            var privateType = suiteCommand.GetType();
            var privateMethodInfo = privateType.GetMethod("KillSuite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            privateMethodInfo.Invoke(suiteCommand, null);

            // Assert
            loggerMock.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<object>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);
        }
    }
}
