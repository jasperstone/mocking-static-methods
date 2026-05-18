using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Bit.SharedWeb.Utilities;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_RegistersServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfigurationMock = new Mock<IIntegrationListenerConfiguration>();
            listenerConfigurationMock.SetupGet(x => x.RoutingKey).Returns("test-routing-key");
            var listenerConfiguration = listenerConfigurationMock.Object;

            services.AddTransient<IEventIntegrationPublisher, MockEventIntegrationPublisher>();
            services.AddTransient<IIntegrationFilterService, MockIntegrationFilterService>();
            services.AddTransient<IIntegrationConfigurationDetailsCache, MockIntegrationConfigurationDetailsCache>();
            services.AddTransient<IUserRepository, MockUserRepository>();
            services.AddTransient<IOrganizationRepository, MockOrganizationRepository>();
            services.AddTransient<ILogger<EventIntegrationHandler<object>>, MockLogger>();

            // Act
            services.AddAzureServiceBusIntegration<object, IIntegrationListenerConfiguration>(listenerConfiguration);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var eventMessageHandler = serviceProvider.GetService<IEventMessageHandler>();
            var eventIntegrationPublisher = serviceProvider.GetService<IEventIntegrationPublisher>();
            var integrationFilterService = serviceProvider.GetService<IIntegrationFilterService>();
            var configurationCache = serviceProvider.GetService<IIntegrationConfigurationDetailsCache>();
            var userRepository = serviceProvider.GetService<IUserRepository>();
            var organizationRepository = serviceProvider.GetService<IOrganizationRepository>();
            var logger = serviceProvider.GetService<ILogger<EventIntegrationHandler<object>>>();

            Assert.NotNull(eventMessageHandler);
            Assert.NotNull(eventIntegrationPublisher);
            Assert.NotNull(integrationFilterService);
            Assert.NotNull(configurationCache);
            Assert.NotNull(userRepository);
            Assert.NotNull(organizationRepository);
            Assert.NotNull(logger);
        }
    }

    public class MockEventIntegrationPublisher : IEventIntegrationPublisher
    {
        public Task PublishEventAsync(EventMessage message)
        {
            return Task.CompletedTask;
        }
    }

    public class MockIntegrationFilterService : IIntegrationFilterService
    {
        public Task<bool> FilterAsync(EventMessage message)
        {
            return Task.FromResult(true);
        }
    }

    public class MockIntegrationConfigurationDetailsCache : IIntegrationConfigurationDetailsCache
    {
        public Task<IntegrationConfigurationDetails> GetAsync(string id)
        {
            return Task.FromResult(new IntegrationConfigurationDetails());
        }
    }

    public class MockUserRepository : IUserRepository
    {
        public Task<User> GetUserAsync(string id)
        {
            return Task.FromResult(new User());
        }
    }

    public class MockOrganizationRepository : IOrganizationRepository
    {
        public Task<Organization> GetOrganizationAsync(string id)
        {
            return Task.FromResult(new Organization());
        }
    }

    public class MockLogger : ILogger<EventIntegrationHandler<object>>
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
        }
    }
}
