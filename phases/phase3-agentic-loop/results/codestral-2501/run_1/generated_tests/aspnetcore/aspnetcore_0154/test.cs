using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Tests
{
    public class WebHostBuilderTests
    {
        [Fact]
        public void LogWarning_WhenDuplicateHostingStartupAssemblies()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<WebHost>>();
            mockLogger.Setup(logger => logger.IsEnabled(LogLevel.Warning)).Returns(true);

            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockOptions = new Mock<WebHostOptions>();

            var assemblyNames = new List<string> { "Assembly1", "Assembly2", "Assembly1" };
            mockOptions.Setup(o => o.GetFinalHostingStartupAssemblies()).Returns(assemblyNames);

            var services = new ServiceCollection();
            services.AddSingleton(mockLogger.Object);
            var serviceProvider = services.BuildServiceProvider();

            var webHost = new WebHost(
                services,
                mockServiceProvider.Object,
                mockOptions.Object,
                mockConfiguration.Object,
                null);

            webHost._applicationServices = serviceProvider;

            // Act
            webHost.Initialize();

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("The assembly Assembly1 was specified multiple times. Hosting startup assemblies should only be specified once.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(1));
        }
    }
}
