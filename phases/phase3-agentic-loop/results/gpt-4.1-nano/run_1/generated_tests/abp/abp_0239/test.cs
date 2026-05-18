using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands.Services;

namespace Volo.Abp.Cli.Tests
{
    public class DotnetEfToolManagerTests
    {
        [Fact]
        public async Task BeSureInstalledAsync_Should_LogInformation_When_Installing()
        {
            // Arrange
            var mockCmdHelper = new Mock<ICmdHelper>();
            // Simulate that the tool is not installed initially
            mockCmdHelper.SetupSequence(c => c.RunCmdAndGetOutput(It.IsAny<string>()))
                .Returns("some output without ef")
                .Returns("some output with dotnet-ef");
            var mockLogger = new Mock<ILogger<DotnetEfToolManager>>();
            var manager = new DotnetEfToolManager(mockCmdHelper.Object)
            {
                Logger = mockLogger.Object
            };

            // Act
            await manager.BeSureInstalledAsync();

            // Assert
            mockLogger.Verify(
                x => x.LogInformation("Installing dotnet-ef tool..."),
                Times.Once);
            mockLogger.Verify(
                x => x.LogInformation("dotnet-ef tool is installed."),
                Times.Once);
        }
    }
}
