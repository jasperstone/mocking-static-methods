using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Version;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.Volo.Abp.Cli.Commands
{
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

            // Setup CmdHelper.RunCmd to simulate success (exitCode = 0).
            cmdHelperMock.Setup(c => c.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny))
                .Callback(new RunCmdCallback((string cmd, out int exitCode) => { exitCode = 0; }))
                .Returns(true);

            // Act
            var method = typeof(SuiteCommand).GetMethod("InstallSuiteAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task)method.Invoke(suiteCommand, new object[] { null, true });
            await task;

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Latest preview version is")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);

            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ABP Suite has been successfully installed.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("You can run it with the CLI command \"abp suite\"")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        private delegate void RunCmdCallback(string cmd, out int exitCode);
    }
}
