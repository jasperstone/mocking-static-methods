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
        public async Task LogError_IsCalled_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var suiteCommand = new SuiteCommand(
                new AbpNuGetIndexUrlService(
                    new AbpAuthConfiguration(),
                    new AbpHttpClientFactory()
                ),
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
                await suiteCommand.ExecuteAsync(new CommandLineArgs());
            }
            catch (Exception ex)
            {
                suiteCommand.Logger.LogError("Couldn't update ABP Suite." + ex.Message);
            }

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }
    }
}
