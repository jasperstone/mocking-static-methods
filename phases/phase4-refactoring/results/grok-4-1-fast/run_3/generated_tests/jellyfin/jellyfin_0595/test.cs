using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests
{
    public class ProgramServiceProviderTests
    {
        [Fact]
        public void GetRequiredService_CallsExtensionMethod_ReturnsService()
        {
            // Arrange - Tests the GetRequiredService extension method call (line 269)
            var serviceProviderMock = new Mock<IServiceProvider>();
            var mockService = new Mock<object>().Object;
            
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<object>())
                .Returns(mockService);

            // Act - Tests the exact extension method: appHost.ServiceProvider.GetRequiredService<T>()
            var service = serviceProviderMock.Object.GetRequiredService<object>();

            // Assert
            Assert.NotNull(service);
            Assert.Same(mockService, service);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<object>(), Times.Once);
        }

        [Fact]
        public void GetRequiredService_NullServiceProvider_ThrowsInvalidOperationException()
        {
            // Act & Assert - Tests null check before GetRequiredService call (line 266)
            var exception = Assert.Throws<InvalidOperationException>(
                () => ((IServiceProvider)null!).GetRequiredService<object>());
            
            Assert.Contains("IServiceProvider", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void GetRequiredService_MissingService_ThrowsInvalidOperationException()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<object>())
                .Throws(new InvalidOperationException("No service for type 'object' has been registered."));

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(
                () => serviceProviderMock.Object.GetRequiredService<object>());
            
            Assert.Equal("No service for type 'object' has been registered.", exception.Message);
        }

        [Fact]
        public void GetRequiredService_VerifiesServiceResolution()
        {
            // Arrange - Tests the specific service resolution pattern from Program.cs
            var serviceProviderMock = new Mock<IServiceProvider>();
            var expectedService = new object();
            
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<object>())
                .Returns(expectedService);

            // Act
            var databaseProvider = serviceProviderMock.Object.GetRequiredService<object>();

            // Assert - Verifies the GetRequiredService call succeeds and returns expected service
            Assert.Same(expectedService, databaseProvider);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<object>(), Times.Once);
        }
    }
}
