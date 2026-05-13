using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Volo.Abp.Cli.Commands;
using System;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class SuiteCommandTests
    {
        [Fact]
        public void StartSuite_LogsWarning_WhenSuiteIsNotInstalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var globalToolHelperMock = new Mock<GlobalToolHelper>();
            globalToolHelperMock.Setup(gt => gt.IsGlobalToolInstalled("abp-suite")).Returns(false);

            var suiteCommand = new SuiteCommand(
                null, // AbpNuGetIndexUrlService
                null, // PackageVersionCheckerService
                null, // ICmdHelper
                null, // AuthService
                null, // CliHttpClientFactory
                null  // SuiteAppSettingsService
            )
            {
                Logger = loggerMock.Object,
                GlobalToolHelper = globalToolHelperMock.Object
            };

            // Act
            suiteCommand.StartSuite();

            // Assert
            loggerMock.Verify(
                l => l.LogWarning(It.Is<string>(s => s.Contains("ABP Suite is not installed! To install it you can run the command: \"abp suite install\""))),
                Times.Once
            );
        }
    }
}
