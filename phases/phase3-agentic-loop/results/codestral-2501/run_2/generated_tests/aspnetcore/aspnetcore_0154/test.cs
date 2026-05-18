using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

public class WebHostBuilderTests
{
    [Fact]
    public void LogWarning_When_DuplicateHostingStartupAssemblies()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<WebHost>>();
        var mockConfig = new Mock<IConfiguration>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockServiceCollection = new Mock<IServiceCollection>();

        var options = new WebHostOptions(mockConfig.Object)
        {
            HostingStartupAssemblies = new List<string> { "Assembly1", "Assembly1" }
        };

        var webHostBuilder = new WebHostBuilder();
        var webHost = new WebHost(
            mockServiceCollection.Object,
            mockServiceProvider.Object,
            options,
            mockConfig.Object,
            null);

        mockServiceProvider.Setup(sp => sp.GetService(typeof(ILogger<WebHost>))).Returns(mockLogger.Object);
        mockServiceProvider.Setup(sp => sp.GetService(typeof(IConfiguration))).Returns(mockConfig.Object);

        // Act
        webHost.Initialize();

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("The assembly Assembly1 was specified multiple times. Hosting startup assemblies should only be specified once.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
