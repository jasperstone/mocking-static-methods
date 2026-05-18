using Xunit;
using Microsoft.Extensions.DependencyInjection;
using System;
using Moq;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTokenizers_GetRequiredService_Called()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDataProtection();

            // Act
            services.AddTokenizers();
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var logger = serviceProvider.GetService<ILogger<DataProtectorTokenFactory<SsoTokenable>>>();
            Assert.NotNull(logger);
        }
    }
}
