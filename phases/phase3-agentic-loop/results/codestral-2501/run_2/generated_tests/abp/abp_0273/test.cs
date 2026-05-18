using Xunit;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Cli.Args;
using System.Threading.Tasks;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Http;

public class SuiteCommandTests
{
    [Fact]
    public async Task InstallSuiteAsync_ShouldLogInformation_WhenInstallationSucceeds()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var cmdHelperMock = new Mock<ICmdHelper>();
        var nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>();
        var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
        var authServiceMock = new Mock<AuthService>();
        var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();

        nuGetIndexUrlServiceMock.Setup(x => x.GetAsync()).ReturnsAsync("https://nuget.abp.io/v3/index.json");
        cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny)).Returns(true);

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

        var commandLineArgs = new CommandLineArgs("suite install");

        // Act
        await suiteCommand.ExecuteAsync(commandLineArgs);

        // Assert
        loggerMock.Verify(
            x => x.LogInformation("ABP Suite has been successfully installed."),
            Times.Once
        );
        loggerMock.Verify(
            x => x.LogInformation("You can run it with the CLI command \"abp suite\""),
            Times.Once
        );
    }
}
