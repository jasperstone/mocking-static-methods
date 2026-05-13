using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Tests.Commands.Services
{
    public class DotnetEfToolManagerTests
    {
        [Fact]
        public async Task BeSureInstalledAsync_ShouldInstallAndLog_WhenToolIsNotInstalled()
        {
            var cmdHelperMock = new Mock<ICmdHelper>();
            cmdHelperMock.Setup(helper => helper.RunCmdAndGetOutput("dotnet tool list -g", null))
                         .Returns("some-other-tool");
            var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();

            var manager = new DotnetEfToolManager(cmdHelperMock.Object)
            {
                Logger = loggerMock.Object
            };

            await manager.BeSureInstalledAsync();

            cmdHelperMock.Verify(helper => helper.RunCmd("dotnet tool install --global dotnet-ef", null), Times.Once);
            loggerMock.Verify(logger => logger.LogInformation("Installing dotnet-ef tool..."), Times.Once);
            loggerMock.Verify(logger => logger.LogInformation("dotnet-ef tool is installed."), Times.Once);
        }

        [Fact]
        public async Task BeSureInstalledAsync_ShouldNotInstall_WhenToolAlreadyInstalled()
        {
            var cmdHelperMock = new Mock<ICmdHelper>();
            cmdHelperMock.Setup(helper => helper.RunCmdAndGetOutput("dotnet tool list -g", null))
                         .Returns("dotnet-ef");
            var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();

            var manager = new DotnetEfToolManager(cmdHelperMock.Object)
            {
                Logger = loggerMock.Object
            };

            await manager.BeSureInstalledAsync();

            cmdHelperMock.Verify(helper => helper.RunCmd(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            loggerMock.Verify(logger => logger.LogInformation("Installing dotnet-ef tool..."), Times.Never);
            loggerMock.Verify(logger => logger.LogInformation("dotnet-ef tool is installed."), Times.Never);
        }
    }
}
