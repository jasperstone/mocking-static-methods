using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans.Hosting;
using Orleans.Providers;
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

            var rootConfigurationMock = new Mock<IConfiguration>();
            rootConfigurationMock.Setup(c => c.GetConnectionString("TestConnection")).Returns("TestConnectionString");

            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfigurationMock.Object);

            // Act
            var optionsBuilder = new OptionsBuilder<AzureQueueOptions>();
            var azureQueueStreamProviderBuilder = new AzureQueueStreamProviderBuilder();
            var action = azureQueueStreamProviderBuilder.GetQueueOptionBuilder(configurationSectionMock.Object);
            action(optionsBuilder.Configure(servicesMock.Object));

            // Assert
            rootConfigurationMock.Verify(c => c.GetConnectionString("TestConnection"), Times.Once);
        }

        [Fact]
        public void GetConnectionString_NotCalled_When_ConnectionString_Is_Set()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(s => s["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.SetupGet(s => s["ConnectionString"]).Returns("TestConnectionString");

            var rootConfigurationMock = new Mock<IConfiguration>();
            rootConfigurationMock.Setup(c => c.GetConnectionString("TestConnection")).Returns("TestConnectionString");

            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfigurationMock.Object);

            // Act
            var optionsBuilder = new OptionsBuilder<AzureQueueOptions>();
            var azureQueueStreamProviderBuilder = new AzureQueueStreamProviderBuilder();
            var action = azureQueueStreamProviderBuilder.GetQueueOptionBuilder(configurationSectionMock.Object);
            action(optionsBuilder.Configure(servicesMock.Object));

            // Assert
            rootConfigurationMock.Verify(c => c.GetConnectionString("TestConnection"), Times.Never);
        }

        [Fact]
        public void GetConnectionString_NotCalled_When_ConnectionName_Is_Empty()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(s => s["ConnectionName"]).Returns(string.Empty);
            configurationSectionMock.SetupGet(s => s["ConnectionString"]).Returns(string.Empty);

            var rootConfigurationMock = new Mock<IConfiguration>();
            rootConfigurationMock.Setup(c => c.GetConnectionString("TestConnection")).Returns("TestConnectionString");

            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfigurationMock.Object);

            // Act
            var optionsBuilder = new OptionsBuilder<AzureQueueOptions>();
            var azureQueueStreamProviderBuilder = new AzureQueueStreamProviderBuilder();
            var action = azureQueueStreamProviderBuilder.GetQueueOptionBuilder(configurationSectionMock.Object);
            action(optionsBuilder.Configure(servicesMock.Object));

            // Assert
            rootConfigurationMock.Verify(c => c.GetConnectionString(It.IsAny<string>()), Times.Never);
        }
    }
}
