using Moq;
using System;
using Xunit;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class SuiteCommandTests
    {
        [Fact]
        public async void ExecuteAsync_WhenExceptionOccurs_LogsError()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SuiteCommand>>();
            var mockCmdHelper = new Mock<ICmdHelper>();
            var mockNuGetIndexUrlService = new Mock<AbpNuGetIndexUrlService>();
            var mockPackageVersionCheckerService = new Mock<PackageVersionCheckerService>();
            var mockAuthService = new Mock<AuthService>();
            var mockCliHttpClientFactory = new Mock<CliHttpClientFactory>();
            var mockSuiteAppSettingsService = new Mock<SuiteAppSettingsService>();

            var suiteCommand = new SuiteCommand(
                mockNuGetIndexUrlService.Object,
                mockPackageVersionCheckerService.Object,
                mockCmdHelper.Object,
                mockAuthService.Object,
                mockCliHttpClientFactory.Object,
                mockSuiteAppSettingsService.Object)
            {
                Logger = mockLogger.Object
            };

            var commandLineArgs = new CommandLineArgs();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => suiteCommand.ExecuteAsync(commandLineArgs));

            mockLogger.Verify(
                logger => logger.LogError(
                    It.Is<string>(s => s.Contains("Couldn't update ABP Suite.")),
                    It.IsAny<Exception>()
                ),
                Times.Once
            );
        }
    }
}
