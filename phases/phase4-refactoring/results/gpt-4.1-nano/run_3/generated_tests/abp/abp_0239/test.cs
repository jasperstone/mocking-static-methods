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
        public async Task BeSureInstalledAsync_ShouldLogInformation_WhenToolNotInstalled()
        {
            // Arrange
            var mockCmdHelper = new Mock<ICmdHelper>();
            mockCmdHelper.Setup(m => m.RunCmdAndGetOutput(It.IsAny<string>())).Returns("some output without ef");
            var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();
            var manager = new DotnetEfToolManager(mockCmdHelper.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await manager.BeSureInstalledAsync();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Installing dotnet-ef tool")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
