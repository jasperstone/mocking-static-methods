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
        public async Task StartSelfHostAsync_LogsInformationOnStart()
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
                TargetFramework = Tfm.NetCoreApp22,
                Configuration = "Debug",
                ApplicationPath = "path/to/app",
                ApplicationName = "app",
                PublishApplicationBeforeDeployment = false,
                EnvironmentVariables = new Dictionary<string, string>()
            };

            var deployer = new SelfHostDeployer(deploymentParameters, loggerFactory.Object);

            // Act
            var result = await deployer.StartSelfHostAsync(new Uri("http://localhost:5000"));

            // Assert
            logger.Verify(l => l.LogInformation(
                It.Is<string>(s => s.Contains("Started {fileName}. Process Id : {processId}")),
                It.IsAny<string>(),
                It.IsAny<int>()), Times.Once);
        }
    }
}
