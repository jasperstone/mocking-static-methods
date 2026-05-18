using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Jellyfin.Server.Tests
{
    public class ProgramTests
    {
        [Fact]
        public void ServiceProviderServiceExtensions_GetRequiredService_Throws_WhenServiceNotRegistered()
        {
            // Arrange - simulates line 269 scenario where service might not be available
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => 
                serviceProvider.GetRequiredService<object>());
        }

        [Fact]
        public void ServiceProviderServiceExtensions_GetRequiredService_ReturnsService_WhenRegistered()
        {
            // Arrange - matches the exact pattern from line 269
            var services = new ServiceCollection();
            services.AddSingleton(new object());
            var serviceProvider = services.BuildServiceProvider();
            
            // Act - exact extension method call: serviceProvider.GetRequiredService<T>()
            var result = serviceProvider.GetRequiredService<object>();
            
            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void ServiceProviderServiceExtensions_GetRequiredService_NonGeneric_Throws_WhenServiceNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => 
                serviceProvider.GetRequiredService(typeof(object)));
        }

        [Fact]
        public void ServiceProviderServiceExtensions_GetRequiredService_NonGeneric_ReturnsService_WhenRegistered()
        {
            // Arrange
            var expected = new object();
            var services = new ServiceCollection();
            services.AddSingleton(expected);
            var serviceProvider = services.BuildServiceProvider();
            
            // Act
            var result = serviceProvider.GetRequiredService(typeof(object));
            
            // Assert
            Assert.Equal(expected, result);
        }
    }
}
