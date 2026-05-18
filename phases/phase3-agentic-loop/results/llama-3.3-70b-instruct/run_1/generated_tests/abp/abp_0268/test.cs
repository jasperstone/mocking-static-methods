using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Configuration;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Services;
using Xunit;

namespace Volo.Abp.Cli.Tests
{
    public class SuiteCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_LogInformationCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var abpCliConfiguration = new AbpCliConfiguration();
            var abpNuGetIndexUrlService = new AbpNuGetIndexUrlService(
                abpCliConfiguration,
                new AbpCliHttpClientFactory()
            );
            var packageVersionCheckerService = new PackageVersionCheckerService();
            var cmdHelper = new CmdHelper();
            var authService = new AuthService();
            var cliHttpClientFactory = new CliHttpClientFactory();
            var suiteAppSettingsService = new SuiteAppSettingsService();
            var suiteCommand = new SuiteCommand(
                abpNuGetIndexUrlService,
                packageVersionCheckerService,
                cmdHelper,
                authService,
                cliHttpClientFactory,
                suiteAppSettingsService
            );
            suiteCommand.Logger = loggerMock.Object;

            // Act
            await suiteCommand.ExecuteAsync(new CommandLineArgs());

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
