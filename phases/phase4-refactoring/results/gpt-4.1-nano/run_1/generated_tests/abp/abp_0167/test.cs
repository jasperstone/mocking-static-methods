using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands;
using Volo.Abp.Cli.Args;

public class CleanCommandTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldLogInformationAndRunCmd()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<CleanCommand>>();
        var mockCmdHelper = new Mock<IVirtualCmdHelper>();
        var mockTelemetryService = new Mock<ITelemetryService>();
        var mockTelemetryScope = new Mock<IAsyncDisposable>();

        mockTelemetryService.Setup(s => s.TrackActivityAsync(It.IsAny<string>()))
            .ReturnsAsync(mockTelemetryScope.Object);

        var command = new CleanCommand(mockCmdHelper.Object, mockTelemetryService.Object);
        command.Logger = mockLogger.Object;

        // Act
        await command.ExecuteAsync(new CommandLineArgs());

        // Assert
        mockLogger.Verify(l => l.LogInformation("Cleaning the solution with 'dotnet clean' command..."), Times.Once);
        mockCmdHelper.Verify(c => c.RunCmd("dotnet clean", It.IsAny<string>()), Times.Once);
        mockLogger.Verify(l => l.LogInformation("Solution cleaned successfully!"), Times.Once);
    }

    [Fact]
    public void GetUsageInfo_ShouldContainUsageDetails()
    {
        // Arrange
        var command = new CleanCommand(Mock.Of<IVirtualCmdHelper>(), Mock.Of<ITelemetryService>());

        // Act
        var usageInfo = command.GetUsageInfo();

        // Assert
        Assert.Contains("abp clean", usageInfo);
        Assert.Contains("https://abp.io/docs/latest/cli", usageInfo);
    }

    [Fact]
    public void GetShortDescription_ShouldReturnDescription()
    {
        // Arrange
        var description = CleanCommand.GetShortDescription();

        // Assert
        Assert.Equal("Delete all BIN and OBJ folders in current folder.", description);
    }
}
