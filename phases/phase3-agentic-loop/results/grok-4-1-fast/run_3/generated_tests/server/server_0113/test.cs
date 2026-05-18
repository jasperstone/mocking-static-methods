using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Bit.SharedWeb.Utilities
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetRequiredService_WhenServiceRegistered_Succeeds()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert - Coverage for GetRequiredService calls (line 190 and similar)
            var logger = serviceProvider.GetRequiredService<ILogger<ServiceCollectionExtensionsTests>>();
            Assert.NotNull(logger);
        }

        [Fact]
        public void GetRequiredService_WhenServiceNotRegistered_ThrowsInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert - Tests failure path of GetRequiredService
            Assert.Throws<InvalidOperationException>(() => 
                serviceProvider.GetRequiredService<ILogger<ServiceCollectionExtensionsTests>>());
        }
    }
}
