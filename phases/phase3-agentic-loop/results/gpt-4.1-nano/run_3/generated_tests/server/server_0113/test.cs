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
            services.AddLogging();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            services.AddTokenizers();
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<OrgDeleteTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<EmergencyAccessInviteTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<SsoTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<WebAuthnLoginAssertionOptionsTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<SsoEmail2faSessionTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<OrgUserInviteTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<DuoUserStateTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<ProviderDeleteTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<RegistrationEmailVerificationTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<TwoFactorAuthenticatorUserVerificationTokenable>>());
        }
    }
}
