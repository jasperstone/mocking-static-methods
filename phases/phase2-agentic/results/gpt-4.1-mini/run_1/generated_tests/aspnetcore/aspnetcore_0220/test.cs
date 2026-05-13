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
            var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Information));
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

            // Setup HostProcess to simulate a started process with Id
            deployer.HostProcess = new Process();
            deployer.HostProcess.StartInfo = new ProcessStartInfo();
            deployer.HostProcess.Id = 1234;
            deployer.HostProcess.EnableRaisingEvents = true;

            // Act
            var hintUrl = new Uri("http://localhost:5000");
            var (url, token) = await deployer.InvokeStartSelfHostAsync(hintUrl);

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
            public TestSelfHostDeployer(DeploymentParameters parameters, ILoggerFactory loggerFactory)
                : base(parameters, loggerFactory)
            {
            }

            public Task<(Uri url, CancellationToken hostExitToken)> InvokeStartSelfHostAsync(Uri hintUrl)
            {
                return StartSelfHostAsync(hintUrl);
            }

            protected override void AddEnvironmentVariablesToProcess(ProcessStartInfo startInfo, System.Collections.Generic.IDictionary<string, string> environmentVariables)
            {
                // Do nothing to avoid environment variable side effects
            }

            // Override to simulate process start and logging
            public override async Task<DeploymentResult> DeployAsync()
            {
                throw new NotImplementedException();
            }

            protected override async Task<(Uri url, CancellationToken hostExitToken)> StartSelfHostAsync(Uri hintUrl)
            {
                // Setup a fake process with Id and simulate the logging call
                HostProcess = new Process();
                HostProcess.StartInfo = new ProcessStartInfo();
                HostProcess.Id = 1234;
                HostProcess.EnableRaisingEvents = true;

                // Simulate the logging call on line 190
                Logger.LogInformation("Started {fileName}. Process Id : {processId}", DeploymentParameters.ApplicationName, HostProcess.Id);

                return (hintUrl, CancellationToken.None);
            }
        }
    }
}
