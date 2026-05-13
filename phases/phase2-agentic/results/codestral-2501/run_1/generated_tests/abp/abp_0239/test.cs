using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands.Services
{
    public class DotnetEfToolManagerTests
    {
        private readonly Mock<ICmdHelper> _cmdHelperMock;
        private readonly Mock<ILogger<DotnetEfToolManager>> _loggerMock;
        private readonly DotnetEfToolManager _dotnetEfToolManager;

        public DotnetEfToolManagerTests()
        {
            _cmdHelperMock = new Mock<ICmdHelper>();
            _loggerMock = new Mock<ILogger<DotnetEfToolManager>>();
            _dotnetEfToolManager = new DotnetEfToolManager(_cmdHelperMock.Object)
            {
                Logger = _loggerMock.Object
            };
        }

        [Fact]
        public async Task BeSureInstalledAsync_ShouldInstallDotnetEfTool_WhenNotInstalled()
        {
            // Arrange
            _cmdHelperMock.Setup(x => x.RunCmdAndGetOutput("dotnet tool list -g")).Returns("other tools");
            _cmdHelperMock.Setup(x => x.RunCmd("dotnet tool install --global dotnet-ef")).Verifiable();

            // Act
            await _dotnetEfToolManager.BeSureInstalledAsync();

            // Assert
            _cmdHelperMock.Verify(x => x.RunCmd("dotnet tool install --global dotnet-ef"), Times.Once);
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Installing dotnet-ef tool...")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("dotnet-ef tool is installed.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }

        [Fact]
        public async Task BeSureInstalledAsync_ShouldNotInstallDotnetEfTool_WhenAlreadyInstalled()
        {
            // Arrange
            _cmdHelperMock.Setup(x => x.RunCmdAndGetOutput("dotnet tool list -g")).Returns("dotnet-ef");

            // Act
            await _dotnetEfToolManager.BeSureInstalledAsync();

            // Assert
            _cmdHelperMock.Verify(x => x.RunCmd("dotnet tool install --global dotnet-ef"), Times.Never);
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Installing dotnet-ef tool...")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Never);
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("dotnet-ef tool is installed.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Never);
        }
    }
}
