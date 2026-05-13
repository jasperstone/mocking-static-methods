using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using System;
using System.Collections.Generic;
using Xunit;

namespace Orleans.Tests
{
    public class AzureQueueStreamProviderBuilderTests
    {
        [Fact]
        public void GetConnectionString_Called_When_ConnectionName_Is_Set()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(s => s["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.SetupGet(s => s["ConnectionString"]).Returns(string.Empty);

            var servicesMock = new Mock<IServiceProvider>();
            var rootConfigurationMock = new Mock<IConfiguration>();
            rootConfigurationMock.Setup(c => c.GetConnectionString("TestConnection")).Returns("TestConnectionString");
            servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfigurationMock.Object);

            var optionsBuilderMock = new Mock<OptionsBuilder<AzureQueueOptions>>();

            // Act
            var azureQueueStreamProviderBuilder = new AzureQueueStreamProviderBuilder();
            azureQueueStreamProviderBuilder.GetQueueOptionBuilder(configurationSectionMock.Object)(optionsBuilderMock.Object);

            // Assert
            rootConfigurationMock.Verify(c => c.GetConnectionString("TestConnection"), Times.Once);
        }

        [Fact]
        public void GetConnectionString_Not_Called_When_ConnectionString_Is_Set()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(s => s["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.SetupGet(s => s["ConnectionString"]).Returns("TestConnectionString");

            var servicesMock = new Mock<IServiceProvider>();
            var rootConfigurationMock = new Mock<IConfiguration>();
            servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfigurationMock.Object);

            var optionsBuilderMock = new Mock<OptionsBuilder<AzureQueueOptions>>();

            // Act
            var azureQueueStreamProviderBuilder = new AzureQueueStreamProviderBuilder();
            azureQueueStreamProviderBuilder.GetQueueOptionBuilder(configurationSectionMock.Object)(optionsBuilderMock.Object);

            // Assert
            rootConfigurationMock.Verify(c => c.GetConnectionString("TestConnection"), Times.Never);
        }
    }
}
