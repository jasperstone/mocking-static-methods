using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.DataProtection;
using Xunit;
using Moq;
using Bit.SharedWeb.Utilities;
using Bit.Core.AdminConsole.Models.Business.Tokenables;
using Bit.Core.Tokens;

namespace Bit.SharedWeb.Tests.Utilities
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTokenizers_RegistersDataProtectorTokenFactories_AndCallsGetRequiredServiceLogger()
        {
            // Arrange
            var services = new ServiceCollection();

            // Mock IDataProtectionProvider to be returned by serviceProvider.GetDataProtectionProvider()
            var mockDataProtectionProvider = new Mock<IDataProtectionProvider>();
            services.AddSingleton(mockDataProtectionProvider.Object);

            // Register a generic ILogger<T> mock for all T
            services.AddSingleton(typeof(ILogger<>), typeof(MockLogger<>));

            // Act
            services.AddTokenizers();

            // Build service provider
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            // Check that each IDataProtectorTokenFactory<T> is registered and can be resolved
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

        // A simple mock logger that can be used for any T
        private class MockLogger<T> : ILogger<T>
        {
            public IDisposable BeginScope<TState>(TState state) => null!;
            public bool IsEnabled(LogLevel logLevel) => false;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
        }
    }
}
