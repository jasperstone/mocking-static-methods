using Moq;
using System;
using System.Reflection;
using Xunit;
using Volo.Abp.Cli.Commands;
using Microsoft.Extensions.Logging;

public class SuiteCommandTests
{
    [Fact]
    public void StartSuite_LogsError_WhenPortIsAlreadyInUse()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var nuGetIndexUrlServiceMock = new Mock<AbpNuGetIndexUrlService>();
        var packageVersionCheckerServiceMock = new Mock<PackageVersionCheckerService>();
        var cmdHelperMock = new Mock<ICmdHelper>();
        var authServiceMock = new Mock<AuthService>();
        var cliHttpClientFactoryMock = new Mock<CliHttpClientFactory>();
        var suiteAppSettingsServiceMock = new Mock<SuiteAppSettingsService>();

        var suiteCommand = new SuiteCommand(
            nuGetIndexUrlServiceMock.Object,
            packageVersionCheckerServiceMock.Object,
            cmdHelperMock.Object,
            authServiceMock.Object,
            cliHttpClientFactoryMock.Object,
            suiteAppSettingsServiceMock.Object
        )
        {
            Logger = loggerMock.Object
        };

        // Mock the IsPortAlreadyInUse method to return true
        var isPortAlreadyInUseProperty = suiteCommand.GetType().GetProperty("IsPortAlreadyInUse", BindingFlags.NonPublic | BindingFlags.Instance);
        isPortAlreadyInUseProperty.SetValue(suiteCommand, () => true);

        // Use reflection to invoke the private StartSuite method
        var startSuiteMethod = suiteCommand.GetType().GetMethod("StartSuite", BindingFlags.NonPublic | BindingFlags.Instance);
        startSuiteMethod.Invoke(suiteCommand, null);

        // Assert
        loggerMock.Verify(
            logger => logger.LogError(It.Is<string>(s => s.Contains("Port \"3000\" is already in use."))),
            Times.Once
        );
    }
}
