using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Commands.Services.Tests;

public class DotnetEfToolManagerTests
{
    [Fact]
    public async Task BeSureInstalledAsync_WhenToolAlreadyInstalled_ShouldNotCallInstall()
    {
        // Arrange
        var mockCmdHelper = new Mock<ICmdHelper>();
        mockCmdHelper.Setup(x => x.RunCmdAndGetOutput("dotnet tool list -g")).Returns("dotnet-ef");
        var mockLogger = new Mock<ILogger<DotnetEfToolManager>>();
        var manager = new DotnetEfToolManager(mockCmdHelper.Object);
        manager.Logger = mockLogger.Object;

        // Act
        await manager.BeSureInstalledAsync();

        // Assert
        mockCmdHelper.Verify(x => x.RunCmd(It.Is<string>(cmd => cmd.Contains("dotnet tool install --global dotnet-ef")), null), Times.Never);
    }

    [Fact]
    public async Task BeSureInstalledAsync_WhenToolNotInstalled_ShouldCallInstall()
    {
        // Arrange
        var mockCmdHelper = new Mock<ICmdHelper>();
        mockCmdHelper.Setup(x => x.RunCmdAndGetOutput("dotnet tool list -g")).Returns("no dotnet-ef");
        var mockLogger = new Mock<ILogger<DotnetEfToolManager>>();
        var manager = new DotnetEfToolManager(mockCmdHelper.Object);
        manager.Logger = mockLogger.Object;

        // Act
        await manager.BeSureInstalledAsync();

        // Assert
        mockCmdHelper.Verify(x => x.RunCmd(It.Is<string>(cmd => cmd.Contains("dotnet tool install --global dotnet-ef")), null), Times.Once);
    }

    [Fact]
    public void InstallDotnetEfTool_ShouldLogInformationMessages()
    {
        // Arrange
        var mockCmdHelper = new Mock<ICmdHelper>();
        var mockLogger = new Mock<ILogger<DotnetEfToolManager>>();
        mockLogger.Setup(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Installing dotnet-ef tool...")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ));
        mockLogger.Setup(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("dotnet-ef tool is installed.")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ));
        var manager = new DotnetEfToolManager(mockCmdHelper.Object);
        manager.Logger = mockLogger.Object;

        // Act - Using reflection to call private method for testing Logger.LogInformation call on line 37
        var method = typeof(DotnetEfToolManager).GetMethod("InstallDotnetEfTool", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method!.Invoke(manager, null);

        // Assert
        mockLogger.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Installing dotnet-ef tool...")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ), Times.Once);
        mockLogger.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("dotnet-ef tool is installed.")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ), Times.Once);
    }
}
