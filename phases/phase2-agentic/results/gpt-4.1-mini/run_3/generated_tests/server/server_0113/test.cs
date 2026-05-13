using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.DataProtection;
using Xunit;
using Moq;
using Bit.SharedWeb.Utilities;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTokenizers_Registers_DataProtectorTokenFactories_And_Resolves_Logger()
        {
            // Arrange
            var services = new ServiceCollection();

            // Setup a mock IDataProtectionProvider to be returned by GetDataProtectionProvider extension method
            var mockDataProtectionProvider = new Mock<IDataProtectionProvider>();

            // Setup a mock ILogger for DataProtectorTokenFactory<EmergencyAccessInviteTokenable>
            var mockLoggerEmergency = new Mock<ILogger<DataProtectorTokenFactory<EmergencyAccessInviteTokenable>>>();

            // Setup a mock ILogger for DataProtectorTokenFactory<SsoTokenable>
            var mockLoggerSso = new Mock<ILogger<DataProtectorTokenFactory<SsoTokenable>>>();

            // Setup a mock ILogger for DataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>
            var mockLoggerWebAuthnCredential = new Mock<ILogger<DataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>>();

            // Setup a mock ILogger for DataProtectorTokenFactory<SsoEmail2faSessionTokenable>
            var mockLoggerSsoEmail2fa = new Mock<ILogger<DataProtectorTokenFactory<SsoEmail2faSessionTokenable>>>();

            // Register the mocks in the service collection
            services.AddSingleton(mockDataProtectionProvider.Object);

            // Register loggers for the token factories
            services.AddSingleton(mockLoggerEmergency.Object);
            services.AddSingleton(mockLoggerSso.Object);
            services.AddSingleton(mockLoggerWebAuthnCredential.Object);
            services.AddSingleton(mockLoggerSsoEmail2fa.Object);

            // Because the extension method calls serviceProvider.GetDataProtectionProvider() and GetRequiredService<ILogger<T>>(),
            // we need to provide these extension methods or mocks for them.
            // We will mock IServiceProvider.GetDataProtectionProvider() as an extension method by registering IDataProtectionProvider.
            // For GetRequiredService<ILogger<T>>, the default IServiceProvider.GetRequiredService<T> will resolve the registered ILogger<T>.

            // Act
            ServiceCollectionExtensions.AddTokenizers(services);

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            // Resolve one of the token factories to verify that the GetRequiredService<ILogger<T>> call works
            var emergencyAccessTokenFactory = serviceProvider.GetService<IDataProtectorTokenFactory<EmergencyAccessInviteTokenable>>();
            Assert.NotNull(emergencyAccessTokenFactory);

            var ssoTokenFactory = serviceProvider.GetService<IDataProtectorTokenFactory<SsoTokenable>>();
            Assert.NotNull(ssoTokenFactory);

            var webAuthnCredentialTokenFactory = serviceProvider.GetService<IDataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>();
            Assert.NotNull(webAuthnCredentialTokenFactory);

            var ssoEmail2faTokenFactory = serviceProvider.GetService<IDataProtectorTokenFactory<SsoEmail2faSessionTokenable>>();
            Assert.NotNull(ssoEmail2faTokenFactory);
        }
    }

    // Dummy classes to satisfy generic type parameters in tests
    public class EmergencyAccessInviteTokenable
    {
        public const string ClearTextPrefix = "prefix";
        public const string DataProtectorPurpose = "purpose";
    }

    public class SsoTokenable
    {
        public const string ClearTextPrefix = "prefix";
        public const string DataProtectorPurpose = "purpose";
    }

    public class WebAuthnCredentialCreateOptionsTokenable
    {
        public const string ClearTextPrefix = "prefix";
        public const string DataProtectorPurpose = "purpose";
    }

    public class SsoEmail2faSessionTokenable
    {
        public const string ClearTextPrefix = "prefix";
        public const string DataProtectorPurpose = "purpose";
    }

    // Dummy interface to match the token factory interface
    public interface IDataProtectorTokenFactory<T> { }

    // Dummy implementation of DataProtectorTokenFactory<T> to allow instantiation in tests
    public class DataProtectorTokenFactory<T> : IDataProtectorTokenFactory<T>
    {
        public DataProtectorTokenFactory(string clearTextPrefix, string dataProtectorPurpose, IDataProtectionProvider dataProtectionProvider, ILogger<DataProtectorTokenFactory<T>> logger)
        {
            // No-op constructor for testing
        }
    }

    // Extension method to simulate GetDataProtectionProvider on IServiceProvider
    public static class ServiceProviderExtensions
    {
        public static IDataProtectionProvider GetDataProtectionProvider(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService<IDataProtectionProvider>();
        }
    }
}
