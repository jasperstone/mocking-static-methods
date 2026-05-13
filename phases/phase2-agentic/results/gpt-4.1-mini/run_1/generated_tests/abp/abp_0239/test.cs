using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands.Services;
using Xunit;

namespace Volo.Abp.Cli.Commands.Services.Tests
{
    public class DotnetEfToolManagerTests
    {
        [Fact]
        public async Task BeSureInstalledAsync_InstallsTool_WhenNotInstalled()
        {
            // Arrange
            var cmdHelperMock = new Mock<ICmdHelper>();
            var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();

            // Simulate that dotnet-ef is not installed
            cmdHelperMock.Setup(c => c.RunCmdAndGetOutput("dotnet tool list -g"))
                .Returns("some other tool");

            var manager = new DotnetEfToolManager(cmdHelperMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await manager.BeSureInstalledAsync();

            // Assert
            // Verify that the install command was run
            cmdHelperMock.Verify(c => c.RunCmd("dotnet tool install --global dotnet-ef"), Times.Once);

            // Verify that the logger was called with the expected messages
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Installing dotnet-ef tool..."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "dotnet-ef tool is installed."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task BeSureInstalledAsync_DoesNotInstall_WhenAlreadyInstalled()
        {
            // Arrange
            var cmdHelperMock = new Mock<ICmdHelper>();
            var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();

            // Simulate that dotnet-ef is already installed
            cmdHelperMock.Setup(c => c.RunCmdAndGetOutput("dotnet tool list -g"))
                .Returns("dotnet-ef");

            var manager = new DotnetEfToolManager(cmdHelperMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await manager.BeSureInstalledAsync();

            // Assert
            // Verify that the install command was never run
            cmdHelperMock.Verify(c => c.RunCmd(It.IsAny<string>()), Times.Never);

            // Verify that no information logs about installation were made
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }
    }
}
