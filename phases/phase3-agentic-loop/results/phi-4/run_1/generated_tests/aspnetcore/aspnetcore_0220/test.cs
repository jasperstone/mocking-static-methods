using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class SelfHostDeployerTests
    {
        [Fact]
        public void StartSelfHostAsync_LogsInformationOnStart()
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
                ApplicationName = "TestApp",
                ApplicationPath = "/path/to/app",
                PublishedApplicationRootPath = "/path/to/published"
            };

            var deployer = new SelfHostDeployer(deploymentParameters, loggerFactory.Object);

            // Act
            deployer.StartSelfHostAsync(new Uri("http://localhost:5000")).Wait();

            // Assert
            logger.Verify(l => l.LogInformation(
                It.Is<string>(s => s.Contains("Started {fileName}. Process Id : {processId}")),
                It.IsAny<object[]>()), Times.Once);
        }
    }
}
