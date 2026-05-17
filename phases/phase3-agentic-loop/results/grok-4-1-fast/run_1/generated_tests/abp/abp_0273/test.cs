using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class SuiteCommandTests
{
    [Fact]
    public void ShowSuiteManualInstallCommand_ShouldLogManualInstallMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<SuiteCommand>>();
        mockLogger.Setup(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()));

        // Create SuiteCommand with minimal dependencies
        var command = new SuiteCommand(
            new Mock<AbpNuGetIndexUrlService>(Mock.Of<ILogger<AbpNuGetIndexUrlService>>>()).Object,
            new Mock<PackageVersionCheckerService>().Object,
            new Mock<ICmdHelper>().Object,
            new Mock<AuthService>().Object,
            new Mock<CliHttpClientFactory>().Object,
            new Mock<SuiteAppSettingsService>().Object);

        command.Logger = mockLogger.Object;

        // Use reflection to call private method (line ~333 in source)
        var method = typeof(SuiteCommand).GetMethod("ShowSuiteManualInstallCommand", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        // Act
        method?.Invoke(command, null);

        // Assert - verify LogInformation call with specific message content
        mockLogger.Verify(x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("dotnet tool install -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
