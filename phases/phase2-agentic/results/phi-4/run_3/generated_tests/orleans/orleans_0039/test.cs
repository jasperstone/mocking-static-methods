using System;
using System.Collections.Generic;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using Xunit;

namespace Orleans.Streaming.AzureStorage.Hosting.Tests
{
    public class AzureQueueStreamProviderBuilderTests
    {
        [Fact]
        public void GetQueueOptionBuilder_UsesConnectionStringFromConfiguration()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock
                .SetupGet(section => section["ConnectionName"])
                .Returns("TestConnection");

            configurationSectionMock
                .SetupGet(section => section["ConnectionString"])
                .Returns(string.Empty);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock
                .Setup(config => config.GetConnectionString("TestConnection"))
                .Returns("TestConnectionString");

            var servicesMock = new Mock<IServiceCollection>();
            servicesMock
                .Setup(service => service.BuildServiceProvider())
                .Returns(new ServiceCollection()
                    .AddSingleton<IConfiguration>(configurationMock.Object)
                    .BuildServiceProvider());

            // Act
            var builder = new AzureQueueStreamProviderBuilder();
            var optionsBuilder = new OptionsBuilder<AzureQueueOptions>();
            var configureAction = builder.GetQueueOptionBuilder(configurationSectionMock.Object);
            configureAction(optionsBuilder);

            // Assert
            var serviceProvider = optionsBuilder.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<AzureQueueOptions>>().Value;
            Assert.NotNull(options.QueueServiceClient);
            Assert.IsType<QueueServiceClient>(options.QueueServiceClient);
            var client = (QueueServiceClient)options.QueueServiceClient;
            Assert.Equal("TestConnectionString", client.Uri.ToString());
        }
    }
}
