using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Commands.Services;

public class DotnetEfToolManagerTests
{
    [Fact]
    public async Task Should_LogInformation_When_Installing_DotnetEfTool()
    {
        // Arrange
        var cmdHelperMock = new Mock<ICmdHelper>();
        cmdHelperMock
            .Setup(x => x.RunCmdAndGetOutput("dotnet tool list -g"))
            .Returns("some output without dotnet-ef");

        cmdHelperMock
            .Setup(x => x.RunCmd("dotnet tool install --global dotnet-ef"))
            .Verifiable();

        var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();
        loggerMock
            .Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Installing dotnet-ef tool...")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

        loggerMock
            .Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("dotnet-ef tool is installed.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

        var manager = new DotnetEfToolManager(cmdHelperMock.Object)
        {
            Logger = loggerMock.Object
        };

        // Act
        await manager.BeSureInstalledAsync();

        // Assert
        cmdHelperMock.Verify();
        loggerMock.Verify();
    }

    [Fact]
    public async Task Should_Not_Call_Install_When_DotnetEfTool_Is_Already_Installed()
    {
        // Arrange
        var cmdHelperMock = new Mock<ICmdHelper>();
        cmdHelperMock
            .Setup(x => x.RunCmdAndGetOutput("dotnet tool list -g"))
            .Returns("dotnet-ef");

        var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();
        var manager = new DotnetEfToolManager(cmdHelperMock.Object)
        {
            Logger = loggerMock.Object
        };

        // Act
        await manager.BeSureInstalledAsync();

        // Assert
        cmdHelperMock.Verify(x => x.RunCmd("dotnet tool install --global dotnet-ef"), Times.Never);
        loggerMock.Verify(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
    }
}
