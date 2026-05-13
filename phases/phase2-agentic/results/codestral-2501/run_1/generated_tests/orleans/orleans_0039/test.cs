using System;
using System.Collections.Generic;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class AzureQueueStreamProviderBuilderTests
    {
        [Fact]
        public void GetQueueOptionBuilder_ShouldSetConnectionStringFromConfiguration()
        {
            // Arrange
            var mockConfigurationSection = new Mock<IConfigurationSection>();
            mockConfigurationSection.Setup(c => c["ConnectionName"]).Returns("TestConnection");
            mockConfigurationSection.Setup(c => c["ConnectionString"]).Returns((string)null);

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c.GetConnectionString("TestConnection")).Returns("TestConnectionString");

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IConfiguration>()).Returns(mockConfiguration.Object);

            var mockServiceCollection = new ServiceCollection();
            mockServiceCollection.AddSingleton(mockServiceProvider.Object);
            var serviceProvider = mockServiceCollection.BuildServiceProvider();

            var optionsBuilder = new OptionsBuilder<AzureQueueOptions>();
            optionsBuilder.Services.AddSingleton(serviceProvider);

            // Act
            var optionBuilder = AzureQueueStreamProviderBuilder.GetQueueOptionBuilder(mockConfigurationSection.Object);
            optionBuilder(optionsBuilder);

            // Assert
            var options = optionsBuilder.Value;
            Assert.NotNull(options.QueueServiceClient);
            Assert.Equal("TestConnectionString", options.QueueServiceClient.AccountName);
        }
    }
}
