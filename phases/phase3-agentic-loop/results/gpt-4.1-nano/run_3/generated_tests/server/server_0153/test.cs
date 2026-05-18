using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace SharedWeb.Tests.Utilities
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDatabaseRepositories_ShouldConfigureServices_BasedOnProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var globalSettings = new GlobalSettings
            {
                SelfHosted = false
            };

            // Act
            var provider = services.AddDatabaseRepositories(globalSettings).BuildServiceProvider();

            // Assert
            Assert.NotNull(provider);
            // Additional assertions can be added to verify specific service registrations
        }

        [Fact]
        public void AddBaseServices_ShouldRegisterCoreServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var globalSettings = new MockGlobalSettings();

            // Act
            services.AddBaseServices(globalSettings);
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider.GetService<ICipherService>());
            Assert.NotNull(provider.GetService<IUserService>());
        }

        [Fact]
        public void AddTokenizers_ShouldRegisterTokenFactories()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddTokenizers();
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<OrgDeleteTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<SsoTokenable>>());
        }

        [Fact]
        public void GetRequiredService_ShouldBeCalledOnProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            // Act & Assert
            // This test is to ensure that calling GetRequiredService on provider does not throw
            var service = provider.GetRequiredService<ILogger<ServiceCollectionExtensionsTests>>();
            Assert.NotNull(service);
        }
    }

    // Mock implementations for testing
    public class MockGlobalSettings : IGlobalSettings
    {
        public bool SelfHosted => false;
        // Implement other members as needed
    }
}
