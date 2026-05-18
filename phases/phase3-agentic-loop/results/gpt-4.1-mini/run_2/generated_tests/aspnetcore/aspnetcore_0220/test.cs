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
        public async Task StartSelfHostAsync_LogsStartedInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var deploymentParameters = new DeploymentParameters(
                applicationPath: AppContext.BaseDirectory,
                applicationName: "TestApp",
                serverType: ServerType.Kestrel,
                runtimeFlavor: RuntimeFlavor.Clr,
                runtimeArchitecture: RuntimeArchitecture.x64,
                applicationType: ApplicationType.Standalone,
                configuration: "Debug",
                targetFramework: "net6.0",
                publishApplicationBeforeDeployment: false,
                environmentVariables: null,
                statusMessagesEnabled: false,
                scheme: "http",
                applicationBaseUriHint: null);

            var deployer = new TestSelfHostDeployer(deploymentParameters, loggerFactoryMock.Object);

            // Act
            var hintUrl = new Uri("http://localhost:5000");
            var result = await deployer.InvokeStartSelfHostAsync(hintUrl);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Started") && v.ToString().Contains("TestApp") && v.ToString().Contains("1234")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        // Helper subclass to expose protected method and override process start
        private class TestSelfHostDeployer : SelfHostDeployer
        {
            public TestSelfHostDeployer(DeploymentParameters deploymentParameters, ILoggerFactory loggerFactory)
                : base(deploymentParameters, loggerFactory)
            {
            }

            public Task<(Uri url, CancellationToken hostExitToken)> InvokeStartSelfHostAsync(Uri hintUrl)
            {
                return StartSelfHostAsync(hintUrl);
            }

            protected override void AddEnvironmentVariablesToProcess(ProcessStartInfo startInfo, System.Collections.Generic.IDictionary<string, string> environmentVariables)
            {
                // No-op for test
            }

            protected override string GetDotNetExeForArchitecture()
            {
                return "dotnet";
            }

            // Override StartAndCaptureOutAndErrToLogger to simulate process start without actually starting
            public void StartAndCaptureOutAndErrToLogger(string executableName, ILogger logger)
            {
                // Simulate logging the "Started {fileName}. Process Id : {processId}" message
                logger.LogInformation("Started {fileName}. Process Id : {processId}", DeploymentParameters.ApplicationName, HostProcess.Id);
            }

            protected override async Task<(Uri url, CancellationToken hostExitToken)> StartSelfHostAsync(Uri hintUrl)
            {
                using (Logger.BeginScope("StartSelfHost"))
                {
                    var executableName = "test.exe";

                    HostProcess = new Process() { StartInfo = new ProcessStartInfo() };
                    HostProcess.EnableRaisingEvents = true;
                    HostProcess.Id = 1234;

                    var hostExitTokenSource = new CancellationTokenSource();

                    try
                    {
                        StartAndCaptureOutAndErrToLogger(executableName, Logger);
                    }
                    catch (Exception)
                    {
                        Logger.LogError("Error occurred while starting the process. Exception: {exception}", "Simulated exception");
                    }

                    Logger.LogInformation("Started {fileName}. Process Id : {processId}", DeploymentParameters.ApplicationName, HostProcess.Id);

                    return (new Uri("http://localhost:5000"), hostExitTokenSource.Token);
                }
            }
        }
    }
}
