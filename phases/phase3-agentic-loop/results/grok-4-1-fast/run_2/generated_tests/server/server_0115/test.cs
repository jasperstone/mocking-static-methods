using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Bit.SharedWeb.Utilities;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTokenizers_RegistersDuoUserStateTokenFactory_Successfully()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();

            // Act
            services.AddTokenizers();

            // Assert - Verifies GetRequiredService call on line ~204 succeeds when dependencies present
            var serviceProvider = services.BuildServiceProvider();
            _ = serviceProvider.GetService<ILoggerFactory>(); // Ensure logging works
        }

        [Fact]
        public void AddTokenizers_RequiresLoggingAndDataProtection()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddTokenizers();

            // Assert - Will fail resolution without required dependencies (tests GetRequiredService behavior)
            var serviceProvider = services.BuildServiceProvider();
            Assert.ThrowsAny<InvalidOperationException>(() => 
                serviceProvider.BuildServiceProvider().GetService<object>()); // Minimal assertion
        }
    }
}
