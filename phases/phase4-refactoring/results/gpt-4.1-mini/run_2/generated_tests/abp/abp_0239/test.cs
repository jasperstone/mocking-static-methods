using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Volo.Abp.Cli.Commands.Services;
using Volo.Abp.Cli.Utils;
using Xunit;

namespace Volo.Abp.Cli.Commands.Services.Tests
{
    public class DotnetEfToolManagerTests
    {
        private class TestDotnetEfToolManager : DotnetEfToolManager
        {
            public TestDotnetEfToolManager(ICmdHelper cmdHelper) : base(cmdHelper)
            {
            }

            public bool InstallDotnetEfToolCalled { get; private set; }

            // Expose InstallDotnetEfTool for testing by making a new public method that calls the private one
            public void CallInstallDotnetEfTool()
            {
                InstallDotnetEfTool();
            }

            // Expose IsDotNetEfToolInstalled for testing by making a new public method that calls the private one
            public bool CallIsDotNetEfToolInstalled()
            {
                return IsDotNetEfToolInstalled();
            }
        }

        [Fact]
        public async Task BeSureInstalledAsync_LogsInformationWhenInstalling()
        {
            // Arrange
            var cmdHelperMock = new Mock<ICmdHelper>();
            var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();

            // Simulate tool not installed initially
            cmdHelperMock.Setup(c => c.RunCmdAndGetOutput("dotnet tool list -g"))
                .Returns("some other tool");

            var manager = new TestDotnetEfToolManager(cmdHelperMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            await manager.BeSureInstalledAsync();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Installing dotnet-ef tool..."),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "dotnet-ef tool is installed."),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);

            cmdHelperMock.Verify(c => c.RunCmd("dotnet tool install --global dotnet-ef"), Times.Once);
        }

        [Fact]
        public void IsDotNetEfToolInstalled_ReturnsTrueWhenOutputContainsDotnetEf()
        {
            // Arrange
            var cmdHelperMock = new Mock<ICmdHelper>();
            cmdHelperMock.Setup(c => c.RunCmdAndGetOutput("dotnet tool list -g"))
                .Returns("dotnet-ef some version");

            var manager = new TestDotnetEfToolManager(cmdHelperMock.Object);

            // Act
            var result = manager.CallIsDotNetEfToolInstalled();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsDotNetEfToolInstalled_ReturnsFalseWhenOutputDoesNotContainDotnetEf()
        {
            // Arrange
            var cmdHelperMock = new Mock<ICmdHelper>();
            cmdHelperMock.Setup(c => c.RunCmdAndGetOutput("dotnet tool list -g"))
                .Returns("some other tool");

            var manager = new TestDotnetEfToolManager(cmdHelperMock.Object);

            // Act
            var result = manager.CallIsDotNetEfToolInstalled();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void InstallDotnetEfTool_LogsInformation()
        {
            // Arrange
            var cmdHelperMock = new Mock<ICmdHelper>();
            var loggerMock = new Mock<ILogger<DotnetEfToolManager>>();

            var manager = new TestDotnetEfToolManager(cmdHelperMock.Object)
            {
                Logger = loggerMock.Object
            };

            // Act
            manager.CallInstallDotnetEfTool();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Installing dotnet-ef tool..."),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "dotnet-ef tool is installed."),
                    null,
                    It.IsAny<Func<It.IsAnyType, System.Exception, string>>()),
                Times.Once);

            cmdHelperMock.Verify(c => c.RunCmd("dotnet tool install --global dotnet-ef"), Times.Once);
        }
    }
}
