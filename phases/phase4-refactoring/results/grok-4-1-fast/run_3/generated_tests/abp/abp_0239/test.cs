using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.Commands.Services;

public class DotnetEfToolManagerTests
{
    private readonly Mock<ICmdHelper> _mockCmdHelper;
    private readonly Mock<ILogger<DotnetEfToolManager>> _mockLogger;
    private readonly DotnetEfToolManager _manager;

    public DotnetEfToolManagerTests()
    {
        _mockCmdHelper = new Mock<ICmdHelper>();
        _mockLogger = new Mock<ILogger<DotnetEfToolManager>>();
        _manager = new DotnetEfToolManager(_mockCmdHelper.Object)
        {
            Logger = _mockLogger.Object
        };
    }

    [Fact]
    public async Task BeSureInstalledAsync_ShouldNotInstall_WhenToolAlreadyInstalled()
    {
        // Arrange
        _mockCmdHelper.Setup(x => x.RunCmdAndGetOutput("dotnet tool list -g")).Returns("dotnet-ef");

        // Act
        await _manager.BeSureInstalledAsync();

        // Assert
        _mockCmdHelper.Verify(x => x.RunCmd("dotnet tool install --global dotnet-ef"), Times.Never);
    }

    [Fact]
    public async Task BeSureInstalledAsync_ShouldInstallAndLog_WhenToolNotInstalled()
    {
        // Arrange
        _mockCmdHelper.Setup(x => x.RunCmdAndGetOutput("dotnet tool list -g")).Returns("no dotnet-ef");
        _mockCmdHelper.Setup(x => x.RunCmd("dotnet tool install --global dotnet-ef"));

        // Act
        await _manager.BeSureInstalledAsync();

        // Assert
        _mockCmdHelper.Verify(x => x.RunCmd("dotnet tool install --global dotnet-ef"), Times.Once);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString()!.Contains("Installing dotnet-ef tool...") == true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString()!.Contains("dotnet-ef tool is installed.") == true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task BeSureInstalledAsync_ShouldLogInstallingMessage_WhenToolNotInstalled()
    {
        // Arrange - specifically tests the LogInformation call on line 37
        _mockCmdHelper.Setup(x => x.RunCmdAndGetOutput("dotnet tool list -g")).Returns("no dotnet-ef");
        _mockCmdHelper.Setup(x => x.RunCmd(It.IsAny<string>()));

        // Act
        await _manager.BeSureInstalledAsync();

        // Assert - verifies the specific LogInformation("Installing dotnet-ef tool...") call
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString()!.Contains("Installing dotnet-ef tool...") == true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
