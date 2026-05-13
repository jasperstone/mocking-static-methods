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

            var servicesMock = new Mock<IServiceCollection>();
            servicesMock
                .Setup(service => service.BuildServiceProvider())
                .Returns(new ServiceCollection().AddSingleton<IConfiguration>(configurationMock.Object).BuildServiceProvider());

            // Act
            var builder = new AzureQueueStreamProviderBuilder();
            var optionsBuilderMock = new Mock<OptionsBuilder<AzureQueueOptions>>();
            var configureAction = builder.GetQueueOptionBuilder(configurationSectionMock.Object);
            configureAction(optionsBuilderMock.Object);

            // Assert
            configurationMock.Verify(config => config.GetConnectionString("TestConnectionName"), Times.Once);
        }
    }
}
