using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Http;
using Xunit;

public class SuiteCommandTests
{
    [Fact]
    public void ShowSuiteManualUpdateCommand_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var suiteCommand = new SuiteCommand(
            Mock.Of<AbpNuGetIndexUrlService>(),
            Mock.Of<PackageVersionCheckerService>(),
            Mock.Of<ICmdHelper>(),
            Mock.Of<AuthService>(),
            Mock.Of<CliHttpClientFactory>(),
            Mock.Of<SuiteAppSettingsService>()
        )
        {
            Logger = loggerMock.Object
        };

        // Act
        suiteCommand.ShowSuiteManualUpdateCommand();

        // Assert
        loggerMock.Verify(
            x => x.LogError("You can also run the following command to update ABP Suite."),
            Times.Once()
        );

        loggerMock.Verify(
            x => x.LogError("dotnet tool update -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json"),
            Times.Once()
        );
    }
}
