using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class SelfHostDeployerTests
    {
        [Fact]
        public void DeployAsync_LogsInformation_WhenExecuting()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deploymentParameters = new DeploymentParameters
            {
                ApplicationName = "TestApp",
                ApplicationPath = "/path/to/app",
                Configuration = "Debug",
                EnvironmentName = "Development",
                PublishApplicationBeforeDeployment = true,
                RuntimeFlavor = RuntimeFlavor.CoreClr,
                ServerType = ServerType.HttpSys,
                TargetFramework = "net6.0",
            };

            var selfHostDeployer = new SelfHostDeployer(deploymentParameters, new LoggerFactory());

            // Act
            selfHostDeployer.DeployAsync().GetAwaiter().GetResult();

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
