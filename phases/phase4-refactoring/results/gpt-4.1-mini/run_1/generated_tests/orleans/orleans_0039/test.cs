using System;
using System.Collections.Generic;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Hosting;
using Xunit;

namespace Orleans.Streaming.AzureStorage.Tests
{
    public class AzureQueueStreamProviderBuilderTests
    {
        [Fact]
        public void ConfigureOptions_UsesGetConnectionString_WhenConnectionNameProvidedAndConnectionStringEmpty()
        {
            // Arrange
            var connectionName = "MyConnectionName";
            var expectedConnectionString = "UseDevelopmentStorage=true";

            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(s => s["ServiceKey"]).Returns(string.Empty);
            configurationSectionMock.Setup(s => s["ConnectionName"]).Returns(connectionName);
            configurationSectionMock.Setup(s => s["ConnectionString"]).Returns(string.Empty);
            configurationSectionMock.Setup(s => s.GetSection("QueueNames")).Returns((IConfigurationSection)null);
            configurationSectionMock.Setup(s => s["MessageVisibilityTimeout"]).Returns(string.Empty);

            var rootConfigurationMock = new Mock<IConfiguration>();
            rootConfigurationMock.Setup(c => c.GetConnectionString(connectionName)).Returns(expectedConnectionString);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IConfiguration))).Returns(rootConfigurationMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(IConfiguration))).Returns(rootConfigurationMock.Object);

            var options = new AzureQueueOptions();

            // Act
            var configureAction = typeof(AzureQueueStreamProviderBuilder)
                .GetMethod("GetQueueOptionBuilder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, new object[] { configurationSectionMock.Object }) as Action<OptionsBuilder<AzureQueueOptions>>;

            var optionsBuilder = new OptionsBuilder<AzureQueueOptions>(new ServiceCollection(), null);
            configureAction(optionsBuilder);

            // The configure delegate is the one registered on optionsBuilder
            // We invoke it directly with the options instance and the mocked service provider
            var configureDelegate = optionsBuilder.ConfigureActions[0];
            configureDelegate(options, serviceProviderMock.Object);

            // Assert
            Assert.NotNull(options.QueueServiceClient);
            Assert.Equal(expectedConnectionString, options.QueueServiceClient.AccountName);
        }

        [Fact]
        public void ConfigureOptions_UsesConnectionStringDirectly_WhenProvided()
        {
            // Arrange
            var connectionString = "UseDevelopmentStorage=true";

            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(s => s["ServiceKey"]).Returns(string.Empty);
            configurationSectionMock.Setup(s => s["ConnectionName"]).Returns(string.Empty);
            configurationSectionMock.Setup(s => s["ConnectionString"]).Returns(connectionString);
            configurationSectionMock.Setup(s => s.GetSection("QueueNames")).Returns((IConfigurationSection)null);
            configurationSectionMock.Setup(s => s["MessageVisibilityTimeout"]).Returns(string.Empty);

            var serviceProviderMock = new Mock<IServiceProvider>();

            var options = new AzureQueueOptions();

            // Act
            var configureAction = typeof(AzureQueueStreamProviderBuilder)
                .GetMethod("GetQueueOptionBuilder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, new object[] { configurationSectionMock.Object }) as Action<OptionsBuilder<AzureQueueOptions>>;

            var optionsBuilder = new OptionsBuilder<AzureQueueOptions>(new ServiceCollection(), null);
            configureAction(optionsBuilder);

            var configureDelegate = optionsBuilder.ConfigureActions[0];
            configureDelegate(options, serviceProviderMock.Object);

            // Assert
            Assert.NotNull(options.QueueServiceClient);
            Assert.Equal(connectionString, options.QueueServiceClient.AccountName);
        }
    }
}
