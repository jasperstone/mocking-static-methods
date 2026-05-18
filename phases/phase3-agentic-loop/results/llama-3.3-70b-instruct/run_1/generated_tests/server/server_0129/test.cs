using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Bit.Core.Services;
using Bit.Core.Services.Implementations;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAzureServiceBusIntegration_RegistersEventIntegrationHandler()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>().Object;
            services.AddAzureServiceBusIntegration(listenerConfiguration);

            // Act
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var eventIntegrationHandler = serviceProvider.GetService(typeof(IEventMessageHandler));
            Assert.NotNull(eventIntegrationHandler);
        }

        [Fact]
        public void AddAzureServiceBusIntegration_GetRequiredService_ReturnsCorrectInstance()
        {
            // Arrange
            var services = new ServiceCollection();
            var listenerConfiguration = new Mock<IIntegrationListenerConfiguration>().Object;
            services.AddAzureServiceBusIntegration(listenerConfiguration);

            // Act
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var eventIntegrationHandler = serviceProvider.GetService(typeof(IEventMessageHandler));
            var eventIntegrationPublisher = serviceProvider.GetService(typeof(IEventIntegrationPublisher));
            var integrationFilterService = serviceProvider.GetService(typeof(IIntegrationFilterService));
            var configurationCache = serviceProvider.GetService(typeof(IIntegrationConfigurationDetailsCache));
            var userRepository = serviceProvider.GetService(typeof(IUserRepository));
            var organizationRepository = serviceProvider.GetService(typeof(IOrganizationRepository));
            var logger = serviceProvider.GetService(typeof(ILogger));

            Assert.NotNull(eventIntegrationHandler);
            Assert.NotNull(eventIntegrationPublisher);
            Assert.NotNull(integrationFilterService);
            Assert.NotNull(configurationCache);
            Assert.NotNull(userRepository);
            Assert.NotNull(organizationRepository);
            Assert.NotNull(logger);
        }
    }
}
