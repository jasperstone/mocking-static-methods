using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DeploymentTests
{
    public class SelfHostDeployerTests
    {
        [Fact]
        public async Task StartSelfHostAsync_ShouldLogInformation_WhenProcessStartsAndExits()
        {
            // Arrange
            var deploymentParametersMock = new Mock<DeploymentParameters>();
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var deployer = new SelfHostDeployer(deploymentParametersMock.Object, loggerFactoryMock.Object);

            // Setup a dummy process
            var processMock = new Mock<Process>();
            var outputDataReceived = new EventHandler<DataReceivedEventArgs>((s, e) => { });
            var exitedEvent = new EventHandler<EventArgs>((s, e) => { });
            processMock.Setup(p => p.OutputDataReceived += It.IsAny<EventHandler<DataReceivedEventArgs>>())
                       .Callback<EventHandler<DataReceivedEventArgs>>(handler => outputDataReceived += handler);
            processMock.Setup(p => p.Exited += It.IsAny<EventHandler<EventArgs>>())
                       .Callback<EventHandler<EventArgs>>(handler => exitedEvent += handler);
            processMock.Setup(p => p.StartAndCaptureOutAndErrToLogger(It.IsAny<string>(), It.IsAny<ILogger>()))
                       .Returns(Task.CompletedTask);
            processMock.SetupGet(p => p.Id).Returns(123);
            processMock.SetupGet(p => p.ExitCode).Returns(0);
            processMock.SetupGet(p => p.HasExited).Returns(false);
            deployer.HostProcess = processMock.Object;

            // Act
            var task = deployer.StartSelfHostAsync(new Uri("http://localhost:5000"));

            // Simulate process output indicating start
            outputDataReceived?.Invoke(deployer.HostProcess, new DataReceivedEventArgs("Application started. Press Ctrl+C to shut down."));
            // Simulate process exit
            exitedEvent?.Invoke(deployer.HostProcess, EventArgs.Empty);

            var result = await task;

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("host process ID 123 shut down"))), Times.Once);
            Assert.NotNull(result);
            Assert.Equal(new Uri("http://localhost:5000"), result.url);
        }
    }
}
