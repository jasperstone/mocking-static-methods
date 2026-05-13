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
        public void LogInformation_ShouldBeCalled_WithExpectedMessage()
        {
            // Arrange
            var loggerFactory = new Mock<ILoggerFactory>();
            var logger = new Mock<ILogger<SelfHostDeployer>>();
            loggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(logger.Object);

            var deploymentParameters = new DeploymentParameters
            {
                ApplicationName = "TestApp",
                Configuration = "Debug",
                ApplicationPath = "/path/to/app",
                RuntimeFlavor = RuntimeFlavor.CoreClr,
                ApplicationType = ApplicationType.Portable,
                ServerType = ServerType.Kestrel,
                TargetFramework = "netcoreapp3.1",
                ApplicationBaseUriHint = "http://localhost:5000",
                StatusMessagesEnabled = true
            };

            var deployer = new SelfHostDeployer(deploymentParameters, loggerFactory.Object);

            var hintUrl = new Uri("http://localhost:5000");

            // Act
            deployer.StartSelfHostAsync(hintUrl).Wait();

            // Assert
            logger.Verify(
                l => l.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
