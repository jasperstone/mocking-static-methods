using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace SharedWeb.Tests.Utilities
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_ThrowsWhenPublisherMissing()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var listenerConfig = new TestListenerConfiguration { RoutingKey = "route-key" };

            Assert.Throws<InvalidOperationException>(() =>
            {
                var serviceProvider = services.BuildServiceProvider();
                serviceProvider.GetRequiredService<IEventIntegrationPublisher>();
            });
        }

        private sealed class TestListenerConfiguration : IIntegrationListenerConfiguration
        {
            public string IntegrationType => "TestIntegration";
            public string RoutingKey { get; set; } = string.Empty;
            public int EventPrefetchCount => 1;
            public int EventMaxConcurrentCalls => 1;
        }
    }

    public interface IIntegrationListenerConfiguration
    {
        string IntegrationType { get; }
        string RoutingKey { get; }
        int EventPrefetchCount { get; }
        int EventMaxConcurrentCalls { get; }
    }

    public interface IEventIntegrationPublisher { }
}
