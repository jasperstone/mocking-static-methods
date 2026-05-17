using Moq;
using System.Net.NetworkInformation;
using Xunit;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Utils;
using Microsoft.Extensions.Logging;
using System.Reflection;

public class SuiteCommandTests
{
    [Fact]
    public void StartSuite_LogsError_WhenPortIsAlreadyInUse()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<SuiteCommand>>();
        var suiteCommand = new SuiteCommand(
            Mock.Of<AbpNuGetIndexUrlService>(),
            Mock.Of<PackageVersionCheckerService>(),
            Mock.Of<ICmdHelper>(),
            Mock.Of<AuthService>(),
            Mock.Of<HttpClientFactory>(),
            Mock.Of<SuiteAppSettingsService>()
        )
        {
            Logger = loggerMock.Object
        };

        // Use reflection to set the private field _abpSuitePort
        var fieldInfo = typeof(SuiteCommand).GetField("_abpSuitePort", BindingFlags.NonPublic | BindingFlags.Instance);
        fieldInfo.SetValue(suiteCommand, 3000);

        // Mock IsPortAlreadyInUse to return true
        var isPortAlreadyInUseMethod = typeof(SuiteCommand).GetMethod("IsPortAlreadyInUse", BindingFlags.NonPublic | BindingFlags.Instance);
        isPortAlreadyInUseMethod.Invoke(suiteCommand, null);

        // Act
        suiteCommand.StartSuite();

        // Assert
        loggerMock.Verify(
            l => l.LogError(It.Is<string>(s => s.Contains("Port \"3000\" is already in use."))),
            Times.Once
        );
    }
}
