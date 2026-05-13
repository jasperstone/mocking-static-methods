using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Bit.SharedWeb.Utilities;

namespace ServiceCollectionExtensionsTests
{
    public class AddTokenizersTests
    {
        [Fact]
        public void AddTokenizers_Should_Register_All_TokenFactories_With_GetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockDataProtectionProvider = new Mock<IDataProtectionProvider>();
            var mockLoggerType = typeof(ILogger<DataProtectorTokenFactory<OrgDeleteTokenable>>);
            var mockLogger = new Mock<ILogger<DataProtectorTokenFactory<OrgDeleteTokenable>>>();
            var mockLogger2 = new Mock<ILogger<DataProtectorTokenFactory<EmergencyAccessInviteTokenable>>>();
            var mockLogger3 = new Mock<ILogger<DataProtectorTokenFactory<SsoTokenable>>>();
            var mockLogger4 = new Mock<ILogger<DataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>>();

            // Setup a service provider to return the mocks
            var serviceProvider = new ServiceCollection()
                .AddSingleton(mockDataProtectionProvider.Object)
                .AddSingleton(mockLogger.Object)
                .AddSingleton(mockLogger2.Object)
                .AddSingleton(mockLogger3.Object)
                .AddSingleton(mockLogger4.Object)
                .BuildServiceProvider();

            // Act
            services.AddTokenizers();

            // Build the service provider to resolve the registered factories
            var provider = services.BuildServiceProvider();

            // Assert
            // Check that the factories are registered
            var factory1 = provider.GetService<IDataProtectorTokenFactory<OrgDeleteTokenable>>();
            var factory2 = provider.GetService<IDataProtectorTokenFactory<EmergencyAccessInviteTokenable>>();
            var factory3 = provider.GetService<IDataProtectorTokenFactory<SsoTokenable>>();
            var factory4 = provider.GetService<IDataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>();

            Assert.NotNull(factory1);
            Assert.NotNull(factory2);
            Assert.NotNull(factory3);
            Assert.NotNull(factory4);

            // Verify that the factories are created with the correct parameters
            Assert.IsType<DataProtectorTokenFactory<OrgDeleteTokenable>>(factory1);
            Assert.IsType<DataProtectorTokenFactory<EmergencyAccessInviteTokenable>>(factory2);
            Assert.IsType<DataProtectorTokenFactory<SsoTokenable>>(factory3);
            Assert.IsType<DataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>(factory4);
        }
    }
}
