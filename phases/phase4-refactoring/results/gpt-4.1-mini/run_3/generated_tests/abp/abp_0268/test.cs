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
    public async Task InstallSuiteAsync_LogsInformationOnPreviewVersion()
    {
        // Arrange
        var nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>();
        var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
        var cmdHelperMock = new Mock<ICmdHelper>();
        var authServiceMock = new Mock<AuthService>();
        var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();

        nuGetIndexUrlServiceMock.Setup(s => s.GetAsync()).ReturnsAsync("http://nuget.index.url");

        var loggerMock = new Mock<ILogger<SuiteCommand>>();

        var suiteCommand = new SuiteCommand(
            nuGetIndexUrlServiceMock.Object,
            packageVersionCheckerServiceMock.Object,
            cmdHelperMock.Object,
            authServiceMock.Object,
            cliHttpClientFactoryMock.Object,
            suiteAppSettingsServiceMock.Object)
        {
            Logger = loggerMock.Object
        };

        // Act
        // Call InstallSuiteAsync via reflection to avoid Moq issues on non-virtual methods
        var method = typeof(SuiteCommand).GetMethod("InstallSuiteAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var task = (Task)method.Invoke(suiteCommand, new object[] { null, true });
        await task.ConfigureAwait(false);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Latest preview version")),
                null,
                It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
            Times.AtLeastOnce);
    }
}
