using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Commands.Services;

namespace Volo.Abp.Cli.Tests
{
    public class DotnetEfToolManagerTests
    {
        [Fact]
        public async Task InstallDotnetEfTool_Should_LogInformation_Called()
        {
            // Arrange
            var cmdHelperMock = new Mock<ICmdHelper>();
            cmdHelperMock.Setup(c => c.RunCmd(It.IsAny<string>())).Returns(string.Empty);
            var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();

            var manager = new DotnetEfToolManager(cmdHelperMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await manager.InstallDotnetEfTool();

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Installing dotnet-ef tool..."),
                Times.Once);
        }
    }
}
