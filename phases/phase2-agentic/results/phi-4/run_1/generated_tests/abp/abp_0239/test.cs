using Moq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Utils;

namespace Volo.Abp.Cli.Tests.Commands.Services
{
    public class DotnetEfToolManagerTests
    {
        [Fact]
        public async Task InstallDotnetEfTool_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();
            var cmdHelperMock = new Mock<ICmdHelper>();

            // Simulate that the tool is not installed
            cmdHelperMock.Setup(x => x.RunCmdAndGetOutput("dotnet tool list -g"))
                         .Returns("Some other tool");

            var manager = new DotnetEfToolManager(cmdHelperMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await manager.BeSureInstalledAsync();

            // Assert
            loggerMock.Verify(x => x.LogInformation("Installing dotnet-ef tool..."), Times.Once);
            loggerMock.Verify(x => x.LogInformation("dotnet-ef tool is installed."), Times.Once);
        }
    }
}
