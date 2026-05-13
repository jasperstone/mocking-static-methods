using System;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class SuiteCommandTests
{
    [Fact]
    public void ShowSuiteManualUpdateCommand_LogsExpectedErrors()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<SuiteCommand>>();
        var suiteCommand = CreateSuiteCommand();
        suiteCommand.Logger = mockLogger.Object;

        // Act
        suiteCommand.InvokeShowSuiteManualUpdateCommand();

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "You can also run the following command to update ABP Suite."),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "dotnet tool update -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json"),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}

internal static class SuiteCommandTestExtensions
{
    public static void InvokeShowSuiteManualUpdateCommand(this SuiteCommand suiteCommand)
    {
        // Use reflection to invoke the private ShowSuiteManualUpdateCommand method
        var method = typeof(SuiteCommand).GetMethod("ShowSuiteManualUpdateCommand", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (method == null)
        {
            throw new InvalidOperationException("ShowSuiteManualUpdateCommand method not found");
        }
        method.Invoke(suiteCommand, null);
    }
    
    public static SuiteCommand CreateSuiteCommand()
    {
        // Create SuiteCommand with null or mock dependencies as needed
        var dummyCmdHelper = new Mock<ICmdHelper>().Object;
        var dummyNuGetIndexUrlService = new Mock<AbpNuGetIndexUrlService>(null).Object;
        var dummyPackageVersionCheckerService = new Mock<PackageVersionCheckerService>(null).Object;
        var dummyAuthService = new Mock<AuthService>(null).Object;
        var dummyCliHttpClientFactory = new Mock<CliHttpClientFactory>(null).Object;
        var dummySuiteAppSettingsService = new Mock<SuiteAppSettingsService>(null).Object;

        var suiteCommand = new SuiteCommand(
            dummyNuGetIndexUrlService,
            dummyPackageVersionCheckerService,
            dummyCmdHelper,
            dummyAuthService,
            dummyCliHttpClientFactory,
            dummySuiteAppSettingsService);

        return suiteCommand;
    }
}
