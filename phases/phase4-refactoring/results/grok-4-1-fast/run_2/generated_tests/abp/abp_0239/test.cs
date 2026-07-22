using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
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
            .Returns("dotnet-ef 8.0.0");

        var manager = new DotnetEfToolManager(cmdHelperMock.Object);

        // Act
        await manager.BeSureInstalledAsync();

        // Assert
        cmdHelperMock.Verify(x => x.RunCmd(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task BeSureInstalledAsync_ShouldInstall_WhenToolNotInstalled()
    {
        // Arrange
        var cmdHelperMock = new Mock<ICmdHelper>();
        cmdHelperMock
            .Setup(x => x.RunCmdAndGetOutput("dotnet tool list -g"))
            .Returns("no dotnet-ef");

        cmdHelperMock
            .Setup(x => x.RunCmd("dotnet tool install --global dotnet-ef"))
            .Verifiable();

        var manager = new DotnetEfToolManager(cmdHelperMock.Object);
        manager.Logger = Mock.Of<ILogger<DotnetEfToolManager>>();

        // Act
        await manager.BeSureInstalledAsync();

        // Assert
        cmdHelperMock.Verify();
    }

    [Fact]
    public void InstallDotnetEfTool_ShouldLogInformationMessages()
    {
        // Arrange
        var cmdHelperMock = new Mock<ICmdHelper>();
        var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();
        
        loggerMock.Setup(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Installing dotnet-ef tool...")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ));

        loggerMock.Setup(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("dotnet-ef tool is installed.")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ));

        var manager = new DotnetEfToolManager(cmdHelperMock.Object);
        manager.Logger = loggerMock.Object;

        // Act - call private method via reflection to specifically test line 37
        var method = typeof(DotnetEfToolManager).GetMethod("InstallDotnetEfTool", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        method.Invoke(manager, null);

        // Assert
        loggerMock.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Installing dotnet-ef tool...")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ), Times.Once);

        loggerMock.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("dotnet-ef tool is installed.")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ), Times.Once);

        cmdHelperMock.Verify(x => x.RunCmd("dotnet tool install --global dotnet-ef"), Times.Once);
    }
}
