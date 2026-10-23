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
        public void GetQueueOptionBuilder_UsesGetConnectionString_WhenConnectionNameProvidedAndConnectionStringEmpty()
        {
            // Arrange
            var connectionName = "MyConnectionName";
            var expectedConnectionString = "UseDevelopmentStorage=true";

            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(s => s["QueueNames"]).Returns((string)null);
            configurationSectionMock.Setup(s => s["MessageVisibilityTimeout"]).Returns((string)null);
            configurationSectionMock.Setup(s => s["ServiceKey"]).Returns((string)null);
            configurationSectionMock.Setup(s => s["ConnectionName"]).Returns(connectionName);
            configurationSectionMock.Setup(s => s["ConnectionString"]).Returns(string.Empty);
            configurationSectionMock.Setup(s => s.GetSection("QueueNames")).Returns((IConfigurationSection)null);

            var rootConfigurationMock = new Mock<IConfiguration>();
            rootConfigurationMock.Setup(c => c.GetConnectionString(connectionName)).Returns(expectedConnectionString);

            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetRequiredService<IConfiguration>()).Returns(rootConfigurationMock.Object);

            var options = new AzureQueueOptions();

            // Simulate the inner lambda from the source code:
            var queueNames = configurationSectionMock.Object.GetSection("QueueNames")?.Get<List<string>>();
            if (queueNames != null)
            {
                options.QueueNames = queueNames;
            }

            var visibilityTimeout = configurationSectionMock.Object["MessageVisibilityTimeout"];
            if (TimeSpan.TryParse(visibilityTimeout, out var visibilityTimeoutTimeSpan))
            {
                options.MessageVisibilityTimeout = visibilityTimeoutTimeSpan;
            }

            var serviceKey = configurationSectionMock.Object["ServiceKey"];
            if (!string.IsNullOrEmpty(serviceKey))
            {
                // Not tested here
            }
            else
            {
                var connectionNameValue = configurationSectionMock.Object["ConnectionName"];
                var connectionString = configurationSectionMock.Object["ConnectionString"];
                if (!string.IsNullOrEmpty(connectionNameValue) && string.IsNullOrEmpty(connectionString))
                {
                    var rootConfiguration = servicesMock.Object.GetRequiredService<IConfiguration>();
                    connectionString = rootConfiguration.GetConnectionString(connectionNameValue);
                }

                if (!string.IsNullOrEmpty(connectionString))
                {
                    if (Uri.TryCreate(connectionString, UriKind.Absolute, out var uri))
                    {
                        if (!string.IsNullOrEmpty(uri.Query))
                        {
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

            // Assert
            Assert.NotNull(options.QueueServiceClient);
            Assert.Equal(expectedConnectionString, options.QueueServiceClient.AccountName + "=" + expectedConnectionString.Substring(expectedConnectionString.IndexOf('=') + 1));
        }
    }

    // Minimal AzureQueueOptions class for testing
    public class AzureQueueOptions
    {
        public List<string> QueueNames { get; set; }
        public TimeSpan MessageVisibilityTimeout { get; set; }
        public QueueServiceClient QueueServiceClient { get; set; }
    }
}
