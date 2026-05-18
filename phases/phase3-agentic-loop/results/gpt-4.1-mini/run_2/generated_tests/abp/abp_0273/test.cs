using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Version;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Cli.Licensing;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class SuiteCommandTests
{
    [Fact]
    public void ShowSuiteManualInstallCommand_LogsExpectedInformation()
    {
        // Arrange
        var apiKeyServiceMock = new Mock<IApiKeyService>();
        var nugetIndexUrlService = new AbpNuGetIndexUrlService(apiKeyServiceMock.Object);

        var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>(
            MockBehavior.Strict,
            new object[] { null, null, null, null, null }
        );
        var cmdHelper = new Mock<ICmdHelper>(MockBehavior.Strict);
        var authService = new Mock<AuthService>(MockBehavior.Strict);
        var cliHttpClientFactory = new Mock<CliHttpClientFactory>(MockBehavior.Strict);
        var suiteAppSettingsService = new Mock<SuiteAppSettingsService>(MockBehavior.Strict);

        var loggerMock = new Mock<ILogger<SuiteCommand>>();

        var suiteCommand = new SuiteCommand(
            nugetIndexUrlService,
            packageVersionCheckerServiceMock.Object,
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
        var methodInfo = typeof(SuiteCommand).GetMethod("ShowSuiteManualInstallCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(methodInfo);
        methodInfo.Invoke(suiteCommand, null);

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
