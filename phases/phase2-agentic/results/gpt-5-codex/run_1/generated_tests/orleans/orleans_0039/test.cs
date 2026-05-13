using System;
using System.Collections.Generic;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Streaming.AzureStorage.Hosting;
using Xunit;

namespace Orleans.Streaming.AzureStorage.Tests.Hosting
{
    public class ConfigurationExtensionsTests
    {
        [Fact]
        public void ConfigureAzureQueue_ShouldUseConnectionStringFromConfiguration()
        {
            // Arrange
            const string connectionName = "MyConnection";
            const string expectedConnectionString = "UseDevelopmentStorage=true";
            var configurationData = new Dictionary<string, string?>
            {
                ["AzureQueue:ConnectionName"] = connectionName
            };

            var rootConfigurationData = new Dictionary<string, string?>
            {
                [$"ConnectionStrings:{connectionName}"] = expectedConnectionString
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationData)
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
                .AddInMemoryCollection(rootConfigurationData)
                .Build());

            services.AddSingleton<QueueServiceClient>(sp =>
                throw new InvalidOperationException("Should not be called during this test"));

            var providerBuilder = services.AddAzureQueueStreams(
                "AzureQueue",
                builder => { });

            // Act
            providerBuilder.ConfigureAzureQueue(configuration.GetSection("AzureQueue"));
            var serviceProvider = services.BuildServiceProvider();
            var options = new AzureQueueOptions();

            var postConfigure = Assert.Single(
                serviceProvider.GetServices<IPostConfigureOptions<AzureQueueOptions>>());

            postConfigure.PostConfigure("AzureQueue", options);

            // Assert
            Assert.NotNull(options.QueueServiceClient);
            Assert.Equal(expectedConnectionString, options.QueueServiceClient.Uri.ToString());
        }
    }
}
