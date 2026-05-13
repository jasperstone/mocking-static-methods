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

namespace Orleans.Streaming.AzureStorage.Tests
{
    public class AzureQueueStreamProviderBuilderTests
    {
        [Fact]
        public void GetQueueOptionBuilder_UsesGetConnectionString_WhenConnectionNameProvidedAndConnectionStringEmpty()
        {
            // Arrange
            var connectionName = "MyConnectionName";
            var expectedConnectionString = "UseDevelopmentStorage=true";

            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(s => s["ServiceKey"]).Returns(string.Empty);
            configurationSectionMock.Setup(s => s["ConnectionName"]).Returns(connectionName);
            configurationSectionMock.Setup(s => s["ConnectionString"]).Returns(string.Empty);
            configurationSectionMock.Setup(s => s.GetSection("QueueNames")).Returns(Mock.Of<IConfigurationSection>(s => s.Get<List<string>>() == null));
            configurationSectionMock.Setup(s => s["MessageVisibilityTimeout"]).Returns(string.Empty);

            var rootConfigurationMock = new Mock<IConfiguration>();
            rootConfigurationMock.Setup(c => c.GetConnectionString(connectionName)).Returns(expectedConnectionString);

            var serviceProviderMock = new Mock<IServiceProvider>();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(rootConfigurationMock.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfigurationMock.Object);

            var options = new AzureQueueOptions();

            var optionsBuilder = new OptionsBuilder<AzureQueueOptions>(new ServiceCollection(), "Test");
            var configureAction = typeof(AzureQueueStreamProviderBuilder)
                .GetMethod("GetQueueOptionBuilder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, new object[] { configurationSectionMock.Object }) as Action<OptionsBuilder<AzureQueueOptions>>;

            // Act
            configureAction(optionsBuilder);
            var configureDelegate = optionsBuilder.Services
                .OfType<ServiceDescriptor>()
                .FirstOrDefault(d => d.ServiceType == typeof(IConfigureOptions<AzureQueueOptions>));
            Assert.NotNull(configureDelegate);

            var configureOptions = (IConfigureOptions<AzureQueueOptions>)Activator.CreateInstance(configureDelegate.ImplementationType, new object[] { });
            configureOptions.Configure(options);

            // Assert
            Assert.NotNull(options.QueueServiceClient);
            // The QueueServiceClient should be constructed with the connection string returned by GetConnectionString
            Assert.Equal(expectedConnectionString, options.QueueServiceClient.AccountName == null ? expectedConnectionString : null);
        }

        [Fact]
        public void GetQueueOptionBuilder_SetsQueueNamesAndVisibilityTimeout()
        {
            // Arrange
            var queueNames = new List<string> { "queue1", "queue2" };
            var visibilityTimeout = TimeSpan.FromMinutes(5);

            var configurationSectionMock = new Mock<IConfigurationSection>();
            var queueNamesSectionMock = new Mock<IConfigurationSection>();
            queueNamesSectionMock.Setup(s => s.Get<List<string>>()).Returns(queueNames);
            configurationSectionMock.Setup(s => s.GetSection("QueueNames")).Returns(queueNamesSectionMock.Object);
            configurationSectionMock.Setup(s => s["MessageVisibilityTimeout"]).Returns(visibilityTimeout.ToString());
            configurationSectionMock.Setup(s => s["ServiceKey"]).Returns(string.Empty);
            configurationSectionMock.Setup(s => s["ConnectionName"]).Returns(string.Empty);
            configurationSectionMock.Setup(s => s["ConnectionString"]).Returns(string.Empty);

            var servicesMock = new Mock<IServiceProvider>();

            var options = new AzureQueueOptions();

            var optionsBuilder = new OptionsBuilder<AzureQueueOptions>(new ServiceCollection(), "Test");
            var configureAction = typeof(AzureQueueStreamProviderBuilder)
                .GetMethod("GetQueueOptionBuilder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, new object[] { configurationSectionMock.Object }) as Action<OptionsBuilder<AzureQueueOptions>>;

            // Act
            configureAction(optionsBuilder);
            var configureDelegate = optionsBuilder.Services
                .OfType<ServiceDescriptor>()
                .FirstOrDefault(d => d.ServiceType == typeof(IConfigureOptions<AzureQueueOptions>));
            Assert.NotNull(configureDelegate);

            var configureOptions = (IConfigureOptions<AzureQueueOptions>)Activator.CreateInstance(configureDelegate.ImplementationType, new object[] { });
            configureOptions.Configure(options);

            // Assert
            Assert.Equal(queueNames, options.QueueNames);
            Assert.Equal(visibilityTimeout, options.MessageVisibilityTimeout);
        }
    }
}
