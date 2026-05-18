using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Server.IntegrationTesting.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting;

public class NginxDeployerTests
{
    [Fact]
    public void SetupNginx_LogsDebugMessagesWithCorrectArguments()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NginxDeployer>>();
        loggerMock.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
        loggerMock.Setup(x => x.IsEnabled(LogLevel.Trace)).Returns(false);

        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

        var deploymentParameters = new DeploymentParameters
        {
            ApplicationPath = "/tmp/app",
            ServerConfigTemplateContent = "dummy template"
        };

        var deployer = new TestableNginxDeployer(deploymentParameters, loggerFactoryMock.Object);

        // Act
        deployer.SetupNginx("http://localhost:5001", new Uri("http://localhost:5000"));

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Using PID file:") && v.ToString()!.Contains("test.pid")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Using Error Log file:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Using Access Log file:")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private class TestableNginxDeployer : NginxDeployer
    {
        public TestableNginxDeployer(DeploymentParameters deploymentParameters, ILoggerFactory loggerFactory)
            : base(deploymentParameters, loggerFactory)
        {
        }

        protected override string GetUserName() => "testuser";

        public new void SetupNginx(string redirectUri, Uri originalUri) => base.SetupNginx(redirectUri, originalUri);
    }
}
