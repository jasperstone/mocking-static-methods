using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands.Services;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests.Commands.Services;

public class DotnetEfToolManagerTests
{
    [Fact]
    public async Task BeSureInstalledAsync_Should_Log_Information_When_Installing()
    {
        // Arrange
        var cmdHelperMock = new Mock<ICmdHelper>();
        cmdHelperMock
            .Setup(h => h.RunCmdAndGetOutput("dotnet tool list -g"))
            .Returns("some-other-tool");
        cmdHelperMock
            .Setup(h => h.RunCmd("dotnet tool install --global dotnet-ef"));

        var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();

        var manager = new DotnetEfToolManager(cmdHelperMock.Object)
        {
            Logger = loggerMock.Object
        };

        // Act
        await manager.BeSureInstalledAsync();

        // Assert
        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) =>
                    state.ToString() == "Installing dotnet-ef tool..."),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
