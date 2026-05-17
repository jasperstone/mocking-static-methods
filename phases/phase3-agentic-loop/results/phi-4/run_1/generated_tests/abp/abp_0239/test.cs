using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Utils;

namespace Volo.Abp.Cli.Tests
{
    public class DotnetEfToolManagerTests
    {
        [Fact]
        public void InstallDotnetEfTool_LogsInformationMessages()
        {
            // Arrange
            var cmdHelperMock = new Mock<ICmdHelper>();
            var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();
            var manager = new DotnetEfToolManager(cmdHelperMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            manager.InstallDotnetEfTool();

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation("Installing dotnet-ef tool..."),
                Times.Once);

            loggerMock.Verify(
                logger => logger.LogInformation("dotnet-ef tool is installed."),
                Times.Once);
        }
    }
}
