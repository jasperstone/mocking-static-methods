using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Core.Tests
{
    public class DotnetEfToolManagerTests
    {
        [Fact]
        public async Task BeSureInstalledAsync_InstallsEfTool_WhenNotInstalled()
        {
            // Arrange
            var cmdHelperMock = new Mock<ICmdHelper>();
            cmdHelperMock.Setup(x => x.RunCmdAndGetOutput(It.IsAny<string>())).Returns(string.Empty);
            cmdHelperMock.Setup(x => x.RunCmd(It.IsAny<string>()));
            var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();
            var dotnetEfToolManager = new DotnetEfToolManager(cmdHelperMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await dotnetEfToolManager.BeSureInstalledAsync();

            // Assert
            loggerMock.Verify(x => ((ILogger)x).Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                (Func<object, Exception, string>)((state, exception) => state.ToString()))
            , Times.Exactly(2));
            cmdHelperMock.Verify(x => x.RunCmd("dotnet tool install --global dotnet-ef"), Times.Once);
        }

        [Fact]
        public async Task BeSureInstalledAsync_DoesNotInstallEfTool_WhenAlreadyInstalled()
        {
            // Arrange
            var cmdHelperMock = new Mock<ICmdHelper>();
            cmdHelperMock.Setup(x => x.RunCmdAndGetOutput(It.IsAny<string>())).Returns("dotnet-ef");
            var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();
            var dotnetEfToolManager = new DotnetEfToolManager(cmdHelperMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await dotnetEfToolManager.BeSureInstalledAsync();

            // Assert
            loggerMock.Verify(x => ((ILogger)x).Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                (Func<object, Exception, string>)((state, exception) => state.ToString()))
            , Times.Never);
            cmdHelperMock.Verify(x => x.RunCmd("dotnet tool install --global dotnet-ef"), Times.Never);
        }
    }
}
