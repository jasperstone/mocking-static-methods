using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Commands;

namespace Volo.Abp.Cli.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public async Task LogInformation_Called_WithExpectedMessage_When_RemoveIsCalled()
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

            var commandLineArgs = new CommandLineArgs
            {
                Target = "remove",
                Options = new System.Collections.Generic.Dictionary<string, string>()
            };

            // Act
            await suiteCommand.ExecuteAsync(commandLineArgs);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Removing ABP Suite")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
