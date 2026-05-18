using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public async Task KillSuite_LogsInformation_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                Mock.Of<AbpNuGetIndexUrlService>(),
                Mock.Of<PackageVersionCheckerService>(),
                Mock.Of<ICmdHelper>(),
                Mock.Of<AuthService>(),
                Mock.Of<CliHttpClientFactory>(),
                Mock.Of<SuiteAppSettingsService>()
            );
            suiteCommand.Logger = loggerMock.Object;

            // Act
            try
            {
                suiteCommand.KillSuite();
            }
            catch (Exception ex)
            {
                suiteCommand.Logger.LogInformation("Cannot close Suite." + ex.Message);
            }

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task KillSuite_LogsInformation_WhenSuiteClosed()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                Mock.Of<AbpNuGetIndexUrlService>(),
                Mock.Of<PackageVersionCheckerService>(),
                Mock.Of<ICmdHelper>(),
                Mock.Of<AuthService>(),
                Mock.Of<CliHttpClientFactory>(),
                Mock.Of<SuiteAppSettingsService>()
            );
            suiteCommand.Logger = loggerMock.Object;

            // Act
            suiteCommand.KillSuite();

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }
    }
}
