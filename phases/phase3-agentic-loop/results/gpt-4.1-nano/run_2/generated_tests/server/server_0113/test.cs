using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Server.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTokenizers_ShouldRegisterAllTokenFactories()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging(); // Add logging to support GetRequiredService<ILogger<>>
            var serviceProvider = services.BuildServiceProvider();

            // Act
            services.AddTokenizers();
            var provider = services.BuildServiceProvider();

            // Assert
            var factorySso = provider.GetService<IDataProtectorTokenFactory<SsoTokenable>>();
            var factoryEmergency = provider.GetService<IDataProtectorTokenFactory<EmergencyAccessInviteTokenable>>();
            var factoryWebAuthn = provider.GetService<IDataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>();

            Assert.NotNull(factorySso);
            Assert.NotNull(factoryEmergency);
            Assert.NotNull(factoryWebAuthn);
        }
    }
}
