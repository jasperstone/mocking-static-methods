using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests
{
    public class ServiceProviderServiceExtensionsTests
    {
        [Fact]
        public void GetRequiredService_ThrowsInvalidOperationException_WhenServiceNotRegistered()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(object)))
                .Returns((object?)null);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(
                () => serviceProviderMock.Object.GetRequiredService<object>());

            Assert.Contains("No service for type", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetRequiredService_ReturnsService_WhenServiceAvailable()
        {
            // Arrange
            var expectedService = new Mock<object>().Object;
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(object)))
                .Returns(expectedService);

            // Act
            var result = serviceProviderMock.Object.GetRequiredService<object>();

            // Assert
            Assert.Equal(expectedService, result);
        }

        [Fact]
        public void GetRequiredService_SucceedsWithRealServiceProvider_WhenServiceRegistered()
        {
            // Arrange - simulates the line 269 scenario where service is registered
            var services = new ServiceCollection();
            services.AddSingleton<ILogger>(NullLogger.Instance);
            services.AddSingleton(Mock.Of<object>()); // Mock service simulating IJellyfinDatabaseProvider
            
            var serviceProvider = services.BuildServiceProvider();

            // Act - exact pattern from line 269
            var databaseProvider = serviceProvider.GetRequiredService<object>();

            // Assert
            Assert.NotNull(databaseProvider);
        }

        [Fact]
        public void GetRequiredService_MultipleCalls_ReturnsSameInstance_ForSingleton()
        {
            // Arrange
            var expectedService = new Mock<object>().Object;
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(object)))
                .Returns(expectedService);

            // Act
            var result1 = serviceProviderMock.Object.GetRequiredService<object>();
            var result2 = serviceProviderMock.Object.GetRequiredService<object>();

            // Assert
            Assert.Equal(expectedService, result1);
            Assert.Equal(expectedService, result2);
            Assert.Same(result1, result2);
        }
    }
}
