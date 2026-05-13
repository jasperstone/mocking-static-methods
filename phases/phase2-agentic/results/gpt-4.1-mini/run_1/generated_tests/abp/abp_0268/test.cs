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
        var nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>();
        var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
        var cmdHelperMock = new Mock<ICmdHelper>();
        var authServiceMock = new Mock<AuthService>();
        var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();

        nuGetIndexUrlServiceMock.Setup(s => s.GetAsync()).ReturnsAsync("http://nuget.index.url");

        var suiteCommand = new SuiteCommand(
            nuGetIndexUrlServiceMock.Object,
            packageVersionCheckerServiceMock.Object,
            cmdHelperMock.Object,
            authServiceMock.Object,
            cliHttpClientFactoryMock.Object,
            suiteAppSettingsServiceMock.Object);

        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        suiteCommand.Logger = loggerMock.Object;

        // Setup GetLatestPreviewVersion to return a version string
        var privateObject = new PrivateObject(suiteCommand);
        privateObject.SetFieldOrProperty("GetLatestPreviewVersion", new Func<Task<string?>>(() => Task.FromResult<string?>("1.2.3-preview")));

        // Setup CmdHelper.RunCmd to simulate success
        cmdHelperMock.Setup(c => c.RunCmd(It.IsAny<string>(), out It.Ref<int>.IsAny))
            .Callback<string, int>((cmd, out int exitCode) => exitCode = 0)
            .Returns(true);

        // Act
        // Call InstallSuiteAsync with preview = true
        var method = typeof(SuiteCommand).GetMethod("InstallSuiteAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        await (Task)method.Invoke(suiteCommand, new object?[] { null, true })!;

        // Assert
        loggerMock.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Latest preview version is 1.2.3-preview")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);

        loggerMock.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ABP Suite has been successfully installed.")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);

        loggerMock.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("You can run it with the CLI command \"abp suite\"")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}
