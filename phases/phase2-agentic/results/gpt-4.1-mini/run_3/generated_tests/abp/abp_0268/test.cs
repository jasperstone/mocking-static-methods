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
    public async Task InstallSuiteAsync_LogsInformation_WhenPreviewVersionIsAvailable()
    {
        // Arrange
        var nugetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>();
        var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
        var cmdHelperMock = new Mock<ICmdHelper>();
        var authServiceMock = new Mock<AuthService>();
        var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();

        nugetIndexUrlServiceMock.Setup(s => s.GetAsync()).ReturnsAsync("http://nuget.index.url");

        var loggerMock = new Mock<ILogger<SuiteCommand>>();

        var suiteCommand = new SuiteCommand(
            nugetIndexUrlServiceMock.Object,
            packageVersionCheckerServiceMock.Object,
            cmdHelperMock.Object,
            authServiceMock.Object,
            cliHttpClientFactoryMock.Object,
            suiteAppSettingsServiceMock.Object)
        {
            Logger = loggerMock.Object
        };

        // Setup GetLatestPreviewVersion to return a preview version string
        var previewVersion = "1.2.3-preview";
        var getLatestPreviewVersionMethod = typeof(SuiteCommand).GetMethod("GetLatestPreviewVersion", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(getLatestPreviewVersionMethod);
        var getLatestPreviewVersionTask = (Task<string>)getLatestPreviewVersionMethod.Invoke(suiteCommand, null);
        // We cannot directly set the method, so we will mock the method by subclassing or by other means.
        // Instead, we will create a derived class to override the method for testing.

        var testSuiteCommand = new TestSuiteCommand(
            nugetIndexUrlServiceMock.Object,
            packageVersionCheckerServiceMock.Object,
            cmdHelperMock.Object,
            authServiceMock.Object,
            cliHttpClientFactoryMock.Object,
            suiteAppSettingsServiceMock.Object,
            previewVersion)
        {
            Logger = loggerMock.Object
        };

        // Act
        await testSuiteCommand.InvokeInstallSuiteAsync(preview: true, version: null);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Latest preview version is " + previewVersion)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    private class TestSuiteCommand : SuiteCommand
    {
        private readonly string _previewVersion;

        public TestSuiteCommand(
            AbpNuGetIndexUrlService nuGetIndexUrlService,
            PackageVersionCheckerService packageVersionCheckerService,
            ICmdHelper cmdHelper,
            AuthService authService,
            CliHttpClientFactory cliHttpClientFactory,
            SuiteAppSettingsService suiteAppSettingsService,
            string previewVersion)
            : base(nuGetIndexUrlService, packageVersionCheckerService, cmdHelper, authService, cliHttpClientFactory, suiteAppSettingsService)
        {
            _previewVersion = previewVersion;
        }

        protected override Task<string> GetLatestPreviewVersion()
        {
            return Task.FromResult(_previewVersion);
        }

        public async Task InvokeInstallSuiteAsync(bool preview, string version)
        {
            // Call the private InstallSuiteAsync method via reflection
            var method = typeof(SuiteCommand).GetMethod("InstallSuiteAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);
            var task = (Task)method.Invoke(this, new object[] { version, preview });
            await task;
        }
    }
}
