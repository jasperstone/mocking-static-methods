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
    public class SuiteCommandTest
    {
        [Fact]
        public async Task InstallSuiteAsync_Should_Log_Preview_Version()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SuiteCommand>>();
            var mockNuGetService = new Mock<AbpNuGetIndexUrlService>();
            var mockPackageChecker = new Mock<PackageVersionCheckerService>();
            var mockCmdHelper = new Mock<ICmdHelper>();
            var mockAuthService = new Mock<AuthService>();
            var mockHttpClientFactory = new Mock<CliHttpClientFactory>();
            var mockSuiteSettings = new Mock<SuiteAppSettingsService>();

            var suiteCommand = new SuiteCommand(
                mockNuGetService.Object,
                mockPackageChecker.Object,
                mockCmdHelper.Object,
                mockAuthService.Object,
                mockHttpClientFactory.Object,
                mockSuiteSettings.Object
            );

            suiteCommand.Logger = mockLogger.Object;

            // Setup for the method to test
            var args = new CommandLineArgs
            {
                Target = "install",
                Options = new System.Collections.Generic.Dictionary<string, string>
                {
                    { Options.Preview.Short, "true" }
                }
            };

            // Act
            await suiteCommand.ExecuteAsync(args);

            // Assert
            mockLogger.Verify(
                x => x.LogInformation(It.Is<string>(s => s.Contains("Latest preview version is"))),
                Times.AtLeastOnce);
        }
    }
}
