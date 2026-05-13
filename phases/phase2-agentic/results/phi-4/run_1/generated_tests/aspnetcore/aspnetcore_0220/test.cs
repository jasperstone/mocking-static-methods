using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class SelfHostDeployerTests
    {
        [Fact]
        public async void StartSelfHostAsync_LogsInformationOnProcessStart()
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
                ApplicationPath = "path/to/application",
                ApplicationName = "TestApp"
            };

            var deployer = new SelfHostDeployer(deploymentParameters, loggerFactory.Object);

            // Act
            var result = await deployer.StartSelfHostAsync(new Uri("http://localhost:5000"));

            // Assert
            logger.Verify(l => l.LogInformation("Started {fileName}. Process Id : {processId}", It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }
    }
}
