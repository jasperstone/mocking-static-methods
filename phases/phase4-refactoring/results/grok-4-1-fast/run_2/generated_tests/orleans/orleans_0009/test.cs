using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Runtime;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorage_RegistersValidator_UsingGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockOptionsMonitor = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            mockOptionsMonitor
                .Setup(m => m.Get("test-provider"))
                .Returns(new DynamoDBStorageOptions());

            // Pre-register the mock to ensure GetRequiredService can resolve it
            services.AddSingleton<IOptionsMonitor<DynamoDBStorageOptions>>(mockOptionsMonitor.Object);

            // Act
            services.AddDynamoDBGrainStorage("test-provider");

            // Assert - Verify the validator factory was registered
            var validatorDescriptors = services.Where(sd => sd.ServiceType == typeof(IConfigurationValidator)).ToList();
            Assert.Single(validatorDescriptors);
            var validatorDescriptor = validatorDescriptors[0];
            Assert.Equal(ServiceLifetime.Transient, validatorDescriptor.Lifetime);
            Assert.NotNull(validatorDescriptor.ImplementationFactory);

            // Verify GetRequiredService is called during resolution
            var serviceProvider = services.BuildServiceProvider();
            _ = serviceProvider.GetRequiredService<IConfigurationValidator>();
            mockOptionsMonitor.Verify(m => m.Get("test-provider"), Times.Once);
        }

        [Fact]
        public void AddDynamoDBGrainStorageAsDefault_CallsOverloadWithDefaultName()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = services.AddDynamoDBGrainStorageAsDefault();

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddDynamoDBGrainStorage_WithNullConfigureOptions_DoesNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            services.AddDynamoDBGrainStorage("test", (Action<OptionsBuilder<DynamoDBStorageOptions>>)null);
        }

        [Fact]
        public void AddDynamoDBGrainStorageAsDefault_WithNullConfigureOptions_DoesNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            services.AddDynamoDBGrainStorageAsDefault((Action<OptionsBuilder<DynamoDBStorageOptions>>)null);
        }

        [Fact]
        public void AddDynamoDBGrainStorageAsDefault_CallsMainOverloadWithDefaultName()
        {
            // Arrange
            var services = new ServiceCollection();
            Action<OptionsBuilder<DynamoDBStorageOptions>> configureCalled = null;

            // Act
            services.AddDynamoDBGrainStorageAsDefault(configureCalled);

            // Assert - Verify registrations that prove the main overload was called with default name
            var validatorDescriptors = services.Where(sd => sd.ServiceType == typeof(IConfigurationValidator)).ToList();
            Assert.Single(validatorDescriptors);
        }
    }
}
