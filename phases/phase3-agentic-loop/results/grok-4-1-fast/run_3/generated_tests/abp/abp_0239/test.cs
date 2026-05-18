using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Volo.Abp.Cli.Utils;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace Volo.Abp.Cli.Commands.Services;

public class DotnetEfToolManagerTests
{
    [Fact]
    public async Task BeSureInstalledAsync_ShouldNotInstall_WhenToolAlreadyInstalled()
    {
        // Arrange
        var cmdHelperMock = new Mock<ICmdHelper>();
        cmdHelperMock
            .Setup(x => x.RunCmdAndGetOutput("dotnet tool list -g"))
            .Returns("dotnet-ef");

        var manager = new DotnetEfToolManager(cmdHelperMock.Object);

        // Act
        await manager.BeSureInstalledAsync();

        // Assert
        cmdHelperMock.Verify(x => x.RunCmd("dotnet tool install --global dotnet-ef"), Times.Never);
    }

    [Fact]
    public async Task BeSureInstalledAsync_ShouldInstall_WhenToolNotInstalled()
    {
        // Arrange
        var cmdHelperMock = new Mock<ICmdHelper>();
        cmdHelperMock
            .Setup(x => x.RunCmdAndGetOutput("dotnet tool list -g"))
            .Returns("no dotnet-ef");

        var manager = new DotnetEfToolManager(cmdHelperMock.Object);

        // Act
        await manager.BeSureInstalledAsync();

        // Assert
        cmdHelperMock.Verify(x => x.RunCmd("dotnet tool install --global dotnet-ef"), Times.Once);
    }

    [Fact]
    public void InstallDotnetEfTool_ShouldExecuteCmdAndLog()
    {
        // Arrange
        var cmdHelperMock = new Mock<ICmdHelper>();
        var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();
        loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        
        var manager = new DotnetEfToolManager(cmdHelperMock.Object);
        manager.Logger = loggerMock.Object;

        // Act
        var method = typeof(DotnetEfToolManager).GetMethod("InstallDotnetEfTool", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method!.Invoke(manager, null);

        // Assert
        cmdHelperMock.Verify(x => x.RunCmd("dotnet tool install --global dotnet-ef"), Times.Once);
        loggerMock.Verify(x => x.IsEnabled(LogLevel.Information), Times.AtLeastOnce);
    }

    [Fact]
    public void IsDotnetEfToolInstalled_ShouldReturnTrue_WhenOutputContainsDotnetEf()
    {
        // Arrange
        var cmdHelperMock = new Mock<ICmdHelper>();
        cmdHelperMock
            .Setup(x => x.RunCmdAndGetOutput("dotnet tool list -g"))
            .Returns("dotnet-ef 7.0.0");

        var manager = new DotnetEfToolManager(cmdHelperMock.Object);

        // Act
        var method = typeof(DotnetEfToolManager).GetMethod("IsDotnetEfToolInstalled", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = (bool)method!.Invoke(manager, null)!;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsDotnetEfToolInstalled_ShouldReturnFalse_WhenOutputDoesNotContainDotnetEf()
    {
        // Arrange
        var cmdHelperMock = new Mock<ICmdHelper>();
        cmdHelperMock
            .Setup(x => x.RunCmdAndGetOutput("dotnet tool list -g"))
            .Returns("other tools");

        var manager = new DotnetEfToolManager(cmdHelperMock.Object);

        // Act
        var method = typeof(DotnetEfToolManager).GetMethod("IsDotnetEfToolInstalled", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var result = (bool)method!.Invoke(manager, null)!;

        // Assert
        Assert.False(result);
    }
}
