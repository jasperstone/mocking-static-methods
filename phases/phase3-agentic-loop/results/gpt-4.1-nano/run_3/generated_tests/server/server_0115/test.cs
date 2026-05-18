using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Bit.SharedWeb.Utilities;
using Microsoft.AspNetCore.DataProtection;

namespace Bit.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTokenizers_Should_Register_All_TokenFactories()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDataProtection();

            // Act
            services.AddTokenizers();

            var provider = services.BuildServiceProvider();

            // Assert
            var orgDeleteTokenFactory = provider.GetService<IDataProtectorTokenFactory<OrgDeleteTokenable>>();
            var emergencyInviteTokenFactory = provider.GetService<IDataProtectorTokenFactory<EmergencyAccessInviteTokenable>>();
            var ssoTokenFactory = provider.GetService<IDataProtectorTokenFactory<SsoTokenable>>();
            var webAuthnCreateOptionsTokenFactory = provider.GetService<IDataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>();

            Assert.NotNull(orgDeleteTokenFactory);
            Assert.NotNull(emergencyInviteTokenFactory);
            Assert.NotNull(ssoTokenFactory);
            Assert.NotNull(webAuthnCreateOptionsTokenFactory);
        }
    }
}
