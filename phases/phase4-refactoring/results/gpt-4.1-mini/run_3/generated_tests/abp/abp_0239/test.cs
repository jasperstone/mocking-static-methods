using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands.Services;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands.Services
{
    public class DotnetEfToolManagerTests
    {
        [Fact]
        public async Task BeSureInstalledAsync_InstallsTool_WhenNotInstalled_AndLogsInformation()
        {
            // Arrange
            var cmdHelperMock = new Mock<ICmdHelper>();
            var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();

            // Simulate that dotnet-ef is not installed
            cmdHelperMock.Setup(c => c.RunCmdAndGetOutput("dotnet tool list -g")).Returns(string.Empty);
            cmdHelperMock.Setup(c => c.RunCmd("dotnet tool install --global dotnet-ef"));

            var manager = new DotnetEfToolManager(cmdHelperMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await manager.BeSureInstalledAsync();

            // Assert
            cmdHelperMock.Verify(c => c.RunCmdAndGetOutput("dotnet tool list -g"), Times.Once);
            cmdHelperMock.Verify(c => c.RunCmd("dotnet tool install --global dotnet-ef"), Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Installing dotnet-ef tool..."),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "dotnet-ef tool is installed."),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task BeSureInstalledAsync_DoesNotInstallTool_WhenAlreadyInstalled()
        {
            // Arrange
            var cmdHelperMock = new Mock<ICmdHelper>();
            var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();

            // Simulate that dotnet-ef is installed
            cmdHelperMock.Setup(c => c.RunCmdAndGetOutput("dotnet tool list -g")).Returns("dotnet-ef");

            var manager = new DotnetEfToolManager(cmdHelperMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await manager.BeSureInstalledAsync();

            // Assert
            cmdHelperMock.Verify(c => c.RunCmdAndGetOutput("dotnet tool list -g"), Times.Once);
            cmdHelperMock.Verify(c => c.RunCmd(It.IsAny<string>()), Times.Never);

            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<System.Exception>(),
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Never);
        }
    }
}
