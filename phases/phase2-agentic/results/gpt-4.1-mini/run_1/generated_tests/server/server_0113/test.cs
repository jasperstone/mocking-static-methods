using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.DataProtection;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;
using Bit.Core.AdminConsole.Models.Business.Tokenables;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTokenizers_RegistersDataProtectorTokenFactories_AndResolvesLogger()
        {
            // Arrange
            var services = new ServiceCollection();

            // Setup a mock IDataProtectionProvider to be returned by GetDataProtectionProvider extension method
            var mockDataProtectionProvider = new Mock<IDataProtectionProvider>();

            // Setup a mock ILogger for the generic type DataProtectorTokenFactory<EmergencyAccessInviteTokenable>
            var mockLogger = new Mock<ILogger<DataProtectorTokenFactory<EmergencyAccessInviteTokenable>>>();

            // Register the mock IDataProtectionProvider and ILogger in the service collection
            services.AddSingleton(mockDataProtectionProvider.Object);
            services.AddSingleton(mockLogger.Object);

            // We need to register the extension method GetDataProtectionProvider for IServiceProvider
            // Since it is an extension method, we simulate it by registering a service that returns the mockDataProtectionProvider
            // The extension method likely calls serviceProvider.GetService<IDataProtectionProvider>()
            // So we register IDataProtectionProvider as singleton (done above)

            // Also register ILogger<DataProtectorTokenFactory<EmergencyAccessInviteTokenable>> as singleton (done above)

            // Act
            services.AddTokenizers();

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            // Resolve the IDataProtectorTokenFactory<EmergencyAccessInviteTokenable> and verify it is not null
            var tokenFactory = serviceProvider.GetService<IDataProtectorTokenFactory<EmergencyAccessInviteTokenable>>();
            Assert.NotNull(tokenFactory);

            // The tokenFactory should be of type DataProtectorTokenFactory<EmergencyAccessInviteTokenable>
            Assert.IsType<DataProtectorTokenFactory<EmergencyAccessInviteTokenable>>(tokenFactory);
        }
    }
}
