using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public void KillSuite_LogsInformation_WhenSuiteProcessesAreKilled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                null, // AbpNuGetIndexUrlService
                null, // PackageVersionCheckerService
                null, // ICmdHelper
                null, // AuthService
                null, // CliHttpClientFactory
                null  // SuiteAppSettingsService
            )
            {
                Logger = loggerMock.Object
            };

            var mockProcess1 = new Mock<Process>();
            var mockProcess2 = new Mock<Process>();
            mockProcess1.Setup(p => p.ProcessName).Returns("abp-suite");
            mockProcess2.Setup(p => p.ProcessName).Returns("abp-suite");

            suiteCommand.GetProcessesRelatedWithSuite = () => new List<Process> { mockProcess1.Object, mockProcess2.Object };

            // Act
            suiteCommand.KillSuite();

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Suite closed."),
                Times.Exactly(2)
            );
        }

        [Fact]
        public void KillSuite_LogsInformation_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                null, // AbpNuGetIndexUrlService
                null, // PackageVersionCheckerService
                null, // ICmdHelper
                null, // AuthService
                null, // CliHttpClientFactory
                null  // SuiteAppSettingsService
            )
            {
                Logger = loggerMock.Object
            };

            suiteCommand.GetProcessesRelatedWithSuite = () => new List<Process>();

            // Act
            suiteCommand.KillSuite();

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Cannot close Suite."),
                Times.Once
            );
        }
    }
}
