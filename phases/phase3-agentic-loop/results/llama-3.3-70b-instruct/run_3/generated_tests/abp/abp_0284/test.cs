using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Version;
using Volo.Abp.DependencyInjection;

public class SuiteCommandTests
{
    [Fact]
    public void StartSuite_LogsWarning_WhenAbpSuiteIsNotInstalled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>();
        var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
        var cmdHelperMock = new Mock<ICmdHelper>();
        var authServiceMock = new Mock<AuthService>();
        var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();
        var globalToolHelperMock = new Mock<GlobalToolHelper>();
        globalToolHelperMock.Setup(x => x.IsGlobalToolInstalled("abp-suite")).Returns(false);

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
        var result = suiteCommand.StartSuite();

        // Assert
        loggerMock.Verify(x => x.LogWarning("ABP Suite is not installed! To install it you can run the command: \"abp suite install\""), Times.Once);
    }
}
