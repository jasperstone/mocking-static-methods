using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Http;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class SuiteCommandTests
{
    [Fact]
    public void ShowSuiteManualInstallCommand_LogsExpectedInformation()
    {
        // Arrange
        var nugetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>(MockBehavior.Loose, null);
        var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>(MockBehavior.Loose, null);
        var cmdHelperMock = new Mock<ICmdHelper>();
        var authServiceMock = new Mock<AuthService>(MockBehavior.Loose, null, null);
        var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>(MockBehavior.Loose, null);
        var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>(MockBehavior.Loose, null);

        var suiteCommand = new SuiteCommand(
            nugetIndexUrlServiceMock.Object,
            packageVersionCheckerServiceMock.Object,
            cmdHelperMock.Object,
            authServiceMock.Object,
            cliHttpClientFactoryMock.Object,
            suiteAppSettingsServiceMock.Object
        );

        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        suiteCommand.Logger = loggerMock.Object;

        // Act
        // Call the private method ShowSuiteManualInstallCommand via reflection
        var method = typeof(SuiteCommand).GetMethod("ShowSuiteManualInstallCommand", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(suiteCommand, null);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("You can also run the following command to install ABP Suite.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("dotnet tool install -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
