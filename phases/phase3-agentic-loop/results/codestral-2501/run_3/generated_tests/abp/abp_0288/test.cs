using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Version;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class SuiteCommandTests
    {
        private readonly Mock<ILogger<SuiteCommand>> _loggerMock;
        private readonly SuiteCommand _suiteCommand;

        public SuiteCommandTests()
        {
            _loggerMock = new Mock<ILogger<SuiteCommand>>();
            _suiteCommand = new SuiteCommand(
                Mock.Of<AbpNuGetIndexUrlService>(),
                Mock.Of<PackageVersionCheckerService>(),
                Mock.Of<ICmdHelper>(),
                Mock.Of<AuthService>(),
                Mock.Of<CliHttpClientFactory>(),
                Mock.Of<SuiteAppSettingsService>()
            )
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public void KillSuite_ShouldLogInformation_WhenSuiteProcessesAreKilled()
        {
            // Arrange
            var mockProcesses = new List<Process>
            {
                new Process { StartInfo = new ProcessStartInfo { FileName = "abp-suite" } },
                new Process { StartInfo = new ProcessStartInfo { FileName = "abp-suite" } }
            };

            var suiteCommandType = typeof(SuiteCommand);
            var getProcessesRelatedWithSuiteMethod = suiteCommandType.GetMethod("GetProcessesRelatedWithSuite", BindingFlags.NonPublic | BindingFlags.Instance);
            getProcessesRelatedWithSuiteMethod.Invoke(_suiteCommand, null).Returns(mockProcesses);

            // Act
            getProcessesRelatedWithSuiteMethod.Invoke(_suiteCommand, null);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Suite closed.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(mockProcesses.Count));
        }

        [Fact]
        public void KillSuite_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            var suiteCommandType = typeof(SuiteCommand);
            var getProcessesRelatedWithSuiteMethod = suiteCommandType.GetMethod("GetProcessesRelatedWithSuite", BindingFlags.NonPublic | BindingFlags.Instance);
            getProcessesRelatedWithSuiteMethod.Invoke(_suiteCommand, null).Throws(new Exception("Test exception"));

            // Act
            getProcessesRelatedWithSuiteMethod.Invoke(_suiteCommand, null);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cannot close Suite.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
