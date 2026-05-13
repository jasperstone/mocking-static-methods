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
        public void GetQueueOptionBuilder_ShouldSetQueueNames()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(c => c.GetSection("QueueNames")).Returns(new Mock<IConfigurationSection>().Object);
            configurationSectionMock.Setup(c => c.GetSection("QueueNames").Get<List<string>>()).Returns(new List<string> { "queue1", "queue2" });

            var optionsBuilderMock = new Mock<OptionsBuilder<AzureQueueOptions>>();
            var optionsMock = new Mock<AzureQueueOptions>();

            optionsBuilderMock.Setup(o => o.Configure(It.IsAny<Action<AzureQueueOptions, IServiceProvider>>()))
                .Callback<Action<AzureQueueOptions, IServiceProvider>>(action => action(optionsMock.Object, new Mock<IServiceProvider>().Object));

            // Act
            var optionBuilder = AzureQueueStreamProviderBuilder.GetQueueOptionBuilder(configurationSectionMock.Object);
            optionBuilder(optionsBuilderMock.Object);

            // Assert
            Assert.Equal(new List<string> { "queue1", "queue2" }, optionsMock.Object.QueueNames);
        }

        [Fact]
        public void GetQueueOptionBuilder_ShouldSetMessageVisibilityTimeout()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(c => c["MessageVisibilityTimeout"]).Returns("00:01:00");

            var optionsBuilderMock = new Mock<OptionsBuilder<AzureQueueOptions>>();
            var optionsMock = new Mock<AzureQueueOptions>();

            optionsBuilderMock.Setup(o => o.Configure(It.IsAny<Action<AzureQueueOptions, IServiceProvider>>()))
                .Callback<Action<AzureQueueOptions, IServiceProvider>>(action => action(optionsMock.Object, new Mock<IServiceProvider>().Object));

            // Act
            var optionBuilder = AzureQueueStreamProviderBuilder.GetQueueOptionBuilder(configurationSectionMock.Object);
            optionBuilder(optionsBuilderMock.Object);

            // Assert
            Assert.Equal(TimeSpan.FromMinutes(1), optionsMock.Object.MessageVisibilityTimeout);
        }

        [Fact]
        public void GetQueueOptionBuilder_ShouldSetQueueServiceClientFromServiceKey()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(c => c["ServiceKey"]).Returns("serviceKey");

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetRequiredKeyedService<QueueServiceClient>("serviceKey")).Returns(new QueueServiceClient(new Uri("https://example.com")));

            var optionsBuilderMock = new Mock<OptionsBuilder<AzureQueueOptions>>();
            var optionsMock = new Mock<AzureQueueOptions>();

            optionsBuilderMock.Setup(o => o.Configure(It.IsAny<Action<AzureQueueOptions, IServiceProvider>>()))
                .Callback<Action<AzureQueueOptions, IServiceProvider>>(action => action(optionsMock.Object, serviceProviderMock.Object));

            // Act
            var optionBuilder = AzureQueueStreamProviderBuilder.GetQueueOptionBuilder(configurationSectionMock.Object);
            optionBuilder(optionsBuilderMock.Object);

            // Assert
            Assert.NotNull(optionsMock.Object.QueueServiceClient);
        }

        [Fact]
        public void GetQueueOptionBuilder_ShouldSetQueueServiceClientFromConnectionString()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(c => c["ConnectionName"]).Returns("connectionName");
            configurationSectionMock.Setup(c => c["ConnectionString"]).Returns("");

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetConnectionString("connectionName")).Returns("UseDevelopmentStorage=true");

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(configurationMock.Object);

            var optionsBuilderMock = new Mock<OptionsBuilder<AzureQueueOptions>>();
            var optionsMock = new Mock<AzureQueueOptions>();

            optionsBuilderMock.Setup(o => o.Configure(It.IsAny<Action<AzureQueueOptions, IServiceProvider>>()))
                .Callback<Action<AzureQueueOptions, IServiceProvider>>(action => action(optionsMock.Object, serviceProviderMock.Object));

            // Act
            var optionBuilder = AzureQueueStreamProviderBuilder.GetQueueOptionBuilder(configurationSectionMock.Object);
            optionBuilder(optionsBuilderMock.Object);

            // Assert
            Assert.NotNull(optionsMock.Object.QueueServiceClient);
        }
    }
}
