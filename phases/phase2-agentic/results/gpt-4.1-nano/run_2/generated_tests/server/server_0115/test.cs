using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Bit.SharedWeb.Utilities;

namespace ServiceCollectionExtensionsTests
{
    public class AddTokenizersTests
    {
        [Fact]
        public void AddTokenizers_ShouldRegisterAllTokenFactories()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockDataProtectionProvider = new Mock<IDataProtectionProvider>();
            var mockLogger = new Mock<ILogger<DataProtectorTokenFactory<object>>>();

            // Setup service provider to return mocks
            var serviceProvider = new ServiceCollection()
                .AddSingleton(mockDataProtectionProvider.Object)
                .AddSingleton(mockLogger.Object)
                .BuildServiceProvider();

            // Act
            services.AddTokenizers();

            // Build the service provider to resolve services
            var provider = services.BuildServiceProvider();

            // Assert
            // Check that the factories are registered
            var factory = provider.GetService<IDataProtectorTokenFactory<OrgDeleteTokenable>>();
            Assert.NotNull(factory);
            Assert.IsType<DataProtectorTokenFactory<OrgDeleteTokenable>>(factory);

            var ssoFactory = provider.GetService<IDataProtectorTokenFactory<SsoTokenable>>();
            Assert.NotNull(ssoFactory);
            Assert.IsType<DataProtectorTokenFactory<SsoTokenable>>(ssoFactory);

            var webAuthnFactory = provider.GetService<IDataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>();
            Assert.NotNull(webAuthnFactory);
            Assert.IsType<DataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>(webAuthnFactory);
        }
    }
}
