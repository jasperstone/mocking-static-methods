using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.DataProtection;
using Bit.SharedWeb.Utilities;
using Moq;
using Xunit;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTokenizers_RegistersAllTokenFactories_Successfully()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDataProtection();

            // Mock all required loggers for the token factories
            var orgDeleteLogger = new Mock<ILogger<DataProtectorTokenFactory<object>>>().Object;
            var emergencyAccessLogger = new Mock<ILogger<DataProtectorTokenFactory<object>>>().Object;
            var ssoLogger = new Mock<ILogger<DataProtectorTokenFactory<object>>>().Object;
            services.AddSingleton(orgDeleteLogger);
            services.AddSingleton(emergencyAccessLogger);
            services.AddSingleton(ssoLogger);

            // Act
            services.AddTokenizers();

            // Assert - successfully build provider and resolve one service
            using var serviceProvider = services.BuildServiceProvider();
            var tokenFactory = serviceProvider.GetService<IDataProtectorTokenFactory<object>>();
            Assert.NotNull(tokenFactory);
        }

        [Fact]
        public void AddTokenizers_MissingLoggerDependency_ThrowsInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddDataProtection();

            // Act & Assert
            services.AddTokenizers();
            using var serviceProvider = services.BuildServiceProvider();
            Assert.Throws<InvalidOperationException>(
                () => serviceProvider.GetRequiredService<IDataProtectorTokenFactory<object>>());
        }
    }
}
