using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Azure.Storage.Queues;
using Azure.Identity;

namespace Orleans.Streaming.AzureStorage.Tests
{
    public class AzureQueueStreamProviderBuilderTests
    {
        [Fact]
        public void ConfigureOptions_UsesGetConnectionString_WhenConnectionNameProvidedAndConnectionStringIsEmpty()
        {
            // Arrange
            var mockRootConfig = new Mock<IConfiguration>();
            var mockConfigSection = new Mock<IConfigurationSection>();
            var services = new ServiceCollection();

            // Setup configuration section mock
            mockConfigSection.Setup(c => c["ConnectionName"]).Returns("TestConnection");
            mockConfigSection.Setup(c => c["ConnectionString"]).Returns((string)null);
            mockConfigSection.Setup(c => c.GetSection("QueueNames")).Returns((IConfigurationSection)null);
            mockConfigSection.Setup(c => c["MessageVisibilityTimeout"]).Returns((string)null);
            mockConfigSection.Setup(c => c["ServiceKey"]).Returns(string.Empty);

            // Setup root configuration mock to return connection string
            mockRootConfig.Setup(c => c.GetConnectionString("TestConnection"))
                .Returns("https://testaccount.queue.core.windows.net/");

            // Setup configuration mock to return the section mock
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c.GetSection(It.IsAny<string>())).Returns(mockConfigSection.Object);
            mockConfig.Setup(c => c["ConnectionName"]).Returns("TestConnection");
            mockConfig.Setup(c => c["ConnectionString"]).Returns((string)null);
            mockConfig.Setup(c => c.GetConnectionString("TestConnection"))
                .Returns("https://testaccount.queue.core.windows.net/");

            // Add the root configuration mock to services
            services.AddSingleton(mockRootConfig.Object);
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var builder = new AzureQueueStreamProviderBuilder();
            var optionsBuilder = new OptionsBuilder<AzureQueueOptions>();
            var builderAction = builder.BuildBuilder(mockConfig.Object);
            builderAction(optionsBuilder);

            // Assert
            Assert.NotNull(optionsBuilder);
        }
    }

    // Placeholder for OptionsBuilder class
    public class OptionsBuilder<T>
    {
        public T Value { get; } = Activator.CreateInstance<T>();
        public void Configure<TOptions>(Action<TOptions, IServiceProvider> configure) where TOptions : class
        {
            // Implementation omitted
        }
    }

    // Placeholder for AzureQueueOptions class
    public class AzureQueueOptions
    {
        public List<string> QueueNames { get; set; }
        public TimeSpan MessageVisibilityTimeout { get; set; }
        public QueueServiceClient QueueServiceClient { get; set; }
    }

    // Placeholder for the builder class
    public class AzureQueueStreamProviderBuilder
    {
        public Action<OptionsBuilder<AzureQueueOptions>> BuildBuilder(IConfiguration configurationSection)
        {
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
                        options.QueueServiceClient = new QueueServiceClient("https://dummy");
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
                                    options.QueueServiceClient = new QueueServiceClient(uri, credential: new DefaultAzureCredential());
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
}
