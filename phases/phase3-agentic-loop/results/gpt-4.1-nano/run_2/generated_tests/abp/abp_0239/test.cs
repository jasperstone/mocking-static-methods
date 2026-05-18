using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands.Services;

namespace Volo.Abp.Cli.Tests
{
    public class DotnetEfToolManagerTests
    {
        [Fact]
        public async Task BeSureInstalledAsync_Should_CallInstall_When_ToolNotInstalled()
        {
            // Arrange
            var mockCmdHelper = new Mock<ICmdHelper>();
            var outputWithoutEf = "some output without ef";
            var outputWithEf = "some output with dotnet-ef";

            // Setup RunCmdAndGetOutput to simulate tool not installed initially, then installed
            mockCmdHelper.SetupSequence(c => c.RunCmdAndGetOutput("dotnet tool list -g"))
                .Returns(outputWithoutEf)
                .Returns(outputWithEf);

            var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();
            var manager = new DotnetEfToolManager(mockCmdHelper.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await manager.BeSureInstalledAsync();

            // Assert
            mockCmdHelper.Verify(c => c.RunCmd("dotnet tool install --global dotnet-ef"), Times.Once);
            loggerMock.Verify(
                l => l.LogInformation("Installing dotnet-ef tool..."),
                Times.Once);
            loggerMock.Verify(
                l => l.LogInformation("dotnet-ef tool is installed."),
                Times.Once);
        }

        [Fact]
        public async Task BeSureInstalledAsync_Should_Not_CallInstall_When_ToolAlreadyInstalled()
        {
            // Arrange
            var mockCmdHelper = new Mock<ICmdHelper>();
            var outputWithEf = "some output with dotnet-ef";

            mockCmdHelper.Setup(c => c.RunCmdAndGetOutput("dotnet tool list -g"))
                .Returns(outputWithEf);

            var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();
            var manager = new DotnetEfToolManager(mockCmdHelper.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await manager.BeSureInstalledAsync();

            // Assert
            mockCmdHelper.Verify(c => c.RunCmd("dotnet tool install --global dotnet-ef"), Times.Never);
            loggerMock.Verify(
                l => l.LogInformation(It.IsAny<string>()),
                Times.Never);
        }
    }
}
