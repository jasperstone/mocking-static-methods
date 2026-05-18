using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class SelfHostDeployerTests
    {
        [Fact]
        public async Task LogInformation_CalledWithExpectedMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

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
                EnvironmentVariables = new Dictionary<string, string>
                {
                    { "ASPNETCORE_CONTENTROOT", "/path/to/app" }
                }
            };

            var selfHostDeployer = new SelfHostDeployer(deploymentParameters, loggerFactoryMock.Object);

            // Act
            await selfHostDeployer.DeployAsync();

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
