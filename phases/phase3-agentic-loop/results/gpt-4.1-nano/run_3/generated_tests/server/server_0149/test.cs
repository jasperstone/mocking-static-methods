using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Bit.Core;
using Bit.Core.Services;
using Bit.Core.Auth.Services;
using Bit.Core.Vault.Services;
using Bit.Core.Tokens;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.DataProtection;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTokenizers_ShouldRegisterAllTokenFactories()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockDataProtectionProvider = new Mock<IDataProtectionProvider>();
            var mockLogger1 = new Mock<ILogger<DataProtectorTokenFactory<OrgDeleteTokenable>>>();
            var mockLogger2 = new Mock<ILogger<DataProtectorTokenFactory<EmergencyAccessInviteTokenable>>>();
            var mockLogger3 = new Mock<ILogger<DataProtectorTokenFactory<SsoTokenable>>>();
            var mockLogger4 = new Mock<ILogger<DataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>>();

            services.AddSingleton<IDataProtectionProvider>(mockDataProtectionProvider.Object);
            services.AddLogging();

            // Act
            services.AddTokenizers();

            // Assert
            var provider = services.BuildServiceProvider();

            var orgTokenFactory = provider.GetService<IDataProtectorTokenFactory<OrgDeleteTokenable>>();
            var inviteTokenFactory = provider.GetService<IDataProtectorTokenFactory<EmergencyAccessInviteTokenable>>();
            var ssoTokenFactory = provider.GetService<IDataProtectorTokenFactory<SsoTokenable>>();
            var webAuthnTokenFactory = provider.GetService<IDataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>();

            Assert.NotNull(orgTokenFactory);
            Assert.NotNull(inviteTokenFactory);
            Assert.NotNull(ssoTokenFactory);
            Assert.NotNull(webAuthnTokenFactory);
        }
    }
}
