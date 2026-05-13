using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands.Services;
using Xunit;

namespace Volo.Abp.Cli.Commands.Services.Tests
{
    public class DotnetEfToolManagerTests
    {
        private interface ICmdHelper
        {
            string RunCmdAndGetOutput(string command);
            void RunCmd(string command);
        }

        [Fact]
        public async Task BeSureInstalledAsync_InstallsToolAndLogsInformation()
        {
            // Arrange
            var cmdHelperMock = new Mock<ICmdHelper>();
            var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();

            // Simulate that dotnet-ef tool is not installed
            cmdHelperMock.Setup(c => c.RunCmdAndGetOutput("dotnet tool list -g"))
                .Returns("some other tool");

            var manager = new DotnetEfToolManager(cmdHelperMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await manager.BeSureInstalledAsync();

            // Assert
            // Verify that RunCmd was called to install the tool
            cmdHelperMock.Verify(c => c.RunCmd("dotnet tool install --global dotnet-ef"), Times.Once);

            // Verify that LogInformation was called with the expected messages
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
        public async Task BeSureInstalledAsync_DoesNotInstallIfToolAlreadyInstalled()
        {
            // Arrange
            var cmdHelperMock = new Mock<ICmdHelper>();
            var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();

            // Simulate that dotnet-ef tool is already installed
            cmdHelperMock.Setup(c => c.RunCmdAndGetOutput("dotnet tool list -g"))
                .Returns("dotnet-ef");

            var manager = new DotnetEfToolManager(cmdHelperMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await manager.BeSureInstalledAsync();

            // Assert
            // Verify that RunCmd was never called to install the tool
            cmdHelperMock.Verify(c => c.RunCmd(It.IsAny<string>()), Times.Never);

            // Verify that LogInformation was never called
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
