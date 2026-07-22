using System;
using System.Collections.Generic;
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
        [Fact]
        public async Task BeSureInstalledAsync_ShouldInstallDotnetEfTool_WhenNotInstalled()
        {
            // Arrange
            var mockCmdHelper = new Mock<ICmdHelper>();
            mockCmdHelper.Setup(x => x.RunCmdAndGetOutput("dotnet tool list -g")).Returns("other-tools");
            mockCmdHelper.Setup(x => x.RunCmd("dotnet tool install --global dotnet-ef")).Verifiable();

            var logMessages = new List<string>();
            var mockLogger = new Mock<ILogger<DotnetEfToolManager>>();
            mockLogger.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception, string>>()))
                .Callback(new InvocationAction(invocation =>
                {
                    var logLevel = (LogLevel)invocation.Arguments[0];
                    var eventId = (EventId)invocation.Arguments[1];
                    var state = invocation.Arguments[2];
                    var exception = (Exception)invocation.Arguments[3];
                    var formatter = (Func<object, Exception, string>)invocation.Arguments[4];

                    if (logLevel == LogLevel.Information)
                    {
                        logMessages.Add(formatter(state, exception));
                    }
                }));

            var dotnetEfToolManager = new DotnetEfToolManager(mockCmdHelper.Object)
            {
                Logger = mockLogger.Object
            };

            // Act
            await dotnetEfToolManager.BeSureInstalledAsync();

            // Assert
            mockCmdHelper.Verify(x => x.RunCmd("dotnet tool install --global dotnet-ef"), Times.Once);
            Assert.Contains("Installing dotnet-ef tool...", logMessages);
            Assert.Contains("dotnet-ef tool is installed.", logMessages);
        }

        [Fact]
        public async Task BeSureInstalledAsync_ShouldNotInstallDotnetEfTool_WhenAlreadyInstalled()
        {
            // Arrange
            var mockCmdHelper = new Mock<ICmdHelper>();
            mockCmdHelper.Setup(x => x.RunCmdAndGetOutput("dotnet tool list -g")).Returns("dotnet-ef");

            var logMessages = new List<string>();
            var mockLogger = new Mock<ILogger<DotnetEfToolManager>>();
            mockLogger.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception, string>>()))
                .Callback(new InvocationAction(invocation =>
                {
                    var logLevel = (LogLevel)invocation.Arguments[0];
                    var eventId = (EventId)invocation.Arguments[1];
                    var state = invocation.Arguments[2];
                    var exception = (Exception)invocation.Arguments[3];
                    var formatter = (Func<object, Exception, string>)invocation.Arguments[4];

                    if (logLevel == LogLevel.Information)
                    {
                        logMessages.Add(formatter(state, exception));
                    }
                }));

            var dotnetEfToolManager = new DotnetEfToolManager(mockCmdHelper.Object)
            {
                Logger = mockLogger.Object
            };

            // Act
            await dotnetEfToolManager.BeSureInstalledAsync();

            // Assert
            mockCmdHelper.Verify(x => x.RunCmd("dotnet tool install --global dotnet-ef"), Times.Never);
            Assert.DoesNotContain("Installing dotnet-ef tool...", logMessages);
            Assert.DoesNotContain("dotnet-ef tool is installed.", logMessages);
        }
    }
}
