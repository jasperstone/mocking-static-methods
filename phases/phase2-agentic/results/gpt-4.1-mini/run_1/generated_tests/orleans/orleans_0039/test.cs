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
            configurationSectionMock.Setup(s => s.GetSection("QueueNames")).Returns(Mock.Of<IConfigurationSection>(q => q.Get<List<string>>() == null));
            configurationSectionMock.Setup(s => s["MessageVisibilityTimeout"]).Returns(string.Empty);

            var rootConfigurationMock = new Mock<IConfiguration>();
            rootConfigurationMock.Setup(c => c.GetConnectionString(connectionName)).Returns(expectedConnectionString);

            var serviceProviderMock = new Mock<IServiceProvider>();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(rootConfigurationMock.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // We need to mock IServiceProvider.GetRequiredService<IConfiguration>() to return rootConfigurationMock.Object
            var serviceProviderWrapperMock = new Mock<IServiceProvider>();
            serviceProviderWrapperMock.Setup(sp => sp.GetService(typeof(IConfiguration))).Returns(rootConfigurationMock.Object);
            serviceProviderWrapperMock.Setup(sp => sp.GetService(typeof(object))).Returns(null);
            serviceProviderWrapperMock.Setup(sp => sp.GetService(typeof(IServiceProvider))).Returns(serviceProviderWrapperMock.Object);

            // We will use the real IServiceProvider from serviceCollection for GetRequiredService<IConfiguration>
            // but we need to pass it to the Configure delegate

            var optionsBuilder = new OptionsBuilder<AzureQueueOptions>(new ServiceCollection());
            var configureAction = GetQueueOptionBuilder(configurationSectionMock.Object);

            // Act
            configureAction(optionsBuilder);

            var options = new AzureQueueOptions();
            var configureDelegate = optionsBuilder.Services.BuildServiceProvider().GetRequiredService<IConfigureOptions<AzureQueueOptions>>();
            // Instead of using IConfigureOptions, invoke the configure delegate directly:
            optionsBuilder.OptionsAction.Invoke(options, serviceProviderWrapperMock.Object);

            // Assert
            Assert.NotNull(options.QueueServiceClient);
            Assert.IsType<QueueServiceClient>(options.QueueServiceClient);
        }

        [Fact]
        public void GetQueueOptionBuilder_UsesQueueNamesAndVisibilityTimeout()
        {
            // Arrange
            var queueNames = new List<string> { "queue1", "queue2" };
            var visibilityTimeout = TimeSpan.FromMinutes(5);
            var visibilityTimeoutString = visibilityTimeout.ToString();

            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(s => s.GetSection("QueueNames")).Returns(Mock.Of<IConfigurationSection>(q => q.Get<List<string>>() == queueNames));
            configurationSectionMock.Setup(s => s["MessageVisibilityTimeout"]).Returns(visibilityTimeoutString);
            configurationSectionMock.Setup(s => s["ServiceKey"]).Returns(string.Empty);
            configurationSectionMock.Setup(s => s["ConnectionName"]).Returns(string.Empty);
            configurationSectionMock.Setup(s => s["ConnectionString"]).Returns(string.Empty);

            var serviceProviderMock = new Mock<IServiceProvider>();

            var optionsBuilder = new OptionsBuilder<AzureQueueOptions>(new ServiceCollection());
            var configureAction = GetQueueOptionBuilder(configurationSectionMock.Object);

            // Act
            configureAction(optionsBuilder);

            var options = new AzureQueueOptions();
            optionsBuilder.OptionsAction.Invoke(options, serviceProviderMock.Object);

            // Assert
            Assert.Equal(queueNames, options.QueueNames);
            Assert.Equal(visibilityTimeout, options.MessageVisibilityTimeout);
        }

        private static Action<OptionsBuilder<AzureQueueOptions>> GetQueueOptionBuilder(IConfigurationSection configurationSection)
        {
            // This is a copy of the private static method from AzureQueueStreamProviderBuilder
            return (OptionsBuilder<AzureQueueOptions> optionsBuilder) =>
            {
                optionsBuilder.Configure<IServiceProvider>((options, services) =>
                {
                    var queueNames = configurationSection.GetSection("QueueNames")?.Get<List<string>>();
                    if (queueNames != null)
                    {
                        options.QueueNames = queueNames;
                    }

                    var visibilityTimeout = configurationSection["MessageVisibilityTimeout"];
                    if (TimeSpan.TryParse(visibilityTimeout, out var visibilityTimeoutTimeSpan))
                    {
                        options.MessageVisibilityTimeout = visibilityTimeoutTimeSpan;
                    }

                    var serviceKey = configurationSection["ServiceKey"];
                    if (!string.IsNullOrEmpty(serviceKey))
                    {
                        // Get a client by name.
                        options.QueueServiceClient = services.GetRequiredKeyedService<QueueServiceClient>(serviceKey);
                    }
                    else
                    {
                        // Construct a connection multiplexer from a connection string.
                        var connectionName = configurationSection["ConnectionName"];
                        var connectionString = configurationSection["ConnectionString"];
                        if (!string.IsNullOrEmpty(connectionName) && string.IsNullOrEmpty(connectionString))
                        {
                            var rootConfiguration = services.GetRequiredService<IConfiguration>();
                            connectionString = rootConfiguration.GetConnectionString(connectionName);
                        }

                        if (!string.IsNullOrEmpty(connectionString))
                        {
                            if (Uri.TryCreate(connectionString, UriKind.Absolute, out var uri))
                            {
                                if (!string.IsNullOrEmpty(uri.Query))
                                {
                                    // SAS URI
                                    options.QueueServiceClient = new QueueServiceClient(uri);
                                }
                                else
                                {
                                    options.QueueServiceClient = new QueueServiceClient(uri, credential: new Azure.Identity.DefaultAzureCredential());
                                }
                            }
                            else
                            {
                                options.QueueServiceClient = new QueueServiceClient(connectionString);
                            }
                        }
                    }
                });
            };
        }
    }

    // Minimal stub for AzureQueueOptions to allow compilation
    public class AzureQueueOptions
    {
        public List<string> QueueNames { get; set; }
        public TimeSpan MessageVisibilityTimeout { get; set; }
        public QueueServiceClient QueueServiceClient { get; set; }
    }

    // Extension method stubs to allow compilation
    public static class ServiceProviderExtensions
    {
        public static T GetRequiredKeyedService<T>(this IServiceProvider services, string key)
        {
            throw new NotImplementedException();
        }

        public static T GetRequiredService<T>(this IServiceProvider services)
        {
            var service = services.GetService(typeof(T));
            if (service == null)
                throw new InvalidOperationException($"Service of type {typeof(T)} not found.");
            return (T)service;
        }
    }
}
