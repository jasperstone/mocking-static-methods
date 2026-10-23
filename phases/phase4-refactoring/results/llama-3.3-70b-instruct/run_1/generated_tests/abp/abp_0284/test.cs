using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public void StartSuite_LogsWarning_WhenAbpSuiteIsNotInstalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>();
            var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
            var authServiceMock = new Mock<AuthService>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();
            var globalToolHelperMock = new Mock<GlobalToolHelper>();

            globalToolHelperMock
                .Setup(x => x.IsGlobalToolInstalled("abp-suite"))
                .Returns(false);

            var suiteCommand = new SuiteCommand(
                nuGetIndexUrlServiceMock.Object,
                packageVersionCheckerServiceMock.Object,
                cmdHelperMock.Object,
                authServiceMock.Object,
                cliHttpClientFactoryMock.Object,
                suiteAppSettingsServiceMock.Object
            );

            suiteCommand.Logger = loggerMock.Object;

            // Act
            var process = suiteCommand.StartSuite();

            // Assert
            loggerMock.Verify(
                logger => logger.LogWarning("ABP Suite is not installed! To install it you can run the command: \"abp suite install\""),
                Times.Once
            );
        }
    }
}
