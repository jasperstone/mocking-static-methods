using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;
using Microsoft.Extensions.Logging;

namespace Volo.Abp.Cli.Commands.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public void KillSuite_LogsInformation_WhenProcessesAreKilled()
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

            suiteCommand.GetProcessesRelatedWithSuite = () => new List<Process>
            {
                new Mock<Process>().Object { ProcessName = "abp-suite" },
                new Mock<Process>().Object { ProcessName = "abp-suite" }
            };

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
