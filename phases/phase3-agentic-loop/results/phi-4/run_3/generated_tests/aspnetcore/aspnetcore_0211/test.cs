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
        public void LogInformation_ShouldLogCorrectMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var deploymentParameters = new Mock<DeploymentParameters>();
            deploymentParameters.SetupGet(p => p.ApplicationName).Returns("TestApp");
            deploymentParameters.SetupGet(p => p.Configuration).Returns("Debug");
            deploymentParameters.SetupGet(p => p.TargetFramework).Returns("netcoreapp3.1");
            deploymentParameters.SetupGet(p => p.RuntimeFlavor).Returns(RuntimeFlavor.CoreClr);
            deploymentParameters.SetupGet(p => p.ApplicationType).Returns(ApplicationType.Portable);
            deploymentParameters.SetupGet(p => p.ServerType).Returns(ServerType.Kestrel);
            deploymentParameters.SetupGet(p => p.ApplicationBaseUriHint).Returns("http://localhost:5000");
            deploymentParameters.SetupGet(p => p.StatusMessagesEnabled).Returns(true);

            var deployer = new SelfHostDeployer(deploymentParameters.Object, new LoggerFactory().AddProvider(new MockProvider(loggerMock.Object)));

            // Act
            deployer.StartSelfHostAsync(new Uri("http://localhost:5000")).Wait();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }

    public class MockProvider : ILoggerProvider
    {
        private readonly ILogger _logger;

        public MockProvider(ILogger logger)
        {
            _logger = logger;
        }

        public ILogger CreateLogger(string categoryName) => _logger;

        public void Dispose() { }
    }
}
