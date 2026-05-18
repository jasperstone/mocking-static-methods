using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Moq;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddRabbitMqIntegration_GetRequiredService_ReturnsInstance()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTransient<IEventMessageHandler, EventIntegrationHandler<SlackIntegrationConfigurationDetails>>();
            services.AddTransient<IIntegrationFilterService, IntegrationFilterService>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IOrganizationRepository, OrganizationRepository>();
            services.AddTransient<ILogger<EventIntegrationHandler<SlackIntegrationConfigurationDetails>>, Logger<EventIntegrationHandler<SlackIntegrationConfigurationDetails>>>();
            services.AddTransient<IEventIntegrationPublisher, EventIntegrationPublisher>();
            services.AddTransient<IIntegrationConfigurationDetailsCache, IntegrationConfigurationDetailsCache>();

            var listenerConfiguration = new SlackListenerConfiguration();
            services.AddRabbitMqIntegration<SlackIntegrationConfigurationDetails, SlackListenerConfiguration>(listenerConfiguration);

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var eventMessageHandler = serviceProvider.GetRequiredService<IEventMessageHandler>();

            // Assert
            Assert.NotNull(eventMessageHandler);
        }
    }

    public class EventIntegrationHandler<T> : IEventMessageHandler
    {
        public EventIntegrationHandler(
            string integrationType,
            IEventIntegrationPublisher eventIntegrationPublisher,
            IIntegrationFilterService integrationFilterService,
            IIntegrationConfigurationDetailsCache configurationCache,
            IUserRepository userRepository,
            IOrganizationRepository organizationRepository,
            ILogger<EventIntegrationHandler<T>> logger)
        {
        }
    }

    public class IntegrationFilterService : IIntegrationFilterService
    {
    }

    public class UserRepository : IUserRepository
    {
    }

    public class OrganizationRepository : IOrganizationRepository
    {
    }

    public class Logger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            throw new NotImplementedException();
        }
    }

    public class EventIntegrationPublisher : IEventIntegrationPublisher
    {
    }

    public class IntegrationConfigurationDetailsCache : IIntegrationConfigurationDetailsCache
    {
    }
}
