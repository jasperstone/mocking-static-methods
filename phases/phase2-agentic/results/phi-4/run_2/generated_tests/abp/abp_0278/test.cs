using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Utils;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class SuiteCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
            var authServiceMock = new Mock<AuthService>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();

            var suiteCommand = new SuiteCommand(
                new AbpNuGetIndexUrlService(),
                packageVersionCheckerServiceMock.Object,
                cmdHelperMock.Object,
                authServiceMock.Object,
                cliHttpClientFactoryMock.Object,
                suiteAppSettingsServiceMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => suiteCommand.ExecuteAsync(new CommandLineArgs()));

            loggerMock.Verify(
                x => x.LogError(
                    It.Is<string>(s => s.Contains("Couldn't update ABP Suite.")),
                    It.IsAny<Exception>()
                ),
                Times.Once
            );
        }
    }
}
