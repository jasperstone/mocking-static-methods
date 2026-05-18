using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ServiceCollectionExtensionsTests
{
    public class AddTokenizersTests
    {
        [Fact]
        public void AddTokenizers_Should_Register_All_TokenFactories_With_GetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add a dummy DataProtectionProvider
            services.AddDataProtection();

            // Add a dummy logger factory
            services.AddLogging();

            // Build initial provider to resolve dependencies
            var initialProvider = services.BuildServiceProvider();

            // Act
            services.AddTokenizers();

            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<OrgDeleteTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<EmergencyAccessInviteTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<SsoTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<WebAuthnLoginAssertionOptionsTokenable>>());
            // Add more assertions if needed for other tokenables
        }
    }
}
