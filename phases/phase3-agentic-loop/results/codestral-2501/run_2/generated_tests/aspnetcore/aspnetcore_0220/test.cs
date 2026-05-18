using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class SelfHostDeployerTests
    {
        [Fact]
        public async Task DeployAsync_LogsInformation_WhenHostProcessStartsSuccessfully()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SelfHostDeployer>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var deploymentParameters = new DeploymentParameters
            {
                // Set necessary properties for the test
                ServerType = ServerType.Kestrel,
                Scheme = "http",
                ApplicationBaseUriHint = "http://localhost:5000",
                StatusMessagesEnabled = true,
                ApplicationType = ApplicationType.Portable,
                RuntimeFlavor = RuntimeFlavor.CoreClr,
                RuntimeArchitecture = RuntimeArchitecture.x64,
                ApplicationPath = "/path/to/application",
                ApplicationName = "TestApp",
                Configuration = "Debug",
                TargetFramework = "net6.0"
            };

            var processMock = new Mock<Process>();
            processMock.Setup(p => p.Start()).Returns(true);
            processMock.Setup(p => p.HasExited).Returns(false);
            processMock.Setup(p => p.Id).Returns(12345);
            processMock.Setup(p => p.OutputDataReceived += It.IsAny<DataReceivedEventHandler>())
                .Callback<DataReceivedEventHandler>((handler) =>
                {
                    handler.Invoke(null, new DataReceivedEventArgs("Application started. Press Ctrl+C to shut down."));
                });

            var selfHostDeployer = new SelfHostDeployer(deploymentParameters, loggerFactoryMock.Object)
            {
                HostProcess = processMock.Object
            };

            // Act
            await selfHostDeployer.DeployAsync();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Started")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task DeployAsync_LogsInformation_WhenHostProcessExitsUnexpectedly()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SelfHostDeployer>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var deploymentParameters = new DeploymentParameters
            {
                // Set necessary properties for the test
                ServerType = ServerType.Kestrel,
                Scheme = "http",
                ApplicationBaseUriHint = "http://localhost:5000",
                StatusMessagesEnabled = true,
                ApplicationType = ApplicationType.Portable,
                RuntimeFlavor = RuntimeFlavor.CoreClr,
                RuntimeArchitecture = RuntimeArchitecture.x64,
                ApplicationPath = "/path/to/application",
                ApplicationName = "TestApp",
                Configuration = "Debug",
                TargetFramework = "net6.0"
            };

            var processMock = new Mock<Process>();
            processMock.Setup(p => p.Start()).Returns(true);
            processMock.Setup(p => p.HasExited).Returns(true);
            processMock.Setup(p => p.Id).Returns(12345);
            processMock.Setup(p => p.ExitCode).Returns(1);

            var selfHostDeployer = new SelfHostDeployer(deploymentParameters, loggerFactoryMock.Object)
            {
                HostProcess = processMock.Object
            };

            // Simulate host process exit
            selfHostDeployer.HostProcess.Exited += (sender, e) =>
            {
                loggerMock.Object.LogInformation("host process ID {pid} shut down", selfHostDeployer.HostProcess.Id);
            };

            // Act
            await selfHostDeployer.DeployAsync();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("shut down")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
