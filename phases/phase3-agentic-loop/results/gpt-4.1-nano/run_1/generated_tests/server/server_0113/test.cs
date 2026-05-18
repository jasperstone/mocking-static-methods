using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Bit.Core.AdminConsole.Models.Business.Tokenables;
using Bit.Core.Utilities;

namespace Server.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTokenizers_ShouldRegisterAllTokenFactories()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add required dependencies
            services.AddLogging();

            // Act
            ServiceCollectionExtensions.AddTokenizers(services);
            var provider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<OrgDeleteTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<EmergencyAccessInviteTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<SsoTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<WebAuthnLoginAssertionOptionsTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<SsoEmail2faSessionTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<DuoUserStateTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<ProviderDeleteTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<RegistrationEmailVerificationTokenable>>());
            Assert.NotNull(provider.GetService<IDataProtectorTokenFactory<TwoFactorAuthenticatorUserVerificationTokenable>>());
        }
    }
}
