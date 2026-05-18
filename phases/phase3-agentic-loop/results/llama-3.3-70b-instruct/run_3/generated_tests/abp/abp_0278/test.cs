using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogErrorCalled_WhenExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var suiteCommand = new SuiteCommand(
                new AbpNuGetIndexUrlService(),
                new PackageVersionCheckerService(),
                cmdHelperMock.Object,
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
            catch (Exception)
            {
                // Expected
            }

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.Once);
        }
    }
}
