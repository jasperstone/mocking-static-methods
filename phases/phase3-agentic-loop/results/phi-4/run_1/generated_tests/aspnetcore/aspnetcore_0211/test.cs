using System;
using System.Diagnostics;
using System.Threading;
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
            var deploymentParameters = new DeploymentParameters
            {
                ApplicationName = "TestApp",
                Configuration = "Debug",
                TargetFramework = "netcoreapp3.1",
                RuntimeFlavor = RuntimeFlavor.CoreClr,
                ApplicationType = ApplicationType.Portable,
                ServerType = ServerType.Kestrel,
                ApplicationBaseUriHint = "http://localhost:5000",
                StatusMessagesEnabled = true,
                PublishApplicationBeforeDeployment = false
            };

            var deployer = new SelfHostDeployer(deploymentParameters, new LoggerFactory().AddProvider(new MockProvider(loggerMock.Object)));

            // Act
            deployer.StartSelfHostAsync(new Uri("http://localhost:5000")).Wait();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
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
