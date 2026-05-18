using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class DotnetEfToolManagerTests
    {
        [Fact]
        public async Task BeSureInstalledAsync_InstallsEfTool_WhenNotInstalled()
        {
            // Arrange
            var cmdHelperMock = new Mock<ICmdHelper>();
            cmdHelperMock.Setup(x => x.RunCmdAndGetOutput("dotnet tool list -g")).Returns(string.Empty);
            cmdHelperMock.Setup(x => x.RunCmd("dotnet tool install --global dotnet-ef")).Verifiable();

            var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();

            var dotnetEfToolManager = new DotnetEfToolManager(cmdHelperMock.Object);
            dotnetEfToolManager.Logger = loggerMock.Object;

            // Act
            await dotnetEfToolManager.BeSureInstalledAsync();

            // Assert
            cmdHelperMock.Verify(x => x.RunCmd("dotnet tool install --global dotnet-ef"), Times.Once);
            loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Installing dotnet-ef tool..."),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);
            loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "dotnet-ef tool is installed."),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);
        }

        [Fact]
        public async Task BeSureInstalledAsync_DoesNotInstallEfTool_WhenAlreadyInstalled()
        {
            // Arrange
            var cmdHelperMock = new Mock<ICmdHelper>();
            cmdHelperMock.Setup(x => x.RunCmdAndGetOutput("dotnet tool list -g")).Returns("dotnet-ef");

            var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();

            var dotnetEfToolManager = new DotnetEfToolManager(cmdHelperMock.Object);
            dotnetEfToolManager.Logger = loggerMock.Object;

            // Act
            await dotnetEfToolManager.BeSureInstalledAsync();

            // Assert
            cmdHelperMock.Verify(x => x.RunCmd("dotnet tool install --global dotnet-ef"), Times.Never);
            loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Installing dotnet-ef tool..."),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Never);
            loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "dotnet-ef tool is installed."),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Never);
        }
    }
}
