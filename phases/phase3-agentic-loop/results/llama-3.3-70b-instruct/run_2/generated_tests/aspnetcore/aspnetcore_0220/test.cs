using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class SelfHostDeployerTests
{
    [Fact]
    public async Task StartSelfHostAsync_LogsInformationMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var loggerFactory = new LoggerFactory();
        loggerFactory.AddProvider(new MockLoggerProvider(loggerMock.Object));
        var deploymentParameters = new DeploymentParameters();
        var selfHostDeployer = new SelfHostDeployer(deploymentParameters, loggerFactory);

        // Act
        await selfHostDeployer.StartSelfHostAsync(new Uri("http://localhost:5000"));

        // Assert
        loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task StartSelfHostAsync_CallsLogInformationWithCorrectMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var loggerFactory = new LoggerFactory();
        loggerFactory.AddProvider(new MockLoggerProvider(loggerMock.Object));
        var deploymentParameters = new DeploymentParameters();
        var selfHostDeployer = new SelfHostDeployer(deploymentParameters, loggerFactory);

        // Act
        await selfHostDeployer.StartSelfHostAsync(new Uri("http://localhost:5000"));

        // Assert
        loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s.Contains("Started")), It.IsAny<object[]>()), Times.Once);
    }
}

public class MockLoggerProvider : ILoggerProvider
{
    private readonly ILogger _logger;

    public MockLoggerProvider(ILogger logger)
    {
        _logger = logger;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _logger;
    }

    public void Dispose()
    {
    }
}
