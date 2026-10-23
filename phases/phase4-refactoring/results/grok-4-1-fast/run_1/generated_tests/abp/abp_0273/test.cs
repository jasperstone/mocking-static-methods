using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands;

public class SuiteCommandTests
{
    private readonly Mock<ICmdHelper> _mockCmdHelper;
    private readonly Mock<ILogger<SuiteCommand>> _mockLogger;

    public SuiteCommandTests()
    {
        _mockCmdHelper = new();
        _mockLogger = new();
    }

    [Fact]
    public void ShowSuiteManualInstallCommand_Should_Log_InstallCommand_On_Line_333()
    {
        // Arrange
        var command = CreateSuiteCommand();

        // Act
        command.ShowSuiteManualInstallCommand();

        // Assert - Verifies Logger.LogInformation call on line 333
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                0,
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("dotnet tool install -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void ShowSuiteManualInstallCommand_Calls_LogInformation_Extension()
    {
        // Arrange
        var command = CreateSuiteCommand();

        // Act
        command.ShowSuiteManualInstallCommand();

        // Assert - Verifies the ILogger extension method LogInformation was called
        _mockLogger.Verify(
            x => x.LogInformation(
                It.Is<string>(s => s.Contains("dotnet tool install -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json"))),
            Times.Once);
    }

    private SuiteCommand CreateSuiteCommand()
    {
        var mockNuGetService = new Mock<object>();
        var mockVersionChecker = new Mock<object>();
        var mockAuth = new Mock<object>();
        var mockHttpFactory = new Mock<object>();
        var mockAppSettings = new Mock<object>();

        var command = new SuiteCommand(
            mockNuGetService.Object,
            mockVersionChecker.Object,
            _mockCmdHelper.Object,
            mockAuth.Object,
            mockHttpFactory.Object,
            mockAppSettings.Object);

        command.Logger = _mockLogger.Object;
        return command;
    }
}
