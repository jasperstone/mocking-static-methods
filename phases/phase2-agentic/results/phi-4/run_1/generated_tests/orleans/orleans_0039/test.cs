using System;
using System.Collections.Generic;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class AzureQueueStreamProviderBuilderTests
    {
        [Fact]
        public void GetQueueOptionBuilder_UsesGetConnectionString_WhenConnectionNameProvided()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock
                .SetupGet(section => section["ConnectionName"])
                .Returns("TestConnectionName");
            configurationSectionMock
                .SetupGet(section => section["ConnectionString"])
                .Returns(string.Empty);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock
                .Setup(config => config.GetConnectionString("TestConnectionName"))
                .Returns("TestConnectionString");

            var serviceCollectionMock = new Mock<IServiceCollection>();
            serviceCollectionMock
                .Setup(service => service.BuildServiceProvider())
                .Returns(new ServiceCollection().BuildServiceProvider());

            // Act
            var builder = new AzureQueueStreamProviderBuilder();
            var optionBuilder = builder.GetQueueOptionBuilder(configurationSectionMock.Object);
            var optionsBuilder = new OptionsBuilder<AzureQueueOptions>();
            optionBuilder(optionsBuilder);

            // Assert
            var options = optionsBuilder.Options;
            Assert.NotNull(options.QueueServiceClient);
            Assert.IsType<QueueServiceClient>(options.QueueServiceClient);
            Assert.Equal("TestConnectionString", ((QueueServiceClient)options.QueueServiceClient).Uri.ToString());
        }
    }
}
