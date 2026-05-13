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
            var mockLogger = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                null, // AbpNuGetIndexUrlService
                null, // PackageVersionCheckerService
                null, // ICmdHelper
                null, // AuthService
                null, // CliHttpClientFactory
                null  // SuiteAppSettingsService
            )
            {
                Logger = mockLogger.Object
            };

            var mockProcess1 = new Mock<Process>();
            var mockProcess2 = new Mock<Process>();
            mockProcess1.Setup(p => p.ProcessName).Returns("abp-suite");
            mockProcess2.Setup(p => p.ProcessName).Returns("abp-suite");

            suiteCommand.GetProcessesRelatedWithSuite = () => new List<Process> { mockProcess1.Object, mockProcess2.Object };

            // Act
            suiteCommand.KillSuite();

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation("Suite closed."),
                Times.Exactly(2)
            );
        }

        [Fact]
        public void KillSuite_LogsInformation_WhenExceptionOccurs()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                null, // AbpNuGetIndexUrlService
                null, // PackageVersionCheckerService
                null, // ICmdHelper
                null, // AuthService
                null, // CliHttpClientFactory
                null  // SuiteAppSettingsService
            )
            {
                Logger = mockLogger.Object
            };

            suiteCommand.GetProcessesRelatedWithSuite = () => new List<Process>();

            // Act
            suiteCommand.KillSuite();

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation("Cannot close Suite."),
                Times.Once
            );
        }
    }
}
