using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public void KillSuite_ShouldLogInformation_WhenSuiteClosed()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                Mock.Of<AbpNuGetIndexUrlService>(),
                Mock.Of<PackageVersionCheckerService>(),
                Mock.Of<ICmdHelper>(),
                Mock.Of<AuthService>(),
                Mock.Of<CliHttpClientFactory>(),
                Mock.Of<SuiteAppSettingsService>()
            )
            {
                Logger = mockLogger.Object
            };

            var mockProcess = new Mock<Process>();
            mockProcess.Setup(p => p.ProcessName).Returns("abp-suite");
            mockProcess.Setup(p => p.Kill()).Verifiable();

            var processes = new List<Process> { mockProcess.Object };

            var suiteCommandType = typeof(SuiteCommand);
            var getProcessesRelatedWithSuiteMethod = suiteCommandType.GetMethod("GetProcessesRelatedWithSuite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var getProcessesRelatedWithSuiteDelegate = (Func<IEnumerable<Process>>)Delegate.CreateDelegate(typeof(Func<IEnumerable<Process>>), suiteCommand, getProcessesRelatedWithSuiteMethod);

            var getProcessesRelatedWithSuiteMock = new Mock<Func<IEnumerable<Process>>>();
            getProcessesRelatedWithSuiteMock.Setup(m => m()).Returns(processes);

            suiteCommandType.GetField("_getProcessesRelatedWithSuite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(suiteCommand, getProcessesRelatedWithSuiteDelegate);

            // Act
            suiteCommandType.GetMethod("KillSuite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(suiteCommand, null);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Suite closed.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void KillSuite_ShouldLogInformation_WhenCannotCloseSuite()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                Mock.Of<AbpNuGetIndexUrlService>(),
                Mock.Of<PackageVersionCheckerService>(),
                Mock.Of<ICmdHelper>(),
                Mock.Of<AuthService>(),
                Mock.Of<CliHttpClientFactory>(),
                Mock.Of<SuiteAppSettingsService>()
            )
            {
                Logger = mockLogger.Object
            };

            var mockProcess = new Mock<Process>();
            mockProcess.Setup(p => p.ProcessName).Returns("abp-suite");
            mockProcess.Setup(p => p.Kill()).Throws(new Exception("Test exception"));

            var processes = new List<Process> { mockProcess.Object };

            var suiteCommandType = typeof(SuiteCommand);
            var getProcessesRelatedWithSuiteMethod = suiteCommandType.GetMethod("GetProcessesRelatedWithSuite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var getProcessesRelatedWithSuiteDelegate = (Func<IEnumerable<Process>>)Delegate.CreateDelegate(typeof(Func<IEnumerable<Process>>), suiteCommand, getProcessesRelatedWithSuiteMethod);

            var getProcessesRelatedWithSuiteMock = new Mock<Func<IEnumerable<Process>>>();
            getProcessesRelatedWithSuiteMock.Setup(m => m()).Returns(processes);

            suiteCommandType.GetField("_getProcessesRelatedWithSuite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(suiteCommand, getProcessesRelatedWithSuiteDelegate);

            // Act
            suiteCommandType.GetMethod("KillSuite", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(suiteCommand, null);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cannot close Suite.Test exception")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
