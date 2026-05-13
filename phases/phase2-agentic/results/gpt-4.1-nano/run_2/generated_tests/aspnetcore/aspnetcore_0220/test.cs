using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using System.Diagnostics;

namespace DeploymentTests
{
    public class SelfHostDeployerTests
    {
        private class DummyProcess : Process
        {
            public event Action<object, DataReceivedEventArgs> OutputDataReceivedEvent;
            public event EventHandler ExitedEvent;

            public override void Start()
            {
                // Simulate process start
                return;
            }

            public void RaiseOutputDataReceived(string data)
            {
                OutputDataReceivedEvent?.Invoke(this, new DataReceivedEventArgs(data));
            }

            public void RaiseExited()
            {
                ExitedEvent?.Invoke(this, EventArgs.Empty);
            }
        }

        [Fact]
        public async Task StartSelfHost_ShouldLogInformation_WhenProcessStartsAndExits()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deployer = new SelfHostDeployer(new DeploymentParameters(), new LoggerFactory());
            var process = new DummyProcess();
            deployer.HostProcess = process;

            var outputData = "";
            process.OutputDataReceivedEvent += (sender, args) =>
            {
                outputData = args.Data;
            };

            var startedTcs = new TaskCompletionSource();

            // Replace the process with our dummy
            deployer.HostProcess = process;

            // Act
            var task = deployer.StartSelfHostAsync(new Uri("http://localhost:5000"));

            // Simulate process output indicating start
            process.RaiseOutputDataReceived("Application started. Press Ctrl+C to shut down.");
            // Simulate process exit
            process.RaiseExited();

            var result = await task;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("http://localhost:5000/", result.url.ToString());
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("host process ID")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
