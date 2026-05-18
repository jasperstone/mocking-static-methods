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
        private class MockProcess : Process
        {
            public string MockProcessName { get; set; }

            public override string ProcessName => MockProcessName;
        }

        [Fact]
        public void KillSuite_LogsInformation_WhenNoException()
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

            var mockProcesses = new List<MockProcess>
            {
                new MockProcess { MockProcessName = "abp-suite" }
            };

            suiteCommand.GetProcessesRelatedWithSuite = () => mockProcesses;

            // Act
            suiteCommand.KillSuite();

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Suite closed."),
                Times.Once
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

            var mockProcesses = new List<MockProcess>
            {
                new MockProcess { MockProcessName = "abp-suite" }
            };

            suiteCommand.GetProcessesRelatedWithSuite = () => mockProcesses;

            // Act
            suiteCommand.KillSuite();

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(It.Is<string>(s => s.StartsWith("Cannot close Suite."))),
                Times.Once
            );
        }
    }
}
