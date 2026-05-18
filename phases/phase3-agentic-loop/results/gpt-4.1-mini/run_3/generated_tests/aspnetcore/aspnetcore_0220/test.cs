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
                applicationName: "test",
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

            // Setup the HostProcess to simulate a process with Id
            deployer.SetHostProcessId(1234);

            // Act
            var result = await deployer.InvokeStartSelfHostAsync(new Uri("http://localhost:5000"));

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Started test. Process Id : 1234")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class TestSelfHostDeployer : SelfHostDeployer
        {
            private Process _hostProcess;

            public TestSelfHostDeployer(DeploymentParameters deploymentParameters, ILoggerFactory loggerFactory)
                : base(deploymentParameters, loggerFactory)
            {
            }

            public async Task<(Uri url, CancellationToken hostExitToken)> InvokeStartSelfHostAsync(Uri hintUrl)
            {
                return await StartSelfHostAsync(hintUrl);
            }

            public void SetHostProcessId(int id)
            {
                _hostProcess = new Process();
                // Use reflection to set the private readonly Id property is not possible,
                // so we mock the Process by overriding HostProcess property.
                // Instead, override HostProcess property to return a mock with Id.
                HostProcess = new ProcessMock(id);
            }

            protected override void AddEnvironmentVariablesToProcess(ProcessStartInfo startInfo, System.Collections.Generic.IDictionary<string, string> environmentVariables)
            {
                // Do nothing to avoid side effects
            }

            protected override string GetDotNetExeForArchitecture()
            {
                return "dotnet";
            }

            protected override void StartProcess(Process process, string executableName, ILogger logger)
            {
                // Do nothing to avoid real process start
            }

            public override Process HostProcess
            {
                get => _hostProcess;
                protected set => _hostProcess = value;
            }
        }

        private class ProcessMock : Process
        {
            private readonly int _id;

            public ProcessMock(int id)
            {
                _id = id;
            }

            public override int Id => _id;
        }
    }
}
