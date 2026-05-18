using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Server.IntegrationTesting;

namespace DeploymentTests
{
    public class SelfHostDeployerTests
    {
        [Fact]
        public async Task StartSelfHostAsync_LogsExecutableAndArguments()
        {
            // Arrange
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var deploymentParameters = new DeploymentParameters
            {
                ApplicationPath = "/app/path",
                ApplicationName = "MyApp",
                PublishApplicationBeforeDeployment = false,
                RuntimeFlavor = RuntimeFlavor.CoreClr,
                ApplicationType = ApplicationType.Portable,
                ServerType = ServerType.Kestrel,
                Scheme = "http",
                StatusMessagesEnabled = false,
                EnvironmentVariables = new Dictionary<string, string>(),
                TargetFramework = null,
                RuntimeArchitecture = RuntimeArchitecture.x64
            };

            // Mock Directory.Exists and File.Exists to always return true
            var directoryExists = true;
            var fileExists = true;
            Directory.Exists = (path) => directoryExists;
            File.Exists = (path) => fileExists;

            var deployer = new SelfHostDeployer(deploymentParameters, mockLoggerFactory.Object);

            // Setup a dummy Process to avoid starting real process
            var dummyProcess = new Process();
            deployer.HostProcess = dummyProcess;

            // Act
            var hintUrl = new Uri("http://localhost:5000");
            await deployer.StartSelfHostAsync(hintUrl);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("/dummy/dotnet") && v.ToString().Contains("--urls") && v.ToString().Contains("--server")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
