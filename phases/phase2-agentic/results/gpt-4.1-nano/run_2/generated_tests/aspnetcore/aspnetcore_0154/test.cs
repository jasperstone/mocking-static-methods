using System;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;

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

            var configField = typeof(WebHostBuilder).GetField("_config", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var config = new ConfigurationBuilder().Build();
            configField.SetValue(builder, config);

            // Create a mock logger
            var mockLogger = new Mock<ILogger<WebHost>>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(true);

            // Create a mock WebHost to inject the logger
            var mockHost = new Mock<WebHost>();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<ILogger<WebHost>>(mockLogger.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Use reflection to set the host's Services property
            var hostType = typeof(WebHost);
            var servicesProperty = hostType.GetProperty("Services", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var hostInstance = (WebHost)Activator.CreateInstance(hostType, true);
            servicesProperty.SetValue(hostInstance, serviceProvider);

            // Set the host in the builder via reflection
            var hostField = typeof(WebHostBuilder).GetField("host", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            hostField.SetValue(builder, hostInstance);

            // Prepare options to return duplicate assembly names
            var assemblyNames = new[] { "AssemblyA", "AssemblyA" };
            var getFinalAssembliesMethod = typeof(WebHostOptions).GetMethod("GetFinalHostingStartupAssemblies", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var optionsInstance = new WebHostOptions();
            // Mock GetFinalHostingStartupAssemblies to return duplicate names
            var mockOptions = new Moq.Mock<WebHostOptions>();
            mockOptions.Setup(o => o.GetFinalHostingStartupAssemblies()).Returns(assemblyNames);
            optionsField.SetValue(builder, mockOptions.Object);

            // Act
            var exception = Record.Exception(() => builder.Build());

            // Assert
            mockLogger.Verify(l => l.LogWarning(It.Is<string>(s => s.Contains("AssemblyA"))), Times.Once);
        }
    }
}
