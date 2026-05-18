using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Http;
using Xunit;

public class SuiteCommandTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldLogInformation_WhenPreviewIsTrue()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>();
        var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
        var cmdHelperMock = new Mock<ICmdHelper>();
        var authServiceMock = new Mock<AuthService>();
        var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();

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
            Options = new Dictionary<string, string>
            {
                { "preview", "true" }
            }
        };

        nuGetIndexUrlServiceMock.Setup(x => x.GetAsync()).ReturnsAsync("https://api.nuget.org/v3/index.json");
        packageVersionCheckerServiceMock.Setup(x => x.GetLatestPreviewVersionAsync()).ReturnsAsync("1.0.0-preview");

        // Act
        await suiteCommand.ExecuteAsync(commandLineArgs);

        // Assert
        loggerMock.Verify(
            x => x.LogInformation("Latest preview version is 1.0.0-preview"),
            Times.Once
        );
    }
}
