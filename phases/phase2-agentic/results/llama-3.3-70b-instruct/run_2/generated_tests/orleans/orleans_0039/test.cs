using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using System;
using System.Collections.Generic;
using Xunit;

namespace Orleans.Streaming.AzureStorage.Tests
{
    public class AzureQueueStreamProviderBuilderTests
    {
        [Fact]
        public void GetQueueOptionBuilder_ConfigurationSectionHasConnectionNameAndNoConnectionString_GetConnectionStringIsCalled()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(cs => cs["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.SetupGet(cs => cs["ConnectionString"]).Returns((string)null);

            var servicesMock = new Mock<IServiceProvider>();
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetConnectionString("TestConnection")).Returns("TestConnectionString");
            servicesMock.Setup(s => s.GetService(typeof(IConfiguration))).Returns(configurationMock.Object);

            // Act
            var optionsBuilder = new OptionsBuilder<AzureQueueOptions>();
            var azureQueueStreamProviderBuilder = new AzureQueueStreamProviderBuilder();
            var action = azureQueueStreamProviderBuilder.GetQueueOptionBuilder(configurationSectionMock.Object);
            action(optionsBuilder, servicesMock.Object);

            // Assert
            configurationMock.Verify(c => c.GetConnectionString("TestConnection"), Times.Once);
        }

        [Fact]
        public void GetQueueOptionBuilder_ConfigurationSectionHasConnectionString_GetConnectionStringIsNotCalled()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(cs => cs["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.SetupGet(cs => cs["ConnectionString"]).Returns("TestConnectionString");

            var servicesMock = new Mock<IServiceProvider>();
            var configurationMock = new Mock<IConfiguration>();
            servicesMock.Setup(s => s.GetService(typeof(IConfiguration))).Returns(configurationMock.Object);

            // Act
            var optionsBuilder = new OptionsBuilder<AzureQueueOptions>();
            var azureQueueStreamProviderBuilder = new AzureQueueStreamProviderBuilder();
            var action = azureQueueStreamProviderBuilder.GetQueueOptionBuilder(configurationSectionMock.Object);
            action(optionsBuilder, servicesMock.Object);

            // Assert
            configurationMock.Verify(c => c.GetConnectionString("TestConnection"), Times.Never);
        }

        [Fact]
        public void GetQueueOptionBuilder_ConfigurationSectionHasNoConnectionNameAndNoConnectionString_GetConnectionStringIsNotCalled()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(cs => cs["ConnectionName"]).Returns((string)null);
            configurationSectionMock.SetupGet(cs => cs["ConnectionString"]).Returns((string)null);

            var servicesMock = new Mock<IServiceProvider>();
            var configurationMock = new Mock<IConfiguration>();
            servicesMock.Setup(s => s.GetService(typeof(IConfiguration))).Returns(configurationMock.Object);

            // Act
            var optionsBuilder = new OptionsBuilder<AzureQueueOptions>();
            var azureQueueStreamProviderBuilder = new AzureQueueStreamProviderBuilder();
            var action = azureQueueStreamProviderBuilder.GetQueueOptionBuilder(configurationSectionMock.Object);
            action(optionsBuilder, servicesMock.Object);

            // Assert
            configurationMock.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
        }
    }
}
