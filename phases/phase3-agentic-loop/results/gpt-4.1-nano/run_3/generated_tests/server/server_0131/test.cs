using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Bit.Core;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Core.Auth.Repositories;
using Bit.Core.Auth.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDatabaseRepositories_ShouldConfigureServices_BasedOnProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var globalSettings = new Mock<IGlobalSettings>().Object;

            // Act
            var provider = ServiceCollectionExtensions.GetType()
                .GetMethod("GetDatabaseProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, new object[] { globalSettings });
            var result = ServiceCollectionExtensions.AddDatabaseRepositories(services, globalSettings);

            // Assert
            Assert.Contains(services, s => s.ServiceType == typeof(IEventRepository));
            Assert.Contains(services, s => s.ServiceType == typeof(IInstallationDeviceRepository));
        }

        [Fact]
        public void AddBaseServices_ShouldAddExpectedServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var globalSettings = new Mock<IGlobalSettings>().Object;

            // Act
            ServiceCollectionExtensions.AddBaseServices(services, globalSettings);

            // Assert
            Assert.Contains(services, s => s.ServiceType == typeof(ICipherService));
            Assert.Contains(services, s => s.ServiceType == typeof(IGroupService));
            Assert.Contains(services, s => s.ServiceType == typeof(IEventService));
        }

        [Fact]
        public void AddTokenizers_ShouldRegisterTokenFactories()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            ServiceCollectionExtensions.AddTokenizers(services);
            var provider = services.BuildServiceProvider();

            // Assert
            var factory = provider.GetService<IDataProtectorTokenFactory<OrgDeleteTokenable>>();
            Assert.NotNull(factory);
        }

        [Fact]
        public void GetRequiredService_ShouldCallProviderGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockProvider = new Mock<IServiceProvider>();
            var mockService = new object();
            mockProvider.Setup(p => p.GetRequiredService(typeof(object))).Returns(mockService);
            var sp = mockProvider.Object;

            // Act
            var service = sp.GetRequiredService(typeof(object));

            // Assert
            Assert.Equal(mockService, service);
        }
    }
}
