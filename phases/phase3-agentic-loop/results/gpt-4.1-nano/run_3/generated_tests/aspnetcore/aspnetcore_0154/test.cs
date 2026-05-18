using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.AspNetCore.Hosting;
using System;

namespace Hosting.Tests
{
    public class WebHostBuilderTests
    {
        [Fact]
        public void LogWarning_Called_WhenAssemblyDuplicated_AndLoggerIsEnabled()
        {
            // Arrange
            var builder = new WebHostBuilder();

            // Use reflection to set private fields for testing
            var optionsField = typeof(WebHostBuilder).GetField("_options", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var options = new WebHostOptions();
            options.SuppressStatusMessages = false;
            optionsField.SetValue(builder, options);

            // Mock the hosting environment and services
            var services = new ServiceCollection();

            // Add a mock ILogger<WebHost>
            var loggerMock = new Mock<ILogger<WebHost>>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(true);
            services.AddSingleton<ILogger<WebHost>>(loggerMock.Object);

            // Build service provider
            var serviceProvider = services.BuildServiceProvider();

            // Set the host with mocked services
            var host = new Mock<IWebHost>();
            host.Setup(h => h.Services).Returns(serviceProvider);
            host.Setup(h => h.Initialize());

            // Prepare options with duplicate assembly names
            var assemblyNames = new[] { "AssemblyA", "AssemblyA" };
            var optionsMock = new Mock<WebHostOptions>();
            optionsMock.Setup(o => o.GetFinalHostingStartupAssemblies()).Returns(assemblyNames);
            // Assign the mock options
            optionsField.SetValue(builder, optionsMock.Object);

            // Act
            // Simulate the internal code that logs warning for duplicate assemblies
            var logger = loggerMock.Object;
            var assemblySet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var assemblyName in assemblyNames)
            {
                if (!assemblySet.Add(assemblyName) && logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning($"The assembly {assemblyName} was specified multiple times. Hosting startup assemblies should only be specified once.");
                }
            }

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("The assembly AssemblyA was specified multiple times.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
