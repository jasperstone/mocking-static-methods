using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Utils;
using Xunit;

public class DotnetEfToolManagerTests
{
    [Fact]
    public async Task InstallDotnetEfTool_ShouldLogInformation()
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
        loggerMock.Verify(
            x => x.LogInformation("Installing dotnet-ef tool..."),
            Times.Once);

        loggerMock.Verify(
            x => x.LogInformation("dotnet-ef tool is installed."),
            Times.Once);
    }
}
