using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class SuiteCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_UpdatesSuite_WhenUpdateCommandIsUsed()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SuiteCommand>>();
            var cmdHelperMock = new Mock<Volo.Abp.Cli.Utils.ICmdHelper>();
            var nuGetIndexUrlServiceMock = new Mock<Volo.Abp.Cli.Services.AbpNuGetIndexUrlService>();
            var packageVersionCheckerServiceMock = new Mock<Volo.Abp.Cli.Services.PackageVersionCheckerService>();
            var authServiceMock = new Mock<Volo.Abp.Cli.Auth.AuthService>();
            var cliHttpClientFactoryMock = new Mock<Volo.Abp.Cli.Http.CliHttpClientFactory>();
            var suiteAppSettingsServiceMock = new Mock<Volo.Abp.Cli.Commands.Services.SuiteAppSettingsService>();

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
            await suiteCommand.ExecuteAsync(new Volo.Abp.Cli.Args.CommandLineArgs { Options = new System.Collections.Generic.Dictionary<string, string>() { { "target", "update" } } });

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
