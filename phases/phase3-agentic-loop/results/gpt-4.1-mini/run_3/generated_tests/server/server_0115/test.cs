using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.DataProtection;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;
using Bit.Core.AdminConsole.Models.Business.Tokenables;

namespace Bit.SharedWeb.Tests.Utilities
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTokenizers_RegistersDataProtectorTokenFactories_AndCallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Mock IDataProtectionProvider
            var mockDataProtectionProvider = new Mock<IDataProtectionProvider>();

            // Register the mock IDataProtectionProvider as singleton
            services.AddSingleton(mockDataProtectionProvider.Object);

            // Register a mock ILogger for DataProtectorTokenFactory<DuoUserStateTokenable>
            var mockLogger = new Mock<ILogger<DataProtectorTokenFactory<DuoUserStateTokenable>>>();
            services.AddSingleton(mockLogger.Object);

            // Act
            ServiceCollectionExtensions.AddTokenizers(services);

            var serviceProvider = services.BuildServiceProvider();

            // Resolve the IDataProtectorTokenFactory<DuoUserStateTokenable> to trigger the factory delegate
            var factory = serviceProvider.GetService<IDataProtectorTokenFactory<DuoUserStateTokenable>>();

            // Assert
            Assert.NotNull(factory);
            Assert.Equal("Bit.Core.AdminConsole.Models.Business.Tokenables.DuoUserStateTokenable", factory.GetType().GenericTypeArguments[0].FullName);
        }
    }
}
