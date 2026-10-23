using Xunit;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using Azure.Storage.Queues;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Orleans.Streaming.AzureStorage;

namespace Orleans.Streaming.AzureStorage.Tests
{
    public class AzureQueueStreamProviderBuilderTests
    {
        [Fact]
        public void GetQueueOptionBuilder_ConfigurationSectionHasConnectionName_GetConnectionStringIsCalled()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(cs => cs["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.SetupGet(cs => cs["ConnectionString"]).Returns(string.Empty);

            var servicesMock = new Mock<IServiceProvider>();
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetConnectionString("TestConnection")).Returns("TestConnectionString");

            servicesMock.Setup(s => s.GetService(typeof(IConfiguration))).Returns(configurationMock.Object);
            servicesMock.Setup(s => s.GetService(typeof(IServiceProvider))).Returns(servicesMock.Object);

            // Act
            var azureQueueStreamProviderBuilder = new AzureQueueStreamProviderBuilder();
            var optionsBuilder = new OptionsBuilder<AzureQueueOptions>(servicesMock.Object, "TestName");
            azureQueueStreamProviderBuilder.GetQueueOptionBuilder(configurationSectionMock.Object)(optionsBuilder);

            // Assert
            configurationMock.Verify(c => c.GetConnectionString("TestConnection"), Times.Once);
        }
    }
}
