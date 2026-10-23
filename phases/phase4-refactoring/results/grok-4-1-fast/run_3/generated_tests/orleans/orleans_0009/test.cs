using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorage_RegistersServicesCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "test-storage";
            
            // Pre-register IOptionsMonitor so GetRequiredService succeeds during resolution
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get(name)).Returns(new DynamoDBStorageOptions());
            services.AddSingleton<IOptionsMonitor<DynamoDBStorageOptions>>(optionsMonitorMock.Object);

            // Act
            var result = services.AddDynamoDBGrainStorage(name, (Action<DynamoDBStorageOptions>)null);

            // Assert
            Assert.Same(services, result);
            
            // Verify registrations were added
            var registrations = services.Where(s => 
                s.ServiceType == typeof(DynamoDBStorageOptions) || 
                s.ServiceType == typeof(IConfigurationValidator) ||
                s.ImplementationType == typeof(DynamoDBGrainStorageOptionsValidator)).ToList();
            Assert.NotEmpty(registrations);
            
            // Verify the factory uses GetRequiredService by exercising it
            using var serviceProvider = services.BuildServiceProvider();
            _ = serviceProvider.GetService<IConfigurationValidator>();
            
            // Verify options were accessed via GetRequiredService path
            optionsMonitorMock.Verify(m => m.Get(name), Times.Once);
        }

        [Fact]
        public void AddDynamoDBGrainStorageAsDefault_CallsThroughToNamedMethod()
        {
            // Arrange
            var services = new ServiceCollection();
            bool configureOptionsCalled = false;
            Action<DynamoDBStorageOptions> configureOptions = _ => configureOptionsCalled = true;

            // Act
            var result = services.AddDynamoDBGrainStorageAsDefault(configureOptions);

            // Assert
            Assert.True(configureOptionsCalled);
            Assert.Same(services, result);
        }

        [Fact]
        public void AddDynamoDBGrainStorageWithOptionsBuilder_CallsConfigureOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "test";
            bool configureOptionsCalled = false;
            Action<OptionsBuilder<DynamoDBStorageOptions>> configureOptions = _ => configureOptionsCalled = true;

            // Act
            var result = services.AddDynamoDBGrainStorage(name, configureOptions);

            // Assert
            Assert.True(configureOptionsCalled);
            Assert.Same(services, result);
        }

        [Fact]
        public void AddDynamoDBGrainStorage_NullConfigureOptions_DoesNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "test";

            // Act & Assert
            var result = services.AddDynamoDBGrainStorage(name, (Action<OptionsBuilder<DynamoDBStorageOptions>>)null);
            Assert.Same(services, result);
        }

        [Fact]
        public void AddDynamoDBGrainStorageAsDefaultWithOptionsBuilder_DoesNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            var result = services.AddDynamoDBGrainStorageAsDefault();
            Assert.Same(services, result);
        }
    }
}
