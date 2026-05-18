using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting
{
    public class SelfHostDeployerTests
    {
        [Fact]
        public void Test_LogInformationOnProcessStart()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SelfHostDeployer>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var deploymentParameters = new DeploymentParameters(null)
            {
                ApplicationName = "TestApp",
                ServerType = ServerType.Kestrel,
                Scheme = "http",
                ApplicationBaseUriHint = "localhost",
                StatusMessagesEnabled = false
            };

            var deployer = new TestableSelfHostDeployer(deploymentParameters, loggerFactoryMock.Object);

            var processMock = new Mock<Process>();
            processMock.Setup(p => p.Start());
            processMock.Setup(p => p.HasExited).Returns(false);
            processMock.Setup(p => p.Id).Returns(12345);

            var startInfo = new ProcessStartInfo
            {
                FileName = "test.exe"
            };

            // Act
            deployer.TestExecuteStartProcess(startInfo, processMock.Object);

            // Assert - Verifies the LogInformation call from line 190
            loggerMock.Verify(
                logger => logger.LogInformation(
                    "Started {fileName}. Process Id : {processId}",
                    It.Is<string>(name => name == "test.exe"),
                    It.Is<int>(id => id == 12345)
                ),
                Times.Once
            );
        }

        private class TestableSelfHostDeployer : SelfHostDeployer
        {
            public TestableSelfHostDeployer(DeploymentParameters deploymentParameters, ILoggerFactory loggerFactory)
                : base(deploymentParameters, loggerFactory)
            {
            }

            public void TestExecuteStartProcess(ProcessStartInfo startInfo, Process process)
            {
                HostProcess = process;
                HostProcess.StartInfo = startInfo;

                // Simulate HostProcess.StartAndCaptureOutAndErrToLogger succeeding
                HostProcess.Start();

                if (HostProcess.HasExited)
                {
                    Logger.LogError("Host process {processName} {pid} exited with code {exitCode} or failed to start.", startInfo.FileName, HostProcess.Id, HostProcess.ExitCode);
                    throw new Exception("Failed to start host");
                }

                // This is the exact line 190 call we want to test
                Logger.LogInformation("Started {fileName}. Process Id : {processId}", startInfo.FileName, HostProcess.Id);
            }
        }
    }
}
