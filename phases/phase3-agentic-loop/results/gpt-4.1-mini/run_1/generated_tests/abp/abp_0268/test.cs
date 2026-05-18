using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Version;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands
{
    public class SuiteCommandTests
    {
        [Fact]
        public async Task InstallSuiteAsync_LogsInformationIncludingLatestPreviewVersion()
        {
            // Arrange
            var nugetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>(null, null);
            var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>(null, null);
            var cmdHelperMock = new Mock<ICmdHelper>();
            var authServiceMock = new Mock<AuthService>(null, null);
            var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>(null);
            var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>(null);

            var loggerMock = new Mock<ILogger<SuiteCommand>>();

            var suiteCommand = new TestSuiteCommand(
                nugetIndexUrlServiceMock.Object,
                packageVersionCheckerServiceMock.Object,
                cmdHelperMock.Object,
                authServiceMock.Object,
                cliHttpClientFactoryMock.Object,
                suiteAppSettingsServiceMock.Object,
                loggerMock.Object
            );

            // Setup _nuGetIndexUrlService.GetAsync to return a non-null string
            nugetIndexUrlServiceMock.Setup(s => s.GetAsync()).ReturnsAsync("http://nuget.index.url");

            // Setup CmdHelper.RunCmd to simulate success (exitCode 0)
            cmdHelperMock.Setup(c => c.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny))
                .Callback<string, int>((string cmd, out int exitCode) => { exitCode = 0; })
                .Returns(true);

            // Act
            await suiteCommand.InvokeInstallSuiteAsync(version: null, preview: true);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Latest preview version is 1.2.3-preview")),
                null,
                It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ABP Suite has been successfully installed.")),
                null,
                It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("You can run it with the CLI command \"abp suite\"")),
                null,
                It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }

        private class TestSuiteCommand : SuiteCommand
        {
            private readonly ILogger<SuiteCommand> _logger;

            public TestSuiteCommand(
                AbpNuGetIndexUrlService nuGetIndexUrlService,
                PackageVersionCheckerService packageVersionCheckerService,
                ICmdHelper cmdHelper,
                AuthService authService,
                CliHttpClientFactory cliHttpClientFactory,
                SuiteAppSettingsService suiteAppSettingsService,
                ILogger<SuiteCommand> logger)
                : base(nuGetIndexUrlService, packageVersionCheckerService, cmdHelper, authService, cliHttpClientFactory, suiteAppSettingsService)
            {
                _logger = logger;
                Logger = _logger;
            }

            public Task InvokeInstallSuiteAsync(string version, bool preview)
            {
                return InstallSuiteAsync(version, preview);
            }

            // Provide a new method with the same name as private method to override behavior for testing
            protected Task<string?> GetLatestPreviewVersion()
            {
                return Task.FromResult<string?>("1.2.3-preview");
            }
        }
    }
}
