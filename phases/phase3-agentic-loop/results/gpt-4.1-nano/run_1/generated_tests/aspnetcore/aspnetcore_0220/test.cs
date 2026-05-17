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
        public async Task StartSelfHostAsync_ShouldLogAndInvokeOutputListener_WhenProcessOutputsData()
        {
            // Arrange
            var deploymentParameters = new Mock<DeploymentParameters>();
            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger>();
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

            var deployer = new SelfHostDeployer(deploymentParameters.Object, loggerFactory.Object);
            var outputData = "Now listening on: http://localhost:5000";

            // Setup a dummy process
            var process = new ProcessStub();
            deployer.HostProcess = process;

            // Setup output data event
            var outputDataEvent = new Action<DataReceivedEventArgs>(args =>
            {
                args.Data = outputData;
                process.RaiseOutputDataReceived(args);
            });
            process.OutputDataReceived += (sender, args) => outputDataEvent(args);

            // Act
            var resultTask = deployer.StartSelfHostAsync(new Uri("http://localhost:5000"));

            // Simulate process output
            process.SimulateOutputData(outputData);

            var (url, token) = await resultTask;

            // Assert
            Assert.NotNull(url);
            Assert.Equal("http://localhost:5000/", url.ToString());
            logger.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Executing")), It.IsAny<object[]>()), Times.AtLeastOnce);
            logger.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("host process ID")), It.IsAny<object[]>()), Times.Once);
        }
    }

    // Dummy classes to simulate Process and DataReceivedEventArgs
    public class ProcessStub : Process
    {
        public event EventHandler<DataReceivedEventArgs> OutputDataReceived;

        public void RaiseOutputDataReceived(DataReceivedEventArgs args)
        {
            OutputDataReceived?.Invoke(this, args);
        }

        public void SimulateOutputData(string data)
        {
            RaiseOutputDataReceived(new DataReceivedEventArgs { Data = data });
        }
    }

    public class DataReceivedEventArgs : EventArgs
    {
        public string Data { get; set; }
    }
}
