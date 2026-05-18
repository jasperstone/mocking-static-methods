using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Server.IntegrationTesting;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class SelfHostDeployerTests
    {
        [Fact]
        public async Task LogInformation_ShouldLogExpectedMessage()
        {
            // Arrange
            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger<SelfHostDeployer>>();
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

            var deploymentParameters = new DeploymentParameters
            {
                ApplicationName = "TestApp",
                Configuration = "Debug",
                RuntimeFlavor = RuntimeFlavor.CoreClr,
                ApplicationType = ApplicationType.Portable,
                ServerType = ServerType.Kestrel,
                ApplicationBaseUriHint = "http://localhost:5000",
                StatusMessagesEnabled = true,
                ApplicationPath = "/path/to/app",
                TargetFramework = "netcoreapp3.1"
            };

            var deployer = new SelfHostDeployer(deploymentParameters, loggerFactory.Object);

            var hintUrl = new Uri("http://localhost:5000");

            // Act
            await deployer.StartSelfHostAsync(hintUrl);

            // Assert
            logger.Verify(
                l => l.Log(
                    It.Is<LogLevel>(level => level == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
