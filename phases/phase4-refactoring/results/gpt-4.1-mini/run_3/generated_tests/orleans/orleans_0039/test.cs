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
    // Minimal stub to allow compilation
    public class AzureQueueOptions
    {
        public List<string> QueueNames { get; set; }
        public TimeSpan MessageVisibilityTimeout { get; set; }
        public QueueServiceClient QueueServiceClient { get; set; }
    }

    public class AzureQueueStreamProviderBuilderTests
    {
        [Fact]
        public void GetQueueOptionBuilder_UsesGetConnectionString_WhenConnectionNameProvidedAndConnectionStringEmpty()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(s => s["QueueNames"]).Returns((string)null);
            configurationSectionMock.Setup(s => s["MessageVisibilityTimeout"]).Returns((string)null);
            configurationSectionMock.Setup(s => s["ServiceKey"]).Returns(string.Empty);
            configurationSectionMock.Setup(s => s["ConnectionName"]).Returns("MyConnectionName");
            configurationSectionMock.Setup(s => s["ConnectionString"]).Returns(string.Empty);

            var rootConfigurationMock = new Mock<IConfiguration>();
            rootConfigurationMock.Setup(c => c.GetConnectionString("MyConnectionName")).Returns("UseDevelopmentStorage=true");

            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfigurationMock.Object);

            var options = new AzureQueueOptions();

            var optionsBuilder = new OptionsBuilder<AzureQueueOptions>(new ServiceCollection());

            var configureAction = GetQueueOptionBuilder(configurationSectionMock.Object);

            // Act
            configureAction.Invoke(optionsBuilder);
            var configureDelegate = optionsBuilder.OptionsAction;
            configureDelegate?.Invoke(options, servicesMock.Object);

            // Assert
            Assert.NotNull(options.QueueServiceClient);
            Assert.Equal("UseDevelopmentStorage=true", options.QueueServiceClient.AccountName);
        }

        private static Action<OptionsBuilder<AzureQueueOptions>> GetQueueOptionBuilder(IConfigurationSection configurationSection)
        {
            var method = typeof(AzureQueueStreamProviderBuilder)
                .GetMethod("GetQueueOptionBuilder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return (Action<OptionsBuilder<AzureQueueOptions>>)method.Invoke(null, new object[] { configurationSection });
        }
    }
}
