using System;
using System.Collections.Generic;
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
            configurationSectionMock.Setup(s => s.GetSection("QueueNames")).Returns(Mock.Of<IConfigurationSection>(q => q.Get<List<string>>() == null));
            configurationSectionMock.Setup(s => s["MessageVisibilityTimeout"]).Returns(string.Empty);

            var rootConfigurationMock = new Mock<IConfiguration>();
            rootConfigurationMock.Setup(c => c.GetConnectionString(connectionName)).Returns(expectedConnectionString);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IConfiguration>()).Returns(rootConfigurationMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredKeyedService<QueueServiceClient>(It.IsAny<string>()))
                .Throws(new InvalidOperationException("Should not be called"));

            var options = new AzureQueueOptions();

            var optionsBuilder = new OptionsBuilder<AzureQueueOptions>(new ServiceCollection());
            var configureAction = GetConfigureAction(configurationSectionMock.Object);

            // Act
            configureAction(options, serviceProviderMock.Object);

            // Assert
            Assert.NotNull(options.QueueServiceClient);
            // The QueueServiceClient should be constructed with the connection string returned by GetConnectionString
            Assert.Equal(expectedConnectionString, GetConnectionStringFromQueueServiceClient(options.QueueServiceClient));
        }

        private static Action<AzureQueueOptions, IServiceProvider> GetConfigureAction(IConfigurationSection configurationSection)
        {
            // Access the private static method GetQueueOptionBuilder via reflection
            var method = typeof(AzureQueueStreamProviderBuilder).GetMethod("GetQueueOptionBuilder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var func = (Action<OptionsBuilder<AzureQueueOptions>>)method.Invoke(null, new object[] { configurationSection });

            var optionsBuilder = new OptionsBuilder<AzureQueueOptions>(new ServiceCollection());
            func(optionsBuilder);

            // The Configure method adds a configure delegate to the optionsBuilder
            // We simulate the configure delegate by creating a new options instance and calling Configure<IServiceProvider> delegate
            // Unfortunately, OptionsBuilder does not expose the delegate, so we use reflection to get the private field _configureActions
            var configureActionsField = typeof(OptionsBuilder<AzureQueueOptions>).GetField("_configureActions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var configureActions = (List<Action<AzureQueueOptions, IServiceProvider>>)configureActionsField.GetValue(optionsBuilder);

            // Return a delegate that calls all configure actions with the given options and service provider
            return (options, serviceProvider) =>
            {
                foreach (var action in configureActions)
                {
                    action(options, serviceProvider);
                }
            };
        }

        private static string GetConnectionStringFromQueueServiceClient(QueueServiceClient client)
        {
            // QueueServiceClient does not expose connection string directly.
            // We use reflection to get the private field _pipeline, then get the Uri from the client.
            var uri = client.Uri.ToString();

            // If the Uri is a well-known Azure Storage URI, we return it as connection string for test purposes.
            // Otherwise, we return the Uri string.
            return uri;
        }
    }
}
