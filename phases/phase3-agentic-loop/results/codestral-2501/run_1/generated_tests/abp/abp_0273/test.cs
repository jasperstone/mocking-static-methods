using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Args;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Utils;
using Xunit;

public class SuiteCommandTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldLogSuccessMessage_WhenExitCodeIsZero()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<SuiteCommand>>();
        var mockCmdHelper = new Mock<ICmdHelper>();
        var suiteCommand = new SuiteCommand(
            null, null, mockCmdHelper.Object, null, null, null)
        {
            Logger = mockLogger.Object
        };

        mockCmdHelper.Setup(x => x.RunCmd(
            It.IsAny<string>(),
            out It.Ref<int>.IsAny))
            .Callback((string cmd, out int exitCode) => exitCode = 0);

        var commandLineArgs = new CommandLineArgs
        {
            Target = "install"
        };

        // Act
        await suiteCommand.ExecuteAsync(commandLineArgs);

        // Assert
        mockLogger.Verify(
            x => x.LogInformation(
                It.Is<string>(s => s.Contains("ABP Suite has been successfully installed."))),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogErrorMessage_WhenExceptionIsThrown()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<SuiteCommand>>();
        var mockCmdHelper = new Mock<ICmdHelper>();
        var suiteCommand = new SuiteCommand(
            null, null, mockCmdHelper.Object, null, null, null)
        {
            Logger = mockLogger.Object
        };

        mockCmdHelper.Setup(x => x.RunCmd(
            It.IsAny<string>(),
            out It.Ref<int>.IsAny))
            .Throws(new Exception("Test exception"));

        var commandLineArgs = new CommandLineArgs
        {
            Target = "install"
        };

        // Act
        await suiteCommand.ExecuteAsync(commandLineArgs);

        // Assert
        mockLogger.Verify(
            x => x.LogError(
                It.Is<string>(s => s.Contains("Couldn't install ABP Suite."))),
            Times.Once);
    }

    [Fact]
    public void ShowSuiteManualInstallCommand_ShouldLogManualInstallCommand()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<SuiteCommand>>();
        var suiteCommand = new SuiteCommand(
            null, null, null, null, null, null)
        {
            Logger = mockLogger.Object
        };

        // Act
        suiteCommand.ShowSuiteManualInstallCommand();

        // Assert
        mockLogger.Verify(
            x => x.LogInformation(
                It.Is<string>(s => s.Contains("dotnet tool install -g Volo.Abp.Suite --add-source https://nuget.abp.io/<your-private-key>/v3/index.json"))),
            Times.Once);
    }
}
