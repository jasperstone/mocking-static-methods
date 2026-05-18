using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorage_CallsGetRequiredService_WhenRegisteringValidator()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockOptionsMonitor = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            mockOptionsMonitor.Setup(m => m.Get("testName")).Returns(new DynamoDBStorageOptions());
            services.AddSingleton<IOptionsMonitor<DynamoDBStorageOptions>>(mockOptionsMonitor.Object);

            // Act - Use the overload with Action<DynamoDBStorageOptions> to avoid ambiguity
            services.AddDynamoDBGrainStorage("testName", (Action<DynamoDBStorageOptions>)null);

            // Assert - Verify that GetRequiredService was called by resolving the validator
            var serviceProvider = services.BuildServiceProvider();
            
            var validators = serviceProvider.GetServices<Orleans.Runtime.IConfigurationValidator>();
            Assert.NotEmpty(validators);
            
            // Verify the mock was used (Get was called)
            mockOptionsMonitor.Verify(m => m.Get("testName"), Times.Once);
        }

        [Fact]
        public void AddDynamoDBGrainStorageAsDefault_CallsGetRequiredService_WhenRegisteringValidator()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockOptionsMonitor = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            mockOptionsMonitor.Setup(m => m.Get(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME)).Returns(new DynamoDBStorageOptions());
            services.AddSingleton<IOptionsMonitor<DynamoDBStorageOptions>>(mockOptionsMonitor.Object);

            // Act
            services.AddDynamoDBGrainStorageAsDefault();

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var validators = serviceProvider.GetServices<Orleans.Runtime.IConfigurationValidator>();
            Assert.NotEmpty(validators);
            
            mockOptionsMonitor.Verify(m => m.Get(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME), Times.Once);
        }

        [Fact]
        public void AddDynamoDBGrainStorage_WithOptionsBuilder_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockOptionsMonitor = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            mockOptionsMonitor.Setup(m => m.Get("testName")).Returns(new DynamoDBStorageOptions());
            services.AddSingleton<IOptionsMonitor<DynamoDBStorageOptions>>(mockOptionsMonitor.Object);

            // Act - Use explicit null for OptionsBuilder overload
            services.AddDynamoDBGrainStorage("testName", (Action<OptionsBuilder<DynamoDBStorageOptions>>)null);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var validators = serviceProvider.GetServices<Orleans.Runtime.IConfigurationValidator>();
            Assert.NotEmpty(validators);
        }

        [Fact]
        public void AddDynamoDBGrainStorage_MissingIOptionsMonitor_ThrowsOnGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act 
            services.AddDynamoDBGrainStorage("testName", (Action<DynamoDBStorageOptions>)null);
            var serviceProvider = services.BuildServiceProvider();
            
            // Assert - GetRequiredService should throw InvalidOperationException when IOptionsMonitor is missing
            Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredService<Orleans.Runtime.IConfigurationValidator>());
        }
    }
}
