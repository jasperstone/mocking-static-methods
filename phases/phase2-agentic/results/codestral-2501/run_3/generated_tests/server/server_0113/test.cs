using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using Bit.Core.AdminConsole.Models.Business.Tokenables;
using Bit.Core.Auth.Models.Business.Tokenables;
using Bit.Core.Auth.Identity.TokenProviders;
using Microsoft.AspNetCore.DataProtection;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTokenizers_ShouldRegisterAllTokenFactories()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var dataProtectionProviderMock = new Mock<IDataProtectionProvider>();
            var loggerMock = new Mock<ILogger<DataProtectorTokenFactory<OrgDeleteTokenable>>>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IDataProtectionProvider))).Returns(dataProtectionProviderMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(ILogger<DataProtectorTokenFactory<OrgDeleteTokenable>>))).Returns(loggerMock.Object);

            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Act
            serviceCollection.AddTokenizers();

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetService<IDataProtectorTokenFactory<OrgDeleteTokenable>>());
            Assert.NotNull(serviceProvider.GetService<IDataProtectorTokenFactory<EmergencyAccessInviteTokenable>>());
            Assert.NotNull(serviceProvider.GetService<IDataProtectorTokenFactory<SsoTokenable>>());
            Assert.NotNull(serviceProvider.GetService<IDataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>());
            Assert.NotNull(serviceProvider.GetService<IDataProtectorTokenFactory<WebAuthnLoginAssertionOptionsTokenable>>());
            Assert.NotNull(serviceProvider.GetService<IDataProtectorTokenFactory<SsoEmail2faSessionTokenable>>());
            Assert.NotNull(serviceProvider.GetService<IDataProtectorTokenFactory<OrgUserInviteTokenable>>());
            Assert.NotNull(serviceProvider.GetService<IDataProtectorTokenFactory<DuoUserStateTokenable>>());
            Assert.NotNull(serviceProvider.GetService<IDataProtectorTokenFactory<ProviderDeleteTokenable>>());
            Assert.NotNull(serviceProvider.GetService<IDataProtectorTokenFactory<RegistrationEmailVerificationTokenable>>());
            Assert.NotNull(serviceProvider.GetService<IDataProtectorTokenFactory<TwoFactorAuthenticatorUserVerificationTokenable>>());
        }

        [Fact]
        public void AddTokenizers_ShouldThrowException_WhenLoggerNotRegistered()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var dataProtectionProviderMock = new Mock<IDataProtectionProvider>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IDataProtectionProvider))).Returns(dataProtectionProviderMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService(typeof(ILogger<DataProtectorTokenFactory<OrgDeleteTokenable>>))).Throws(new InvalidOperationException());

            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => serviceCollection.AddTokenizers());
        }
    }
}
