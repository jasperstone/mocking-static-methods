using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Version;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class SuiteCommandTests
{
    [Fact]
    public async Task ShowSuiteManualInstallCommand_LogsExpectedInformationMessages()
    {
        // Arrange
        var nugetIndexUrlService = new Mock<AbpNuGetIndexUrlService>(null, null);
        var packageVersionCheckerService = new Mock<PackageVersionCheckerService>(null, null);
        var cmdHelper = new Mock<ICmdHelper>();
        var authService = new Mock<AuthService>(null, null, null);
        var cliHttpClientFactory = new Mock<CliHttpClientFactory>(null, null);
        var suiteAppSettingsService = new Mock<SuiteAppSettingsService>(null);

        var loggerMock = new Mock<ILogger<SuiteCommand>>();

        var suiteCommand = new SuiteCommand(
            nugetIndexUrlService.Object,
            packageVersionCheckerService.Object,
            cmdHelper.Object,
            authService.Object,
            cliHttpClientFactory.Object,
            suiteAppSettingsService.Object
        )
        {
            Logger = loggerMock.Object
        };

        // Act
        // Call the private method ShowSuiteManualInstallCommand via reflection
        var method = typeof(SuiteCommand).GetMethod("ShowSuiteManualInstallCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(method);
        method.Invoke(suiteCommand, null);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "You can also run the following command to install ABP Suite."),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "dotnet tool install -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json"),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
