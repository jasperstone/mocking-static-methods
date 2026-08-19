using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Commands.Tests;

public class SuiteCommandTests
{
    private readonly Mock<ILogger<SuiteCommand>> _mockLogger;
    private readonly Mock<ICmdHelper> _mockCmdHelper;

    public SuiteCommandTests()
    {
        _mockLogger = new Mock<ILogger<SuiteCommand>>();
        _mockCmdHelper = new Mock<ICmdHelper>();
    }

    [Fact]
    public void ShowSuiteManualInstallCommand_Should_LogInformation_WithCorrectMessage()
    {
        // Arrange
        var suiteCommand = CreateSuiteCommand();

        // Act - Use reflection to call private method (line 333 target)
        var method = typeof(SuiteCommand).GetMethod("ShowSuiteManualInstallCommand", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        method!.Invoke(suiteCommand, null);

        // Assert - Verify LogInformation extension call on line 333
        _mockLogger.Verify(
            x => x.LogInformation(
                "dotnet tool install -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json"),
            Times.Once);
    }

    [Fact]
    public void InstallSuiteAsync_SuccessPath_Should_Log_SuccessMessages()
    {
        // Arrange
        int exitCode = 1; // default non-zero
        _mockCmdHelper.Setup(x => x.RunCmd(It.IsAny<string>(), out exitCode))
            .Callback<string, int>((cmd, refExitCode) => refExitCode = 0);

        var suiteCommand = CreateSuiteCommand();

        // Act - Use reflection to call private InstallSuiteAsync success path
        var method = typeof(SuiteCommand).GetMethod("InstallSuiteAsync", 
            BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(string), typeof(bool) }, null);
        method!.Invoke(suiteCommand, new object?[] { null, false });

        // Assert - Verify both LogInformation calls in success path
        _mockLogger.Verify(x => x.LogInformation("ABP Suite has been successfully installed."), Times.Once);
        _mockLogger.Verify(x => x.LogInformation("You can run it with the CLI command \"abp suite\""), Times.Once);
    }

    private SuiteCommand CreateSuiteCommand()
    {
        // Create mocks only for accessible dependencies
        var mockNuGetService = new Mock<object>();
        var mockVersionChecker = new Mock<object>();
        var mockAuthService = new Mock<object>();
        var mockHttpFactory = new Mock<object>();
        var mockSettingsService = new Mock<object>();

        var command = new SuiteCommand(
            (dynamic)mockNuGetService.Object,
            (dynamic)mockVersionChecker.Object,
            _mockCmdHelper.Object,
            (dynamic)mockAuthService.Object,
            (dynamic)mockHttpFactory.Object,
            (dynamic)mockSettingsService.Object
        );

        command.Logger = _mockLogger.Object;
        return command;
    }
}
