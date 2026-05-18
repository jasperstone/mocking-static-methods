using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Cli.Version;

public class SuiteCommandTests
{
    public class TestableSuiteCommand : SuiteCommand
    {
        public TestableSuiteCommand(
            AbpNuGetIndexUrlService nuGetIndexUrlService,
            PackageVersionCheckerService packageVersionCheckerService,
            ICmdHelper cmdHelper,
            AuthService authService,
            CliHttpClientFactory cliHttpClientFactory,
            SuiteAppSettingsService suiteAppSettingsService)
            : base(nuGetIndexUrlService, packageVersionCheckerService, cmdHelper, authService, cliHttpClientFactory, suiteAppSettingsService)
        {
        }

        protected override void InstallSuite(string version, bool preview)
        {
            base.InstallSuite(version, preview);
        }
    }

    [Fact]
    public void InstallSuite_WhenExitCodeIsZero_LogsSuccessMessages()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var cmdHelperMock = new Mock<ICmdHelper>();
        cmdHelperMock.Setup(cmd => cmd.RunCmd(It.IsAny<string>(), out int exitCode))
            .Callback((string command, out int exitCode) => exitCode = 0);

        var suiteCommand = new TestableSuiteCommand(
            null, // AbpNuGetIndexUrlService
            null, // PackageVersionCheckerService
            cmdHelperMock.Object, // ICmdHelper
            null, // AuthService
            null, // CliHttpClientFactory
            null  // SuiteAppSettingsService
        )
        {
            Logger = loggerMock.Object
        };

        // Act
        suiteCommand.InstallSuite(null, false);

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
