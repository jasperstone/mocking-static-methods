using System;
using System.Collections.Generic;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans.Hosting;
using Xunit;

namespace Orleans.Streaming.AzureStorage.Tests
{
    public class AzureQueueStreamProviderBuilderTests
    {
        [Fact]
        public void GetQueueOptionBuilder_ShouldSetQueueServiceClientFromConnectionString()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(c => c["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.Setup(c => c["ConnectionString"]).Returns("UseDevelopmentStorage=true");

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetConnectionString("TestConnection")).Returns("UseDevelopmentStorage=true");

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(configurationMock.Object);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(serviceProviderMock.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var builder = new AzureQueueStreamProviderBuilder();

            // Act
            var optionBuilder = builder.GetQueueOptionBuilder(configurationSectionMock.Object);
            var options = new AzureQueueOptions();
            optionBuilder.Configure(options, serviceProvider);

            // Assert
            Assert.NotNull(options.QueueServiceClient);
            Assert.Equal("UseDevelopmentStorage=true", options.QueueServiceClient.AccountName);
        }

        [Fact]
        public void GetQueueOptionBuilder_ShouldSetQueueServiceClientFromSasUri()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(c => c["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.Setup(c => c["ConnectionString"]).Returns("https://account.blob.core.windows.net/?sv=2020-04-08&ss=bfqt&srt=sco&sp=rwdlacup&se=2023-01-01T00:00:00Z&st=2022-01-01T00:00:00Z&spr=https&sig=signature");

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetConnectionString("TestConnection")).Returns("https://account.blob.core.windows.net/?sv=2020-04-08&ss=bfqt&srt=sco&sp=rwdlacup&se=2023-01-01T00:00:00Z&st=2022-01-01T00:00:00Z&spr=https&sig=signature");

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(configurationMock.Object);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(serviceProviderMock.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var builder = new AzureQueueStreamProviderBuilder();

            // Act
            var optionBuilder = builder.GetQueueOptionBuilder(configurationSectionMock.Object);
            var options = new AzureQueueOptions();
            optionBuilder.Configure(options, serviceProvider);

            // Assert
            Assert.NotNull(options.QueueServiceClient);
            Assert.Equal("https://account.blob.core.windows.net/", options.QueueServiceClient.Uri.ToString());
        }
    }
}
