using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Utils;

namespace Volo.Abp.Cli.Commands.Services.Tests
{
    public class DotnetEfToolManagerTests
    {
        [Fact]
        public void InstallDotnetEfTool_LogsInformation()
        {
            // Arrange
            var mockCmdHelper = new Mock<ICmdHelper>();
            var mockLogger = new Mock<ILogger<DotnetEfToolManager>>();
            var dotnetEfToolManager = new DotnetEfToolManager(mockCmdHelper.Object)
            {
                Logger = mockLogger.Object
            };

            // Act
            dotnetEfToolManager.InstallDotnetEfTool();

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
