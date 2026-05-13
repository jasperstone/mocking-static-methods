using System;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Auth;
using Volo.Abp.Cli.Http;
using Volo.Abp.Cli.Version;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Http;

public class SuiteCommandTests
{
    [Fact]
    public void LogError_ShouldBeCalled_WhenExceptionOccurs()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>();
        var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
        var authServiceMock = new Mock<AuthService>();
        var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();
        var cmdHelperMock = new Mock<ICmdHelper>();

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
        var exception = new Exception("Test exception");
        try
        {
            throw exception;
        }
        catch (Exception ex)
        {
            suiteCommand.Logger.LogError("Couldn't update ABP Suite." + ex.Message);
            suiteCommand.ShowSuiteManualUpdateCommand();
        }

        // Assert
        loggerMock.Verify(
            logger => logger.LogError(
                It.Is<string>(s => s.Contains("Couldn't update ABP Suite.") && s.Contains(exception.Message)),
                null,
                It.IsAny<Exception>()
            ),
            Times.Once
        );
    }
}
