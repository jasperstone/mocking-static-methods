using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class SelfHostDeployerTests
    {
        [Fact]
        public async Task StartSelfHostAsync_LogsInformationOnProcessStart()
        {
            // Arrange
            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger<SelfHostDeployer>>();
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

            var deploymentParameters = new DeploymentParameters
            {
                ServerType = ServerType.Kestrel,
                Scheme = "http",
                ApplicationBaseUriHint = "http://localhost:5000",
                StatusMessagesEnabled = true,
                RuntimeFlavor = RuntimeFlavor.CoreClr,
                ApplicationType = ApplicationType.Portable,
                Configuration = "Debug",
                TargetFramework = "netcoreapp3.1",
                PublishApplicationBeforeDeployment = false,
                ApplicationPath = "path/to/app",
                ApplicationName = "app",
                EnvironmentVariables = new Dictionary<string, string>()
            };

            var deployer = new SelfHostDeployer(deploymentParameters, loggerFactory.Object);

            // Act
            var hintUrl = new Uri("http://localhost:5000");
            var (actualUrl, hostExitToken) = await deployer.StartSelfHostAsync(hintUrl);

            // Assert
            logger.Verify(l => l.LogInformation(
                It.Is<string>(s => s.Contains("Started {fileName}. Process Id : {processId}")),
                It.IsAny<string>(),
                It.IsAny<int>()), Times.Once);
        }
    }
}
