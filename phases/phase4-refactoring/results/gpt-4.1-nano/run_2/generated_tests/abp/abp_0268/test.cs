using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using System.Net.Http;
using System.Threading;
using System;

namespace SuiteCommandTests
{
    public class SuiteCommandTests
    {
        [Fact]
        public async Task InstallSuiteAsync_Should_LogInformation_With_Preview_Version()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SuiteCommand>>();
            var mockNuGetService = new Mock<AbpNuGetIndexUrlService>();
            var mockPackageChecker = new Mock<PackageVersionCheckerService>();
            var mockCmdHelper = new Mock<ICmdHelper>();
            var mockAuthService = new Mock<AuthService>();
            var mockHttpClientFactory = new Mock<CliHttpClientFactory>();
            var mockSuiteAppSettings = new Mock<SuiteAppSettingsService>();

            var suiteCommand = new SuiteCommand(
                mockNuGetService.Object,
                mockPackageChecker.Object,
                mockCmdHelper.Object,
                mockAuthService.Object,
                mockHttpClientFactory.Object,
                mockSuiteAppSettings.Object
            )
            {
                Logger = mockLogger.Object
            };

            // Setup to simulate version and preview options
            var args = new CommandLineArgs
            {
                Target = "",
                Options = new System.Collections.Generic.Dictionary<string, string>
                {
                    { Options.Preview.Short, "true" }
                }
            };

            // Act
            await suiteCommand.InstallSuiteAsync("1.0.0", preview: true);

            // Assert
            mockLogger.Verify(
                x => x.LogInformation(It.Is<string>(s => s.Contains("Latest preview version is"))),
                Times.AtLeastOnce);
        }
    }
}
