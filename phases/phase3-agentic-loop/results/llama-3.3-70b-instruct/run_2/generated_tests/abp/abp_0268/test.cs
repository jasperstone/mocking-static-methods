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
        public async Task ExecuteAsync_InstallSuiteIfNotInstalledAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var nuGetIndexUrlServiceMock = new Mock<IAbpNuGetIndexUrlService>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var suiteAppSettingsServiceMock = new Mock<ISuiteAppSettingsService>();
            var authServiceMock = new Mock<IAuthService>();
            var cliHttpClientFactoryMock = new Mock<ICliHttpClientFactory>();
            var packageVersionCheckerServiceMock = new Mock<IPackageVersionCheckerService>();

            var suiteCommand = new SuiteCommand(
                nuGetIndexUrlServiceMock.Object,
                packageVersionCheckerServiceMock.Object,
                cmdHelperMock.Object,
                authServiceMock.Object,
                cliHttpClientFactoryMock.Object,
                suiteAppSettingsServiceMock.Object
            );

            suiteCommand.Logger = loggerMock.Object;

            var commandLineArgs = new CommandLineArgs();

            // Act
            await suiteCommand.ExecuteAsync(commandLineArgs);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(It.IsAny<string>()),
                Times.AtLeastOnce
            );
        }
    }
}
