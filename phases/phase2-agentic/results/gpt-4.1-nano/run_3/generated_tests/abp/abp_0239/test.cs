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
        public async Task BeSureInstalledAsync_ShouldLogInformation_WhenInstalling()
        {
            // Arrange
            var mockCmdHelper = new Mock<ICmdHelper>();
            // Simulate that the tool is not installed initially
            mockCmdHelper.SetupSequence(c => c.RunCmdAndGetOutput(It.IsAny<string>()))
                .Returns("") // First call: output does not contain "dotnet-ef"
                .Returns("dotnet-ef 5.0.0"); // Second call: after install, output contains "dotnet-ef"

            var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();
            var manager = new DotnetEfToolManager(mockCmdHelper.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await manager.BeSureInstalledAsync();

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Installing dotnet-ef tool..."),
                Times.Once);
            loggerMock.Verify(
                x => x.LogInformation("dotnet-ef tool is installed."),
                Times.Once);
        }
    }
}
