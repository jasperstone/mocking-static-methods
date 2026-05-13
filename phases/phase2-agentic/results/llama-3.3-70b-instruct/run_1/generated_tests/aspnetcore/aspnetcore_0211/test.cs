using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class SelfHostDeployerTests
    {
        [Fact]
        public void DeployAsync_LogsInformationAboutExecutingProcess()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<SelfHostDeployer>();
            var mockLogger = new Mock<ILogger>();
            var deploymentParameters = new DeploymentParameters
            {
                ApplicationName = "TestApp",
                ApplicationPath = "/path/to/app",
                Configuration = "Debug",
                RuntimeFlavor = RuntimeFlavor.CoreClr,
                ServerType = ServerType.Kestrel,
                TargetFramework = "netcoreapp3.1"
            };

            var selfHostDeployer = new SelfHostDeployer(deploymentParameters, loggerFactory);

            // Act
            selfHostDeployer.DeployAsync().GetAwaiter().GetResult();

            // Assert
            mockLogger.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
