using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Bit.Core;
using Bit.Core.Utilities;
using Bit.Core.SecretsManager.Repositories;
using Bit.Core.Repositories;
using Bit.Core;
using Bit.SharedWeb.Utilities;

namespace Bit.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDatabaseRepositories_ShouldReturnSupportedDatabaseProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var globalSettings = new GlobalSettings
            {
                SelfHosted = false,
                // Set other necessary properties
            };

            // Act
            var provider = services.AddDatabaseRepositories(globalSettings);

            // Assert
            Assert.IsType<SupportedDatabaseProviders>(provider);
        }

        [Fact]
        public void AddBaseServices_ShouldRegisterServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGlobalSettings = new Mock<IGlobalSettings>();
            mockGlobalSettings.Setup(g => g).Returns(() => null); // Simplify for test

            // Act
            services.AddBaseServices(mockGlobalSettings.Object);

            // Assert
            Assert.Contains(services, s => s.ServiceType == typeof(ICipherService));
            Assert.Contains(services, s => s.ServiceType == typeof(IGroupService));
        }

        [Fact]
        public void AddTokenizers_ShouldRegisterTokenFactories()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddTokenizers();

            // Assert
            Assert.Contains(services, s => s.ServiceType == typeof(IDataProtectorTokenFactory<OrgDeleteTokenable>));
            Assert.Contains(services, s => s.ServiceType == typeof(IDataProtectorTokenFactory<SsoTokenable>));
        }

        [Fact]
        public void GetRequiredService_ShouldReturnService()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert
            var provider = serviceProvider.GetRequiredService<IServiceProvider>();
            Assert.NotNull(provider);
        }
    }
}
