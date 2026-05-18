using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class NginxDeployerTests
    {
        [Fact]
        public void LogTrace_CalledWithConfigContent_WhenLogLevelIsTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var deploymentParameters = new DeploymentParameters();
            var nginxDeployer = new NginxDeployer(deploymentParameters, loggerFactoryMock.Object);

            // Act
            nginxDeployer.SetupNginx("http://localhost:5000", new Uri("http://localhost:5000"));

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                (Func<object, Exception, string>)((state, exception) => state.ToString())), Times.Once);
        }

        [Fact]
        public void LogTrace_NotCalled_WhenLogLevelIsNotTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);
            var deploymentParameters = new DeploymentParameters();
            var nginxDeployer = new NginxDeployer(deploymentParameters, loggerFactoryMock.Object);

            // Act
            nginxDeployer.SetupNginx("http://localhost:5000", new Uri("http://localhost:5000"));

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                (Func<object, Exception, string>)((state, exception) => state.ToString())), Times.Never);
        }
    }
}
