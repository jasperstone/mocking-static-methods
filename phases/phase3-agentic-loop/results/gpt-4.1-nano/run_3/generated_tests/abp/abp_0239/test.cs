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
        public async Task InstallDotnetEfTool_Should_LogInformation_Called()
        {
            // Arrange
            var mockCmdHelper = new Mock<ICmdHelper>();
            mockCmdHelper.Setup(c => c.RunCmdAndGetOutput(It.IsAny<string>())).Returns(string.Empty);
            mockCmdHelper.Setup(c => c.RunCmd(It.IsAny<string>()));

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
        }
    }
}
