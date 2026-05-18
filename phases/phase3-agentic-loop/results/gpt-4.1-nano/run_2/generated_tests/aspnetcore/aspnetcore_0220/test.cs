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
            var loggerFactory = new LoggerFactory();
            var deployer = new SelfHostDeployer(deploymentParameters.Object, loggerFactory);
            var loggerMock = new Mock<ILogger>();
            deployer.Logger = loggerMock.Object;

            var outputData = "Now listening on: http://localhost:5000";
            var outputCalled = false;
            deployer.ProcessOutputListener = (data) =>
            {
                outputCalled = true;
                Assert.Contains("localhost:5000", data);
            };

            var processMock = new Mock<Process>();
            var tcs = new TaskCompletionSource();

            // Setup process properties
            processMock.Setup(p => p.StartAndCaptureOutAndErrToLogger(It.IsAny<string>(), It.IsAny<ILogger>()))
                .Callback<string, ILogger>((exe, log) =>
                {
                    // Simulate output data received
                    var dataReceivedEventArgs = new DataReceivedEventArgs(outputData);
                    deployer.HostProcess.OutputDataReceived?.Invoke(deployer.HostProcess, dataReceivedEventArgs);
                    tcs.SetResult();
                });

            deployer.HostProcess = processMock.Object;

            // Act
            var resultTask = deployer.StartSelfHostAsync(new Uri("http://test"));
            await tcs.Task; // Wait for output processing

            // Assert
            loggerMock.VerifyLog(LogLevel.Information, "Executing");
            Assert.True(outputCalled);
        }
    }

    // Helper class for DataReceivedEventArgs
    public class DataReceivedEventArgs : EventArgs
    {
        public string Data { get; }
        public DataReceivedEventArgs(string data)
        {
            Data = data;
        }
    }
}
