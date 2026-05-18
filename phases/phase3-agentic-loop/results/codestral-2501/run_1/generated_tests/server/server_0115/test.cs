using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.DataProtection;
using Microsoft.Extensions.Logging;

namespace SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDefaultServices_ShouldRegisterServicesCorrectly()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockDataProtectionProvider = new Mock<IDataProtectionProvider>();
            var mockLogger = new Mock<ILogger<DataProtectorTokenFactory<DuoUserStateTokenable>>>();

            serviceCollection.AddSingleton(mockDataProtectionProvider.Object);
            serviceCollection.AddSingleton(mockLogger.Object);

            // Act
            serviceCollection.AddDefaultServices();

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetService<IStripeAdapter>());
            Assert.NotNull(serviceProvider.GetService<IOrgUserInviteTokenableFactory>());
        }

        [Fact]
        public void AddDefaultServices_ShouldThrowWhenRequiredServiceIsMissing()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => serviceCollection.AddDefaultServices());
        }
    }
}
