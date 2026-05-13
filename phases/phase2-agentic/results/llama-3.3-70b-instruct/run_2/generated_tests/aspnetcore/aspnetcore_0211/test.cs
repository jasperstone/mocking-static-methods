using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

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
                TargetFramework = "netcoreapp3.1",
                RuntimeFlavor = RuntimeFlavor.CoreClr,
                ApplicationType = ApplicationType.Portable,
                ServerType = ServerType.Kestrel,
                Scheme = "https",
                ApplicationBaseUriHint = "https://localhost:5001",
                StatusMessagesEnabled = true,
                EnvironmentName = "Development",
                PublishApplicationBeforeDeployment = true,
                PreservePublishedApplicationForDebugging = false,
                UserAdditionalCleanup = null,
                EnvironmentVariables = new Dictionary<string, string>
                {
                    { "ASPNETCORE_CONTENTROOT", "/path/to/app" }
                }
            };

            var selfHostDeployer = new SelfHostDeployer(deploymentParameters, loggerFactory);

            // Act
            selfHostDeployer.DeployAsync().GetAwaiter().GetResult();

            // Assert
            mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }
    }
}
