using Xunit;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Version;
using Volo.Abp.Cli.Licensing;
using System.Threading.Tasks;

public class SuiteCommandTests
{
    [Fact]
    public async Task InstallSuiteAsync_ShouldLogSuccessMessage_WhenInstallationSucceeds()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<SuiteCommand>>();
        var mockCmdHelper = new Mock<ICmdHelper>();
        var mockNuGetIndexUrlService = new Mock<AbpNuGetIndexUrlService>(new Mock<IApiKeyService>().Object);
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
            mockSuiteAppSettingsService.Object
        )
        {
            Logger = mockLogger.Object
        };

        mockCmdHelper.Setup(x => x.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny))
                     .Callback((string cmd, out int exitCode) => exitCode = 0)
                     .Returns(true);

        // Act
        await suiteCommand.InstallSuiteAsync(null, false);

        // Assert
        mockLogger.Verify(
            x => x.LogInformation("ABP Suite has been successfully installed."),
            Times.Once
        );
        mockLogger.Verify(
            x => x.LogInformation("You can run it with the CLI command \"abp suite\""),
            Times.Once
        );
    }

    [Fact]
    public void ShowSuiteManualInstallCommand_ShouldLogManualInstallCommand()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<SuiteCommand>>();
        var suiteCommand = new SuiteCommand(
            Mock.Of<AbpNuGetIndexUrlService>(),
            Mock.Of<PackageVersionCheckerService>(),
            Mock.Of<ICmdHelper>(),
            Mock.Of<AuthService>(),
            Mock.Of<CliHttpClientFactory>(),
            Mock.Of<SuiteAppSettingsService>()
        )
        {
            Logger = mockLogger.Object
        };

        // Act
        suiteCommand.ShowSuiteManualInstallCommand();

        // Assert
        mockLogger.Verify(
            x => x.LogInformation("You can also run the following command to install ABP Suite."),
            Times.Once
        );
        mockLogger.Verify(
            x => x.LogInformation("dotnet tool install -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json"),
            Times.Once
        );
    }
}
