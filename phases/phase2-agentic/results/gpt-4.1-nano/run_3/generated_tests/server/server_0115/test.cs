using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Bit.Core.Utilities;
using Bit.Core.Tokens;
using Bit.SharedWeb.Utilities;

namespace Bit.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTokenizers_Should_Register_All_TokenFactories_With_GetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockDataProtectionProvider = new Mock<IDataProtectionProvider>();
            var mockLogger = new Mock<ILogger<DataProtectorTokenFactory<object>>>();

            // Register the DataProtectionProvider and Logger
            services.AddSingleton(mockDataProtectionProvider.Object);
            services.AddSingleton(mockLogger.Object);

            // Build a service provider to resolve dependencies
            var serviceProvider = services.BuildServiceProvider();

            // Act
            services.AddTokenizers();

            // Build the final provider to resolve the registered factories
            var provider = services.BuildServiceProvider();

            // Assert
            // Check that each factory can be resolved and is of correct type
            var orgDeleteFactory = provider.GetService<IDataProtectorTokenFactory<OrgDeleteTokenable>>();
            Assert.NotNull(orgDeleteFactory);
            Assert.IsType<DataProtectorTokenFactory<OrgDeleteTokenable>>(orgDeleteFactory);

            var emergencyInviteFactory = provider.GetService<IDataProtectorTokenFactory<EmergencyAccessInviteTokenable>>();
            Assert.NotNull(emergencyInviteFactory);
            Assert.IsType<DataProtectorTokenFactory<EmergencyAccessInviteTokenable>>(emergencyInviteFactory);

            var ssoFactory = provider.GetService<IDataProtectorTokenFactory<SsoTokenable>>();
            Assert.NotNull(ssoFactory);
            Assert.IsType<DataProtectorTokenFactory<SsoTokenable>>(ssoFactory);

            var webAuthnFactory = provider.GetService<IDataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>();
            Assert.NotNull(webAuthnFactory);
            Assert.IsType<DataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>(webAuthnFactory);
        }
    }
}
