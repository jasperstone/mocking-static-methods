using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class SelfHostDeployerTests
    {
        private class TestSelfHostDeployer : SelfHostDeployer
        {
            public TestSelfHostDeployer(DeploymentParameters deploymentParameters, ILoggerFactory loggerFactory)
                : base(deploymentParameters, loggerFactory)
            {
            }

            public new Task<(Uri url, System.Threading.CancellationToken hostExitToken)> StartSelfHostAsync(Uri hintUrl)
            {
                return base.StartSelfHostAsync(hintUrl);
            }
        }

        [Fact]
        public async Task StartSelfHostAsync_LogsExecutingInformation()
        {
            // Arrange
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockLogger = new Mock<ILogger>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var deploymentParameters = new DeploymentParameters
            {
                ApplicationName = "TestApp",
                ApplicationPath = "TestAppPath",
                Configuration = "Debug",
                EnvironmentVariables = new Dictionary<string, string>(),
                PublishApplicationBeforeDeployment = false,
                RuntimeFlavor = RuntimeFlavor.Clr,
                ApplicationType = ApplicationType.Portable,
                ServerType = ServerType.Kestrel,
                TargetFramework = "net6.0"
            };

            var deployer = new TestSelfHostDeployer(deploymentParameters, mockLoggerFactory.Object);

            var hintUrl = new Uri("http://localhost:5000");

            // Act
            try
            {
                await deployer.StartSelfHostAsync(hintUrl);
            }
            catch
            {
                // Expected exceptions due to process start failure in test environment
            }

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().StartsWith("Executing")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
