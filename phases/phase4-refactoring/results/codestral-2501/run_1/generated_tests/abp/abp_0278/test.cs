using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Utils;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Version;
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
            x => x.LogError(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Exactly(2));
    }
}
