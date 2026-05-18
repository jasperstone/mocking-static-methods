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

namespace Orleans.Streaming.AzureStorage.Tests
{
    public class AzureQueueStreamProviderBuilderTests
    {
        [Fact]
        public void GetQueueOptionBuilder_ShouldSetConnectionStringFromConfiguration()
        {
            // Arrange
            var mockConfigurationSection = new Mock<IConfigurationSection>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockServiceProvider = new Mock<IServiceProvider>();

            var connectionName = "TestConnection";
            var connectionString = "UseDevelopmentStorage=true";

            mockConfigurationSection.Setup(c => c["ConnectionName"]).Returns(connectionName);
            mockConfigurationSection.Setup(c => c["ConnectionString"]).Returns((string)null);
            mockConfiguration.Setup(c => c.GetConnectionString(connectionName)).Returns(connectionString);
            mockServiceProvider.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(mockConfiguration.Object);

            var optionsBuilder = new OptionsBuilder<AzureQueueOptions>();
            var options = new AzureQueueOptions();

            // Act
            var optionBuilder = AzureQueueStreamProviderBuilder.GetQueueOptionBuilder(mockConfigurationSection.Object);
            optionBuilder(optionsBuilder);
            optionsBuilder.Configure(options, mockServiceProvider.Object);

            // Assert
            Assert.NotNull(options.QueueServiceClient);
            Assert.Equal(connectionString, options.QueueServiceClient.AccountName);
        }
    }
}
