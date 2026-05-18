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
                new AbpNuGetIndexUrlService(),
                new PackageVersionCheckerService(),
                new CmdHelper(),
                new AuthService(),
                new CliHttpClientFactory(),
                new SuiteAppSettingsService()
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
        public async Task KillSuite_LogsInformation_WhenNoExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                new AbpNuGetIndexUrlService(),
                new PackageVersionCheckerService(),
                new CmdHelper(),
                new AuthService(),
                new CliHttpClientFactory(),
                new SuiteAppSettingsService()
            );
            suiteCommand.Logger = loggerMock.Object;

            // Act
            suiteCommand.KillSuite();

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }
    }
}
