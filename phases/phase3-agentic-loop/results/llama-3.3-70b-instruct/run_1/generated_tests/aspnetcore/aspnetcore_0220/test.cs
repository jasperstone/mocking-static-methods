using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class SelfHostDeployerTests
    {
        [Fact]
        public async Task StartSelfHostAsync_LogsInformation_WhenHostProcessStarts()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<SelfHostDeployer>();
            var mockLogger = new Mock<ILogger<SelfHostDeployer>>();
            mockLogger.Setup(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>())).Verifiable();

            var deploymentParameters = new DeploymentParameters();
            var selfHostDeployer = new SelfHostDeployer(deploymentParameters, loggerFactory);

            // Act
            try
            {
                await selfHostDeployer.DeployAsync();
            }
            catch (Exception ex)
            {
                // Ignore exception
            }

            // Assert
            mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task StartSelfHostAsync_LogsError_WhenHostProcessFailsToStart()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<SelfHostDeployer>();
            var mockLogger = new Mock<ILogger<SelfHostDeployer>>();
            mockLogger.Setup(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>())).Verifiable();

            var deploymentParameters = new DeploymentParameters();
            var selfHostDeployer = new SelfHostDeployer(deploymentParameters, loggerFactory);

            // Act
            try
            {
                await selfHostDeployer.DeployAsync();
            }
            catch (Exception ex)
            {
                // Ignore exception
            }

            // Assert
            mockLogger.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task StartSelfHostAsync_LogsInformation_WhenHostProcessShutsDown()
        {
            // Arrange
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<SelfHostDeployer>();
            var mockLogger = new Mock<ILogger<SelfHostDeployer>>();
            mockLogger.Setup(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>())).Verifiable();

            var deploymentParameters = new DeploymentParameters();
            var selfHostDeployer = new SelfHostDeployer(deploymentParameters, loggerFactory);

            // Act
            try
            {
                await selfHostDeployer.DeployAsync();
            }
            catch (Exception ex)
            {
                // Ignore exception
            }

            // Assert
            mockLogger.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
