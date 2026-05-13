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
            var mockOptions = new Mock<WebHostOptions>();

            var hostingStartupAssemblies = new List<string> { "Assembly1", "Assembly1" };
            mockOptions.Setup(o => o.GetFinalHostingStartupAssemblies()).Returns(hostingStartupAssemblies);

            var webHostBuilder = new WebHostBuilder();
            webHostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton(mockLogger.Object);
                services.AddSingleton(mockServiceProvider.Object);
                services.AddSingleton(mockServiceCollection.Object);
                services.AddSingleton(mockConfiguration.Object);
                services.AddSingleton(mockOptions.Object);
            });

            // Act
            webHostBuilder.Build();

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
}
