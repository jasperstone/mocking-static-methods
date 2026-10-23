using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Commands.Services.Tests
{
    public class DotnetEfToolManagerTests
    {
        [Fact]
        public async Task BeSureInstalledAsync_ShouldInstallTool_WhenNotInstalled()
        {
            // Arrange
            var cmdHelperMock = new Mock<ICmdHelper>();
            cmdHelperMock.Setup(c => c.RunCmdAndGetOutput("dotnet tool list -g")).Returns("other-tools");
            var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();

            var dotnetEfToolManager = new DotnetEfToolManager(cmdHelperMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await dotnetEfToolManager.BeSureInstalledAsync();

            // Assert
            cmdHelperMock.Verify(c => c.RunCmd("dotnet tool install --global dotnet-ef"), Times.Once);
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Installing dotnet-ef tool...")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("dotnet-ef tool is installed.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task BeSureInstalledAsync_ShouldNotInstallTool_WhenAlreadyInstalled()
        {
            // Arrange
            var cmdHelperMock = new Mock<ICmdHelper>();
            cmdHelperMock.Setup(c => c.RunCmdAndGetOutput("dotnet tool list -g")).Returns("dotnet-ef");
            var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();

            var dotnetEfToolManager = new DotnetEfToolManager(cmdHelperMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await dotnetEfToolManager.BeSureInstalledAsync();

            // Assert
            cmdHelperMock.Verify(c => c.RunCmd("dotnet tool install --global dotnet-ef"), Times.Never);
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Installing dotnet-ef tool...")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Never);

            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("dotnet-ef tool is installed.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Never);
        }
    }
}
