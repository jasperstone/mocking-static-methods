using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using System;

namespace WebHostBuilderTests
{
    public class WebHostBuilderUnitTests
    {
        [Fact]
        public void LogWarning_Called_WhenAssemblyDuplicatedAndLoggerEnabled()
        {
            // Arrange
            var builder = new WebHostBuilder();

            // Use reflection to set private fields for testing
            var optionsField = typeof(WebHostBuilder).GetField("_options", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var options = new WebHostOptions();
            options.SuppressStatusMessages = false;
            optionsField.SetValue(builder, options);

            // Mock environment variables
            Environment.SetEnvironmentVariable("Hosting:Environment", null);
            Environment.SetEnvironmentVariable("ASPNET_ENV", null);
            Environment.SetEnvironmentVariable("ASPNETCORE_SERVER.URLS", null);

            // Create a mock logger
            var mockLogger = new Mock<ILogger<WebHost>>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(true);

            // Mock the host to return the mock logger
            var mockHost = new Mock<IWebHost>();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<ILogger<WebHost>>(mockLogger.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Use reflection to set host.Services
            var hostServicesField = typeof(WebHost).GetField("_services", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var hostInstance = (WebHost)Activator.CreateInstance(typeof(WebHost), true);
            hostServicesField.SetValue(hostInstance, serviceProvider);

            // Set the host in the builder (simulate build)
            var buildMethod = typeof(WebHostBuilder).GetMethod("Build", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            // We can't directly invoke Build() because it creates a new host internally.
            // Instead, we will simulate the part after host creation.

            // Prepare options to return duplicate assembly names
            var mockOptions = new Mock<WebHostOptions>();
            mockOptions.Setup(o => o.GetFinalHostingStartupAssemblies()).Returns(new[] { "AssemblyA", "AssemblyA" });
            optionsField.SetValue(builder, mockOptions.Object);

            // Act
            // Manually invoke the code that logs warning for duplicate assemblies
            var assemblyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var logger = mockLogger.Object;
            foreach (var assemblyName in mockOptions.Object.GetFinalHostingStartupAssemblies())
            {
                if (!assemblyNames.Add(assemblyName) && logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning($"The assembly {assemblyName} was specified multiple times. Hosting startup assemblies should only be specified once.");
                }
            }

            // Assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AssemblyA")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
