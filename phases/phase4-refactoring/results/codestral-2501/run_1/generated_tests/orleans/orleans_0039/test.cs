using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers;
using Xunit;
using Azure.Storage.Queues;

namespace Orleans.Hosting.Tests
{
    public class AzureQueueStreamProviderBuilderWrapper
    {
        public static Action<OptionsBuilder<AzureQueueOptions>> GetQueueOptionBuilder(IConfigurationSection configurationSection, IServiceCollection services)
        {
            var optionsBuilder = new OptionsBuilder<AzureQueueOptions>(services);
            var optionBuilder = AzureQueueStreamProviderBuilder.GetQueueOptionBuilder(configurationSection);
            optionBuilder(optionsBuilder);
            return optionsBuilder;
        }
    }

    public class AzureQueueStreamProviderBuilderTests
    {
        [Fact]
        public void GetQueueOptionBuilder_ShouldConfigureQueueNames()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(c => c.GetSection("QueueNames")).Returns(new Mock<IConfigurationSection>().Object);
            configurationSectionMock.Setup(c => c.GetSection("QueueNames").Get<List<string>>()).Returns(new List<string> { "queue1", "queue2" });

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

            // Act
            var optionBuilder = AzureQueueStreamProviderBuilderWrapper.GetQueueOptionBuilder(configurationSectionMock.Object, services);
            var options = optionBuilder.Build();

            // Assert
            Assert.Equal(new List<string> { "queue1", "queue2" }, options.QueueNames);
        }

        [Fact]
        public void GetQueueOptionBuilder_ShouldConfigureMessageVisibilityTimeout()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(c => c["MessageVisibilityTimeout"]).Returns("00:01:00");

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

            // Act
            var optionBuilder = AzureQueueStreamProviderBuilderWrapper.GetQueueOptionBuilder(configurationSectionMock.Object, services);
            var options = optionBuilder.Build();

            // Assert
            Assert.Equal(TimeSpan.FromMinutes(1), options.MessageVisibilityTimeout);
        }

        [Fact]
        public void GetQueueOptionBuilder_ShouldConfigureQueueServiceClientFromServiceKey()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(c => c["ServiceKey"]).Returns("serviceKey");

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            services.AddKeyedSingleton<QueueServiceClient>("serviceKey", new QueueServiceClient(new Uri("https://example.com")));

            // Act
            var optionBuilder = AzureQueueStreamProviderBuilderWrapper.GetQueueOptionBuilder(configurationSectionMock.Object, services);
            var options = optionBuilder.Build();

            // Assert
            Assert.NotNull(options.QueueServiceClient);
        }

        [Fact]
        public void GetQueueOptionBuilder_ShouldConfigureQueueServiceClientFromConnectionString()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(c => c["ConnectionName"]).Returns("connectionName");
            configurationSectionMock.Setup(c => c["ConnectionString"]).Returns("");

            var services = new ServiceCollection();
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetConnectionString("connectionName")).Returns("UseDevelopmentStorage=true");
            services.AddSingleton<IConfiguration>(configurationMock.Object);

            // Act
            var optionBuilder = AzureQueueStreamProviderBuilderWrapper.GetQueueOptionBuilder(configurationSectionMock.Object, services);
            var options = optionBuilder.Build();

            // Assert
            Assert.NotNull(options.QueueServiceClient);
        }
    }
}
