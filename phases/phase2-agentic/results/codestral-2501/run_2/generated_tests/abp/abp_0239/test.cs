using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Commands.Services.Tests
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
        public void InstallDotnetEfTool_ShouldLogInformation()
        {
            // Arrange
            _cmdHelperMock.Setup(x => x.RunCmdAndGetOutput("dotnet tool list -g")).Returns("");

            // Act
            _dotnetEfToolManager.BeSureInstalledAsync().Wait();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Installing dotnet-ef tool...")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("dotnet-ef tool is installed.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
