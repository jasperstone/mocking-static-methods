using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting
{
    public class ApplicationDeployerTests
    {
        [Fact]
        public void TriggerHostShutdown_LogsInformationMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);

            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            // Create DeploymentParameters with minimal required properties
            var deploymentParameters = new DeploymentParameters
            {
                ServerType = ServerType.Kestrel, // Assuming this enum exists
                ApplicationPath = "test/path"
            };

            var deployerMock = new Mock<ApplicationDeployer>(deploymentParameters, loggerFactoryMock.Object)
            {
                CallBase = true
            }.Object;

            var cts = new CancellationTokenSource();

            // Act
            deployerMock.TriggerHostShutdown(cts);

            // Assert - verify LogInformation extension was called
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<FormattedLogValues>(formatted => formatted.ToString() == "Host process shutting down."),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<FormattedLogValues, Exception, string>>()),
                Times.Once);
        }
    }
}
