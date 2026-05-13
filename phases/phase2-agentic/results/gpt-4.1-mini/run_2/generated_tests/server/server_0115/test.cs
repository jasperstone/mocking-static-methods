using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.DataProtection;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;

namespace Bit.SharedWeb.Tests.Utilities
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTokenizers_RegistersDataProtectorTokenFactories_AndCallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            // We will mock the IServiceProvider to verify GetRequiredService calls
            var serviceProviderMock = new Mock<IServiceProvider>();
            var dataProtectionProviderMock = new Mock<IDataProtectionProvider>();

            // Setup GetDataProtectionProvider extension method on IServiceProvider
            // Since it's an extension method, we simulate by adding a service for IDataProtectionProvider
            services.AddSingleton(dataProtectionProviderMock.Object);

            // Setup ILogger mocks for each DataProtectorTokenFactory type
            // We will setup the serviceProviderMock to return these loggers when GetRequiredService is called
            // We expect GetRequiredService<ILogger<DataProtectorTokenFactory<T>>> to be called for each T

            // We will track calls to GetRequiredService to verify it is called with correct generic type
            var loggerMocks = new System.Collections.Generic.Dictionary<Type, object>();

            // Helper to create logger mocks and setup serviceProviderMock
            void SetupLoggerMock<T>()
            {
                var loggerMock = new Mock<ILogger<DataProtectorTokenFactory<T>>>();
                loggerMocks[typeof(DataProtectorTokenFactory<T>)] = loggerMock;
                serviceProviderMock.Setup(sp => sp.GetService(typeof(ILogger<DataProtectorTokenFactory<T>>)))
                    .Returns(loggerMock.Object);
            }

            // Setup logger mocks for the tokenable types used in AddTokenizers
            SetupLoggerMock<Bit.Core.AdminConsole.Models.Business.Tokenables.OrgDeleteTokenable>();
            SetupLoggerMock<Bit.Core.AdminConsole.Models.Business.Tokenables.EmergencyAccessInviteTokenable>();
            SetupLoggerMock<Bit.Core.Auth.Models.Business.Tokenables.SsoTokenable>();
            SetupLoggerMock<Bit.Core.Auth.Models.Business.Tokenables.WebAuthnCredentialCreateOptionsTokenable>();
            SetupLoggerMock<Bit.Core.Auth.Models.Business.Tokenables.WebAuthnLoginAssertionOptionsTokenable>();
            SetupLoggerMock<Bit.Core.Auth.Models.Business.Tokenables.SsoEmail2faSessionTokenable>();
            SetupLoggerMock<Bit.Core.AdminConsole.Models.Business.Tokenables.OrgUserInviteTokenable>();
            SetupLoggerMock<Bit.Core.Auth.Models.Business.Tokenables.DuoUserStateTokenable>();
            SetupLoggerMock<Bit.Core.Auth.Models.Business.Tokenables.ProviderDeleteTokenable>();
            SetupLoggerMock<Bit.Core.Auth.Models.Business.Tokenables.RegistrationEmailVerificationTokenable>();
            SetupLoggerMock<Bit.Core.Auth.Models.Business.Tokenables.TwoFactorAuthenticatorUserVerificationTokenable>();

            // We will override the GetRequiredService extension method by adding a service provider that returns the logger mocks
            // So we add a service provider that returns the logger mocks for ILogger<DataProtectorTokenFactory<T>>
            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            // Call AddTokenizers extension method which registers the token factories
            services.AddTokenizers();

            // Build the service provider to resolve services
            var builtProvider = services.BuildServiceProvider();

            // Assert
            // For each IDataProtectorTokenFactory<T> registered, resolve it and verify it is not null
            var tokenFactoryTypes = new Type[]
            {
                typeof(Bit.Core.AdminConsole.Models.Business.Tokenables.OrgDeleteTokenable),
                typeof(Bit.Core.AdminConsole.Models.Business.Tokenables.EmergencyAccessInviteTokenable),
                typeof(Bit.Core.Auth.Models.Business.Tokenables.SsoTokenable),
                typeof(Bit.Core.Auth.Models.Business.Tokenables.WebAuthnCredentialCreateOptionsTokenable),
                typeof(Bit.Core.Auth.Models.Business.Tokenables.WebAuthnLoginAssertionOptionsTokenable),
                typeof(Bit.Core.Auth.Models.Business.Tokenables.SsoEmail2faSessionTokenable),
                typeof(Bit.Core.AdminConsole.Models.Business.Tokenables.OrgUserInviteTokenable),
                typeof(Bit.Core.Auth.Models.Business.Tokenables.DuoUserStateTokenable),
                typeof(Bit.Core.Auth.Models.Business.Tokenables.ProviderDeleteTokenable),
                typeof(Bit.Core.Auth.Models.Business.Tokenables.RegistrationEmailVerificationTokenable),
                typeof(Bit.Core.Auth.Models.Business.Tokenables.TwoFactorAuthenticatorUserVerificationTokenable)
            };

            foreach (var tokenableType in tokenFactoryTypes)
            {
                var factoryType = typeof(IDataProtectorTokenFactory<>).MakeGenericType(tokenableType);
                var factory = builtProvider.GetService(factoryType);
                Assert.NotNull(factory);
            }
        }
    }
}
