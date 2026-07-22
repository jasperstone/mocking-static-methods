using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;
using Xunit;

namespace Orleans.Persistence.DynamoDB.Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorage_ShouldAddTransientConfigurationValidator()
        {
            // Arrange
            var services = new ServiceCollection();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            services.AddSingleton(optionsMonitorMock.Object);

            // Act
            services.AddDynamoDBGrainStorage("TestStorage", (Action<OptionsBuilder<DynamoDBStorageOptions>>)null);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var configurationValidator = serviceProvider.GetRequiredService<IConfigurationValidator>();
            Assert.NotNull(configurationValidator);
        }

        [Fact]
        public void AddDynamoDBGrainStorage_ShouldAddTransientPostConfigureOptions()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddDynamoDBGrainStorage("TestStorage", (Action<OptionsBuilder<DynamoDBStorageOptions>>)null);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var postConfigureOptions = serviceProvider.GetRequiredService<IPostConfigureOptions<DynamoDBStorageOptions>>();
            Assert.NotNull(postConfigureOptions);
        }

        [Fact]
        public void AddDynamoDBGrainStorage_ShouldAddGrainStorage()
        {
            // Arrange
            var services = new ServiceCollection();
            var grainStorageMock = new Mock<IGrainStorage>();
            services.AddSingleton(grainStorageMock.Object);

            // Act
            services.AddDynamoDBGrainStorage("TestStorage", (Action<OptionsBuilder<DynamoDBStorageOptions>>)null);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var grainStorage = serviceProvider.GetRequiredService<IGrainStorage>();
            Assert.NotNull(grainStorage);
        }
    }
}
