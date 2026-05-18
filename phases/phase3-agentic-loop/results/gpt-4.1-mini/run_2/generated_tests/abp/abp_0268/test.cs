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

namespace Volo.Abp.Cli.UnitTests.Commands
{
    public class SuiteCommandTests
    {
        [Fact]
        public async Task InstallSuiteAsync_LogsLatestPreviewVersion_WhenPreviewIsTrueAndLatestPreviewVersionIsNotNull()
        {
            // Arrange
            var nugetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>();
            var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
            var cmdHelperMock = new Mock<ICmdHelper>();
            var authServiceMock = new Mock<AuthService>();
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
            var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();

            var loggerMock = new Mock<ILogger<SuiteCommand>>();

            var suiteCommand = new SuiteCommand(
                nugetIndexUrlServiceMock.Object,
                packageVersionCheckerServiceMock.Object,
                cmdHelperMock.Object,
                authServiceMock.Object,
                cliHttpClientFactoryMock.Object,
                suiteAppSettingsServiceMock.Object
            );

            suiteCommand.Logger = loggerMock.Object;

            // Setup for _nuGetIndexUrlService.GetAsync to return a non-null string
            nugetIndexUrlServiceMock.Setup(x => x.GetAsync()).ReturnsAsync("http://nuget.index.url");

            // Setup CmdHelper.RunCmd to simulate success (exitCode 0)
            int exitCode = 0;
            cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>(), out exitCode)).Returns(true);

            // We cannot override private methods, so we simulate by reflection invoking the private method InstallSuiteAsync
            // But since it is private, we create a helper derived class with a public method to call it

            var testSuiteCommand = new TestSuiteCommand(
                nugetIndexUrlServiceMock.Object,
                packageVersionCheckerServiceMock.Object,
                cmdHelperMock.Object,
                authServiceMock.Object,
                cliHttpClientFactoryMock.Object,
                suiteAppSettingsServiceMock.Object,
                "1.2.3-preview"
            );
            testSuiteCommand.Logger = loggerMock.Object;

            // Act
            await testSuiteCommand.InvokeInstallSuiteAsync(version: null, preview: true);

            // Assert
            loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Latest preview version is 1.2.3-preview")),
                null,
                It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
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
            public async Task InvokeInstallSuiteAsync(string version, bool preview)
            {
                // We use reflection to call the private method InstallSuiteAsync
                var method = typeof(SuiteCommand).GetMethod("InstallSuiteAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method == null)
                {
                    throw new System.Exception("InstallSuiteAsync method not found");
                }
                var task = (Task)method.Invoke(this, new object[] { version, preview });
                await task;
            }

            // Shadow the private GetLatestPreviewVersion method by new method with same name
            private new Task<string> GetLatestPreviewVersion()
            {
                return Task.FromResult(_latestPreviewVersion);
            }
        }
    }
}
