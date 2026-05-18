using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Bit.SharedWeb.Utilities;
using Bit.Core;
using Bit.Core.Entities;
using Bit.Core.Repositories;
using Bit.Core.Auth.Repositories;
using Bit.Core.Auth.Services;
using Bit.Core.Vault.Services;
using Bit.Core.HostedServices;
using Bit.Core.OrganizationFeatures;
using Bit.Core.Platform;
using Bit.Core.Services;
using Bit.Core.Settings;

namespace Bit.SharedWeb.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDatabaseRepositories_ShouldConfigureCorrectProviderAndRepositories()
        {
            // Arrange
            var services = new ServiceCollection();
            var globalSettings = new GlobalSettings
            {
                SelfHosted = false,
                DatabaseProvider = "sqlserver",
                SqlServer = new SqlServerSettings { ConnectionString = "connStr" }
            };

            // Act
            var provider = services.AddDatabaseRepositories(globalSettings);

            // Assert
            Assert.Contains(services, s => s.ServiceType == typeof(IEventRepository));
            Assert.Contains(services, s => s.ServiceType == typeof(IInstallationDeviceRepository));
            Assert.Contains(services, s => s.ServiceType == typeof(IGrantRepository));
            Assert.NotNull(provider);
        }

        [Fact]
        public void AddBaseServices_ShouldAddCoreServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var globalSettings = new Mock<IGlobalSettings>();
            globalSettings.Setup(g => g).Returns(new GlobalSettings());

            // Act
            services.AddBaseServices(new GlobalSettings());

            // Assert
            Assert.Contains(services, s => s.ServiceType == typeof(ICipherService));
            Assert.Contains(services, s => s.ServiceType == typeof(IGroupService));
            Assert.Contains(services, s => s.ServiceType == typeof(IEventService));
            Assert.Contains(services, s => s.ServiceType == typeof(IDeviceService));
        }

        [Fact]
        public void AddTokenizers_ShouldRegisterTokenFactories()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDataProtection();

            // Act
            services.AddTokenizers();

            // Assert
            Assert.Contains(services, s => s.ServiceType == typeof(IDataProtectorTokenFactory<OrgDeleteTokenable>));
            Assert.Contains(services, s => s.ServiceType == typeof(IDataProtectorTokenFactory<EmergencyAccessInviteTokenable>));
            Assert.Contains(services, s => s.ServiceType == typeof(IDataProtectorTokenFactory<SsoTokenable>));
        }

        [Fact]
        public void AddAzureServiceBusIntegration_ShouldCallGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockProvider = new Mock<IServiceProvider>();
            var mockPublisher = new Mock<IEventIntegrationPublisher>();
            var mockFilterService = new Mock<IIntegrationFilterService>();
            var mockCache = new Mock<IIntegrationConfigurationDetailsCache>();
            var mockUserRepo = new Mock<IUserRepository>();
            var mockLogger = new Mock<ILogger<EventIntegrationHandler<object>>>();
            var mockHandler = new Mock<IIntegrationHandler<object>>();

            mockProvider.Setup(p => p.GetRequiredService<IEventIntegrationPublisher>()).Returns(mockPublisher.Object);
            mockProvider.Setup(p => p.GetRequiredService<IIntegrationFilterService>()).Returns(mockFilterService.Object);
            mockProvider.Setup(p => p.GetRequiredService<IIntegrationConfigurationDetailsCache>()).Returns(mockCache.Object);
            mockProvider.Setup(p => p.GetRequiredService<IUserRepository>()).Returns(mockUserRepo.Object);
            mockProvider.Setup(p => p.GetRequiredService<ILogger<EventIntegrationHandler<object>>>()).Returns(mockLogger.Object);
            mockProvider.Setup(p => p.GetRequiredService<IServiceProvider>()).Returns(mockProvider.Object);
            mockProvider.Setup(p => p.GetRequiredService<IEventMessageHandler>()).Returns(mockHandler.Object);

            // Act
            services.AddAzureServiceBusIntegration<object, object>(mockProvider.Object);

            // Assert
            mockProvider.Verify(p => p.GetRequiredService<IEventIntegrationPublisher>(), Times.Once);
            mockProvider.Verify(p => p.GetRequiredService<IIntegrationFilterService>(), Times.Once);
            mockProvider.Verify(p => p.GetRequiredService<IUserRepository>(), Times.Once);
        }
    }
}
