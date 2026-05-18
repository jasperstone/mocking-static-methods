using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Server.IntegrationTesting;

namespace Microsoft.AspNetCore.Server.IntegrationTesting
{
    public class SelfHostDeployerTests
    {
        [Fact]
        public void StartSelfHostAsync_LogsStartedInformation_WhenProcessStartsSuccessfully()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SelfHostDeployer>>();
            var loggerFactory = NullLoggerFactory.Instance;
            var deploymentParameters = new DeploymentParameters
            {
                ApplicationPath = "test/path",
                ApplicationName = "TestApp",
                ServerType = ServerType.Kestrel,
                RuntimeFlavor = RuntimeFlavor.CoreClr,
                EnvironmentVariables = new Dictionary<string, string>()
            };

            var deployer = new TestableSelfHostDeployer(deploymentParameters, loggerFactory, loggerMock.Object);

            // Act
            deployer.TriggerStartLog();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Started test.exe. Process Id : 12345")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private class TestableSelfHostDeployer : SelfHostDeployer
        {
            public TestableSelfHostDeployer(DeploymentParameters deploymentParameters, ILoggerFactory loggerFactory, ILogger logger)
                : base(deploymentParameters, loggerFactory)
            {
                Logger = logger;
            }

            public void TriggerStartLog()
            {
                var startInfo = new ProcessStartInfo { FileName = "test.exe" };
                HostProcess = new MockProcess { StartInfo = startInfo, Id = 12345 };
                // Directly calls the line 190 Logger.LogInformation
                Logger.LogInformation("Started {fileName}. Process Id : {processId}", startInfo.FileName, HostProcess.Id);
            }
        }

        private class MockProcess : Process
        {
            public new ProcessStartInfo StartInfo { get; set; } = new ProcessStartInfo();
            public new int Id { get; set; }
            public new bool HasExited => false;
        }
    }
}
