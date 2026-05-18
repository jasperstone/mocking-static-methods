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
