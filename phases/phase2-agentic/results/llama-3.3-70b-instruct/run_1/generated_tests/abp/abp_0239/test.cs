using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Volo.Abp.Cli.Commands.Services;
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
            cmdHelperMock.Setup(x => x.RunCmdAndGetOutput("dotnet tool list -g")).Returns(string.Empty);
            cmdHelperMock.Setup(x => x.RunCmd("dotnet tool install --global dotnet-ef"));

            var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();

            var dotnetEfToolManager = new DotnetEfToolManager(cmdHelperMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await dotnetEfToolManager.BeSureInstalledAsync();

            // Assert
            loggerMock.Verify(x => x.LogInformation("Installing dotnet-ef tool..."), Times.Once);
            loggerMock.Verify(x => x.LogInformation("dotnet-ef tool is installed."), Times.Once);
            cmdHelperMock.Verify(x => x.RunCmd("dotnet tool install --global dotnet-ef"), Times.Once);
        }

        [Fact]
        public async Task BeSureInstalledAsync_DoesNotInstallEfTool_WhenAlreadyInstalled()
        {
            // Arrange
            var cmdHelperMock = new Mock<ICmdHelper>();
            cmdHelperMock.Setup(x => x.RunCmdAndGetOutput("dotnet tool list -g")).Returns("dotnet-ef");

            var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();

            var dotnetEfToolManager = new DotnetEfToolManager(cmdHelperMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await dotnetEfToolManager.BeSureInstalledAsync();

            // Assert
            loggerMock.Verify(x => x.LogInformation("Installing dotnet-ef tool..."), Times.Never);
            loggerMock.Verify(x => x.LogInformation("dotnet-ef tool is installed."), Times.Never);
            cmdHelperMock.Verify(x => x.RunCmd("dotnet tool install --global dotnet-ef"), Times.Never);
        }
    }
}
