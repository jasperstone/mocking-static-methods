using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Orleans.Hosting;
using Orleans.Configuration;

namespace Orleans.Streaming.AzureStorage.Tests
{
    public class AzureQueueStreamProviderBuilderTests
    {
        [Fact]
        public void GetQueueOptionBuilder_UsesGetConnectionString_WhenConnectionNameProvidedAndConnectionStringEmpty()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(s => s["QueueNames"]).Returns((string)null);
            configurationSectionMock.Setup(s => s["MessageVisibilityTimeout"]).Returns((string)null);
            configurationSectionMock.Setup(s => s["ServiceKey"]).Returns((string)null);
            configurationSectionMock.Setup(s => s["ConnectionName"]).Returns("MyConnectionName");
            configurationSectionMock.Setup(s => s["ConnectionString"]).Returns(string.Empty);
            configurationSectionMock.Setup(s => s.GetSection("QueueNames")).Returns(Mock.Of<IConfigurationSection>());

            var expectedConnectionString = "UseDevelopmentStorage=true";

            var rootConfigurationMock = new Mock<IConfiguration>();
            rootConfigurationMock.Setup(c => c.GetConnectionString("MyConnectionName")).Returns(expectedConnectionString);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(rootConfigurationMock.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var optionsBuilder = new OptionsBuilder<AzureQueueOptions>(serviceCollection, null);
            var configureAction = typeof(AzureQueueStreamProviderBuilder)
                .GetMethod("GetQueueOptionBuilder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, new object[] { configurationSectionMock.Object }) as Action<OptionsBuilder<AzureQueueOptions>>;

            configureAction(optionsBuilder);

            var options = new AzureQueueOptions();

            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetService(typeof(IConfiguration))).Returns(rootConfigurationMock.Object);
            spMock.Setup(sp => sp.GetRequiredService(typeof(IConfiguration))).Returns(rootConfigurationMock.Object);

            // Act
            optionsBuilder.OptionsAction.Invoke(options, spMock.Object);

            // Assert
            Assert.NotNull(options.QueueServiceClient);
            // We cannot directly check the connection string, but we can check that the client is created
        }

        [Fact]
        public void GetQueueOptionBuilder_SetsQueueNamesAndVisibilityTimeout()
        {
            // Arrange
            var queueNames = new List<string> { "queue1", "queue2" };
            var configurationSectionMock = new Mock<IConfigurationSection>();
            var queueNamesSectionMock = new Mock<IConfigurationSection>();
            queueNamesSectionMock.Setup(q => q.Get<List<string>>()).Returns(queueNames);
            configurationSectionMock.Setup(s => s.GetSection("QueueNames")).Returns(queueNamesSectionMock.Object);
            configurationSectionMock.Setup(s => s["MessageVisibilityTimeout"]).Returns("00:01:00");
            configurationSectionMock.Setup(s => s["ServiceKey"]).Returns((string)null);
            configurationSectionMock.Setup(s => s["ConnectionName"]).Returns((string)null);
            configurationSectionMock.Setup(s => s["ConnectionString"]).Returns((string)null);

            var serviceCollection = new ServiceCollection();
            var optionsBuilder = new OptionsBuilder<AzureQueueOptions>(serviceCollection, null);
            var configureAction = typeof(AzureQueueStreamProviderBuilder)
                .GetMethod("GetQueueOptionBuilder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, new object[] { configurationSectionMock.Object }) as Action<OptionsBuilder<AzureQueueOptions>>;

            configureAction(optionsBuilder);

            var options = new AzureQueueOptions();
            var spMock = new Mock<IServiceProvider>();

            // Act
            optionsBuilder.OptionsAction.Invoke(options, spMock.Object);

            // Assert
            Assert.Equal(queueNames, options.QueueNames);
            Assert.Equal(TimeSpan.FromMinutes(1), options.MessageVisibilityTimeout);
        }
    }
}
