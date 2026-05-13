using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Version;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class SuiteCommandTests
{
    [Fact]
    public async Task InstallSuiteAsync_LogsInformationOnLatestPreviewVersion()
    {
        // Arrange
        var nugetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>(null, null);
        var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>(null, null);
        var cmdHelperMock = new Mock<ICmdHelper>();
        var authServiceMock = new Mock<AuthService>(null, null);
        var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>(null);
        var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>(null);

        var loggerMock = new Mock<ILogger<SuiteCommand>>();

        var suiteCommand = new SuiteCommand(
            nugetIndexUrlServiceMock.Object,
            packageVersionCheckerServiceMock.Object,
            cmdHelperMock.Object,
            authServiceMock.Object,
            cliHttpClientFactoryMock.Object,
            suiteAppSettingsServiceMock.Object
        )
        {
            Logger = loggerMock.Object
        };

        // Setup _nuGetIndexUrlService.GetAsync to return a non-null string
        nugetIndexUrlServiceMock.Setup(s => s.GetAsync()).ReturnsAsync("http://nuget.index.url");

        // Setup GetLatestPreviewVersion to return a specific version string
        var latestPreviewVersion = "1.2.3-preview";
        var getLatestPreviewVersionMethod = typeof(SuiteCommand).GetMethod("GetLatestPreviewVersion", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(getLatestPreviewVersionMethod);
        getLatestPreviewVersionMethod.Invoke(suiteCommand, null); // Just to ensure method exists

        // We need to mock GetLatestPreviewVersion to return latestPreviewVersion
        // Since it's private, we can create a derived class to override it for testing
        var testSuiteCommand = new TestSuiteCommand(
            nugetIndexUrlServiceMock.Object,
            packageVersionCheckerServiceMock.Object,
            cmdHelperMock.Object,
            authServiceMock.Object,
            cliHttpClientFactoryMock.Object,
            suiteAppSettingsServiceMock.Object,
            latestPreviewVersion
        )
        {
            Logger = loggerMock.Object
        };

        // Setup CmdHelper.RunCmd to simulate success (exitCode 0)
        cmdHelperMock.Setup(c => c.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny))
            .Callback<string, int>((cmd, out int exitCode) => exitCode = 0)
            .Returns(true);

        // Act
        await testSuiteCommand.InvokeInstallSuiteAsync(preview: true, version: null);

        // Assert
        loggerMock.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString() == "Latest preview version is " + latestPreviewVersion),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        loggerMock.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString() == "ABP Suite has been successfully installed."),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        loggerMock.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString() == "You can run it with the CLI command \"abp suite\""),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    private class TestSuiteCommand : SuiteCommand
    {
        private readonly string _latestPreviewVersion;

        public TestSuiteCommand(
            AbpNuGetIndexUrlService nuGetIndexUrlService,
            PackageVersionCheckerService packageVersionCheckerService,
            ICmdHelper cmdHelper,
            AuthService authService,
            CliHttpClientFactory cliHttpClientFactory,
            SuiteAppSettingsService suiteAppSettingsService,
            string latestPreviewVersion)
            : base(nuGetIndexUrlService, packageVersionCheckerService, cmdHelper, authService, cliHttpClientFactory, suiteAppSettingsService)
        {
            _latestPreviewVersion = latestPreviewVersion;
        }

        // Expose the private InstallSuiteAsync method for testing
        public async Task InvokeInstallSuiteAsync(bool preview, string version)
        {
            await InstallSuiteAsync(version, preview);
        }

        // Override the private GetLatestPreviewVersion method to return the test value
        protected override Task<string?> GetLatestPreviewVersion()
        {
            return Task.FromResult<string?>(_latestPreviewVersion);
        }
    }
}
