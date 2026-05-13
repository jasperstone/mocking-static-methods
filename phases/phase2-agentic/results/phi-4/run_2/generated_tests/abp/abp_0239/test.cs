using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Utils;

namespace Volo.Abp.Cli.Tests.Commands.Services
{
    public class DotnetEfToolManagerTests
    {
        [Fact]
        public async Task BeSureInstalledAsync_WhenToolNotInstalled_LogsInformationAndInstallsTool()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();
            var cmdHelperMock = new Mock<ICmdHelper>();

            cmdHelperMock.Setup(helper => helper.RunCmdAndGetOutput("dotnet tool list -g"))
                         .Returns("Some other tool");

            var manager = new DotnetEfToolManager(cmdHelperMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await manager.BeSureInstalledAsync();

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation("Installing dotnet-ef tool..."),
                Times.Once);

            loggerMock.Verify(
                logger => logger.LogInformation("dotnet-ef tool is installed."),
                Times.Once);

            cmdHelperMock.Verify(
                helper => helper.RunCmd("dotnet tool install --global dotnet-ef"),
                Times.Once);
        }
    }
}
