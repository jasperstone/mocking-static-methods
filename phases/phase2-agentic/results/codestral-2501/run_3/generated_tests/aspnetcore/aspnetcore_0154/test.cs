using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Hosting.Tests
{
    public class WebHostBuilderTests
    {
        [Fact]
        public void Build_WithDuplicateHostingStartupAssemblies_LogsWarning()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<WebHost>>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockServiceCollection = new Mock<IServiceCollection>();
            var mockConfiguration = new Mock<IConfiguration>();

            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILogger<WebHost>))).Returns(mockLogger.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IConfiguration))).Returns(mockConfiguration.Object);

            var options = new WebHostOptions
            {
                HostingStartupAssemblies = new List<string> { "Assembly1", "Assembly1" }
            };

            var webHostBuilder = new WebHostBuilder();
            webHostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton(mockServiceProvider.Object);
                services.AddSingleton(mockConfiguration.Object);
            });

            // Act
            var webHost = webHostBuilder.Build();

            // Assert
            mockLogger.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("The assembly Assembly1 was specified multiple times. Hosting startup assemblies should only be specified once.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }
}
