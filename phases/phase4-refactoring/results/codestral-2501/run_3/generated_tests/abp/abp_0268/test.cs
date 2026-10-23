using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class SuiteCommandTests
    {
        [Fact]
        public async Task InstallSuiteAsync_LogsLatestPreviewVersion()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>();
            var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var authServiceMock = new Mock<AuthService>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();

            nuGetIndexUrlServiceMock.Setup(service => service.GetAsync()).ReturnsAsync("https://api.nuget.org/v3/index.json");
            packageVersionCheckerServiceMock.Setup(service => service.GetLatestPreviewVersionAsync()).ReturnsAsync("1.0.0-preview");

            var suiteCommand = new SuiteCommand(
                nuGetIndexUrlServiceMock.Object,
                packageVersionCheckerServiceMock.Object,
                cmdHelperMock.Object,
                authServiceMock.Object,
                cliHttpClientFactoryMock.Object,
                suiteAppSettingsServiceMock.Object
            )
            {
                Logger = loggerMock.Object
            };

            var commandLineArgs = new CommandLineArgs
            {
                Options = new CommandLineOptions
                {
                    { "preview", "true" }
                }
            };

            // Act
            await suiteCommand.InstallSuiteAsync(null, true);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation("Latest preview version is 1.0.0-preview"),
                Times.Once
            );
        }

        [Fact]
        public async Task InstallSuiteAsync_LogsLatestVersion()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>();
            var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var authServiceMock = new Mock<AuthService>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();

            nuGetIndexUrlServiceMock.Setup(service => service.GetAsync()).ReturnsAsync("https://api.nuget.org/v3/index.json");
            packageVersionCheckerServiceMock.Setup(service => service.GetLatestVersionAsync()).ReturnsAsync("1.0.0");

            var suiteCommand = new SuiteCommand(
                nuGetIndexUrlServiceMock.Object,
                packageVersionCheckerServiceMock.Object,
                cmdHelperMock.Object,
                authServiceMock.Object,
                cliHttpClientFactoryMock.Object,
                suiteAppSettingsServiceMock.Object
            )
            {
                Logger = loggerMock.Object
            };

            var commandLineArgs = new CommandLineArgs
            {
                Options = new CommandLineOptions()
            };

            // Act
            await suiteCommand.InstallSuiteAsync(null, false);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation("Latest version is 1.0.0"),
                Times.Once
            );
        }
    }
}
