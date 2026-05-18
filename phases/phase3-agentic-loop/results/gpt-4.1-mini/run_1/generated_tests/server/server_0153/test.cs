using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void EventIntegrationHandler_Factory_CallsGetRequiredService()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();

            var mockEventIntegrationPublisher = new Mock<IEventIntegrationPublisher>();
            var mockIntegrationFilterService = new Mock<IIntegrationFilterService>();
            var mockConfigurationCache = new Mock<IIntegrationConfigurationDetailsCache>();
            var mockUserRepository = new Mock<IUserRepository>();
            var mockOrganizationRepository = new Mock<IOrganizationRepository>();
            var mockLogger = new Mock<ILogger<EventIntegrationHandler<TestConfig>>>();

            mockServiceProvider.Setup(sp => sp.GetService(typeof(IEventIntegrationPublisher)))
                .Returns(mockEventIntegrationPublisher.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IIntegrationFilterService)))
                .Returns(mockIntegrationFilterService.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IIntegrationConfigurationDetailsCache)))
                .Returns(mockConfigurationCache.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IUserRepository)))
                .Returns(mockUserRepository.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(IOrganizationRepository)))
                .Returns(mockOrganizationRepository.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ILogger<EventIntegrationHandler<TestConfig>>)))
                .Returns(mockLogger.Object);

            var listenerConfiguration = new TestListenerConfig
            {
                RoutingKey = "routingKey",
                IntegrationType = "integrationType"
            };

            // Act
            var handler = new EventIntegrationHandler<TestConfig>(
                listenerConfiguration.IntegrationType,
                (IEventIntegrationPublisher)mockServiceProvider.Object.GetService(typeof(IEventIntegrationPublisher)),
                (IIntegrationFilterService)mockServiceProvider.Object.GetService(typeof(IIntegrationFilterService)),
                (IIntegrationConfigurationDetailsCache)mockServiceProvider.Object.GetService(typeof(IIntegrationConfigurationDetailsCache)),
                (IUserRepository)mockServiceProvider.Object.GetService(typeof(IUserRepository)),
                (IOrganizationRepository)mockServiceProvider.Object.GetService(typeof(IOrganizationRepository)),
                (ILogger<EventIntegrationHandler<TestConfig>>)mockServiceProvider.Object.GetService(typeof(ILogger<EventIntegrationHandler<TestConfig>>))
            );

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(IEventIntegrationPublisher)), Times.Once);
            mockServiceProvider.Verify(sp => sp.GetService(typeof(IIntegrationFilterService)), Times.Once);
            mockServiceProvider.Verify(sp => sp.GetService(typeof(IIntegrationConfigurationDetailsCache)), Times.Once);
            mockServiceProvider.Verify(sp => sp.GetService(typeof(IUserRepository)), Times.Once);
            mockServiceProvider.Verify(sp => sp.GetService(typeof(IOrganizationRepository)), Times.Once);
            mockServiceProvider.Verify(sp => sp.GetService(typeof(ILogger<EventIntegrationHandler<TestConfig>>)), Times.Once);

            Assert.NotNull(handler);
            Assert.Equal(listenerConfiguration.IntegrationType, handler.IntegrationType);
        }

        // Dummy classes and interfaces to satisfy dependencies
        private class TestConfig : IIntegrationConfiguration { }

        private class TestListenerConfig : IIntegrationListenerConfiguration
        {
            public string RoutingKey { get; set; }
            public string IntegrationType { get; set; }
        }

        public interface IIntegrationListenerConfiguration
        {
            string RoutingKey { get; }
            string IntegrationType { get; }
        }

        public interface IIntegrationConfiguration { }

        public interface IEventIntegrationPublisher { }
        public interface IIntegrationFilterService { }
        public interface IIntegrationConfigurationDetailsCache { }
        public interface IUserRepository { }
        public interface IOrganizationRepository { }

        public class EventIntegrationHandler<TConfig> where TConfig : IIntegrationConfiguration
        {
            public string IntegrationType { get; }
            public IEventIntegrationPublisher EventIntegrationPublisher { get; }
            public IIntegrationFilterService IntegrationFilterService { get; }
            public IIntegrationConfigurationDetailsCache ConfigurationCache { get; }
            public IUserRepository UserRepository { get; }
            public IOrganizationRepository OrganizationRepository { get; }
            public ILogger<EventIntegrationHandler<TConfig>> Logger { get; }

            public EventIntegrationHandler(
                string integrationType,
                IEventIntegrationPublisher eventIntegrationPublisher,
                IIntegrationFilterService integrationFilterService,
                IIntegrationConfigurationDetailsCache configurationCache,
                IUserRepository userRepository,
                IOrganizationRepository organizationRepository,
                ILogger<EventIntegrationHandler<TConfig>> logger)
            {
                IntegrationType = integrationType;
                EventIntegrationPublisher = eventIntegrationPublisher;
                IntegrationFilterService = integrationFilterService;
                ConfigurationCache = configurationCache;
                UserRepository = userRepository;
                OrganizationRepository = organizationRepository;
                Logger = logger;
            }
        }
    }
}
