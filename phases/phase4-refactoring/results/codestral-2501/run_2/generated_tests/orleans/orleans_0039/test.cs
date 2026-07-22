using System;
using System.Collections.Generic;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class AzureQueueStreamProviderBuilderTests
    {
        [Fact]
        public void GetQueueOptionBuilder_ShouldConfigureQueueNames()
        {
            // Arrange
            var configurationSection = new Mock<IConfigurationSection>();
            configurationSection.Setup(c => c.GetSection("QueueNames")).Returns(new Mock<IConfigurationSection>().Object);
            configurationSection.Setup(c => c.GetSection("QueueNames").Get<List<string>>()).Returns(new List<string> { "queue1", "queue2" });

            var optionsBuilder = new Mock<OptionsBuilder<AzureQueueOptions>>();
            var options = new AzureQueueOptions();

            optionsBuilder.Setup(o => o.Configure(It.IsAny<Action<AzureQueueOptions, IServiceProvider>>()))
                .Callback<Action<AzureQueueOptions, IServiceProvider>>((action, sp) => action(options, sp));

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(s => s.GetService(typeof(IConfiguration))).Returns(new Mock<IConfiguration>().Object);

            // Act
            var builder = AzureQueueStreamProviderBuilder.GetQueueOptionBuilder(configurationSection.Object);
            builder(optionsBuilder.Object);

            // Assert
            Assert.Equal(new List<string> { "queue1", "queue2" }, options.QueueNames);
        }

        [Fact]
        public void GetQueueOptionBuilder_ShouldConfigureMessageVisibilityTimeout()
        {
            // Arrange
            var configurationSection = new Mock<IConfigurationSection>();
            configurationSection.Setup(c => c["MessageVisibilityTimeout"]).Returns("00:01:00");

            var optionsBuilder = new Mock<OptionsBuilder<AzureQueueOptions>>();
            var options = new AzureQueueOptions();

            optionsBuilder.Setup(o => o.Configure(It.IsAny<Action<AzureQueueOptions, IServiceProvider>>()))
                .Callback<Action<AzureQueueOptions, IServiceProvider>>((action, sp) => action(options, sp));

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(s => s.GetService(typeof(IConfiguration))).Returns(new Mock<IConfiguration>().Object);

            // Act
            var builder = AzureQueueStreamProviderBuilder.GetQueueOptionBuilder(configurationSection.Object);
            builder(optionsBuilder.Object);

            // Assert
            Assert.Equal(TimeSpan.FromMinutes(1), options.MessageVisibilityTimeout);
        }

        [Fact]
        public void GetQueueOptionBuilder_ShouldConfigureQueueServiceClient_WithServiceKey()
        {
            // Arrange
            var configurationSection = new Mock<IConfigurationSection>();
            configurationSection.Setup(c => c["ServiceKey"]).Returns("serviceKey");

            var optionsBuilder = new Mock<OptionsBuilder<AzureQueueOptions>>();
            var options = new AzureQueueOptions();

            optionsBuilder.Setup(o => o.Configure(It.IsAny<Action<AzureQueueOptions, IServiceProvider>>()))
                .Callback<Action<AzureQueueOptions, IServiceProvider>>((action, sp) => action(options, sp));

            var serviceProvider = new Mock<IServiceProvider>();
            var queueServiceClient = new QueueServiceClient(new Uri("https://example.com"));
            serviceProvider.Setup(s => s.GetRequiredKeyedService<QueueServiceClient>("serviceKey")).Returns(queueServiceClient);

            // Act
            var builder = AzureQueueStreamProviderBuilder.GetQueueOptionBuilder(configurationSection.Object);
            builder(optionsBuilder.Object);

            // Assert
            Assert.Equal(queueServiceClient, options.QueueServiceClient);
        }

        [Fact]
        public void GetQueueOptionBuilder_ShouldConfigureQueueServiceClient_WithConnectionString()
        {
            // Arrange
            var configurationSection = new Mock<IConfigurationSection>();
            configurationSection.Setup(c => c["ConnectionName"]).Returns("connectionName");
            configurationSection.Setup(c => c["ConnectionString"]).Returns("");

            var optionsBuilder = new Mock<OptionsBuilder<AzureQueueOptions>>();
            var options = new AzureQueueOptions();

            optionsBuilder.Setup(o => o.Configure(It.IsAny<Action<AzureQueueOptions, IServiceProvider>>()))
                .Callback<Action<AzureQueueOptions, IServiceProvider>>((action, sp) => action(options, sp));

            var serviceProvider = new Mock<IServiceProvider>();
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(c => c.GetConnectionString("connectionName")).Returns("connectionString");
            serviceProvider.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(configuration.Object);

            // Act
            var builder = AzureQueueStreamProviderBuilder.GetQueueOptionBuilder(configurationSection.Object);
            builder(optionsBuilder.Object);

            // Assert
            Assert.NotNull(options.QueueServiceClient);
        }
    }
}
