using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Bit.SharedWeb.Services;
using Bit.Core;
using Bit.Core.Auth;
using Bit.Core.Auth.Identity;
using Bit.Core.Auth.Models.Business;
using Bit.Core.Auth.Repositories;
using Bit.Core.Auth.Services;
using Bit.Core.Auth.Services.Implementations;
using Bit.Core.Entities;
using Bit.Core.Enums;
using Bit.Core.Exceptions;
using Bit.Core.Models.Business;
using Bit.Core.Models.Data;
using Bit.Core.Models.Request;
using Bit.Core.Models.Response;
using Bit.Core.OrganizationFeatures;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Core.Services.Implementations;
using Bit.Core.Settings;
using Bit.Core.Tools;
using Bit.Core.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bit.SharedWeb.Tests
{
    public class TestIntegrationListenerConfiguration : IIntegrationListenerConfiguration
    {
        public string RoutingKey { get; set; }
        public IntegrationType IntegrationType { get; set; }
        public int EventPrefetchCount { get; set; }
        public int EventMaxConcurrentCalls { get; set; }
        public int IntegrationPrefetchCount { get; set; }
        public int IntegrationMaxConcurrentCalls { get; set; }
    }

    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_GetRequiredService_Called()
        {
            // Arrange
            var services = new ServiceCollection();
            services.TryAddSingleton<IEventIntegrationPublisher, Mock<IEventIntegrationPublisher>.Object>();
            services.TryAddSingleton<IIntegrationFilterService, Mock<IIntegrationFilterService>.Object>();
            services.TryAddSingleton<IIntegrationConfigurationDetailsCache, Mock<IIntegrationConfigurationDetailsCache>.Object>();
            services.TryAddSingleton<IUserRepository, Mock<IUserRepository>.Object>();
            services.TryAddSingleton<IOrganizationRepository, Mock<IOrganizationRepository>.Object>();
            services.TryAddSingleton<ILogger<EventIntegrationHandler<TestIntegrationListenerConfiguration>>, Mock<ILogger<EventIntegrationHandler<TestIntegrationListenerConfiguration>>>.Object>();
            var serviceProvider = services.BuildServiceProvider();
            var listenerConfiguration = new TestIntegrationListenerConfiguration();
            var eventIntegrationPublisher = serviceProvider.GetService<IEventIntegrationPublisher>();
            var integrationFilterService = serviceProvider.GetService<IIntegrationFilterService>();
            var configurationCache = serviceProvider.GetService<IIntegrationConfigurationDetailsCache>();
            var userRepository = serviceProvider.GetService<IUserRepository>();
            var organizationRepository = serviceProvider.GetService<IOrganizationRepository>();
            var logger = serviceProvider.GetService<ILogger<EventIntegrationHandler<TestIntegrationListenerConfiguration>>>();

            services.TryAddKeyedSingleton<IEventMessageHandler>(listenerConfiguration.RoutingKey, (provider, _) =>
                new EventIntegrationHandler<TestIntegrationListenerConfiguration>(
                    listenerConfiguration.IntegrationType,
                    provider.GetService<IEventIntegrationPublisher>(),
                    provider.GetService<IIntegrationFilterService>(),
                    provider.GetService<IIntegrationConfigurationDetailsCache>(),
                    provider.GetService<IUserRepository>(),
                    provider.GetService<IOrganizationRepository>(),
                    provider.GetService<ILogger<EventIntegrationHandler<TestIntegrationListenerConfiguration>>>()
                )
            );

            serviceProvider = services.BuildServiceProvider();

            // Act
            var eventMessageHandler = serviceProvider.GetService<IEventMessageHandler>();

            // Assert
            Assert.NotNull(eventMessageHandler);
        }
    }
}
