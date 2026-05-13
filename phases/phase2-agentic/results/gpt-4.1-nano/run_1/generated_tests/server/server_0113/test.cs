using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Bit.SharedWeb.Utilities;

namespace Bit.SharedWeb.Tests.Utilities
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTokenizers_Should_Register_All_TokenFactories_With_GetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockDataProtectionProvider = new Mock<IDataProtectionProvider>();
            var mockLoggerType = typeof(ILogger<DataProtectorTokenFactory<OrgDeleteTokenable>>);
            var mockLogger = new Mock<ILogger<DataProtectorTokenFactory<OrgDeleteTokenable>>>();
            var mockLogger2 = new Mock<ILogger<DataProtectorTokenFactory<EmergencyAccessInviteTokenable>>>();
            var mockLogger3 = new Mock<ILogger<DataProtectorTokenFactory<SsoTokenable>>>();
            var mockLogger4 = new Mock<ILogger<DataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>>();

            var serviceProvider = new ServiceCollection()
                .AddSingleton(mockDataProtectionProvider.Object)
                .AddSingleton(mockLogger.Object)
                .BuildServiceProvider();

            services.AddSingleton(serviceProvider.GetRequiredService<ILogger<DataProtectorTokenFactory<OrgDeleteTokenable>>>(), mockLogger.Object);
            services.AddSingleton(serviceProvider.GetRequiredService<ILogger<DataProtectorTokenFactory<EmergencyAccessInviteTokenable>>>(), mockLogger2.Object);
            services.AddSingleton(serviceProvider.GetRequiredService<ILogger<DataProtectorTokenFactory<SsoTokenable>>>(), mockLogger3.Object);
            services.AddSingleton(serviceProvider.GetRequiredService<ILogger<DataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>>(), mockLogger4.Object);

            // Act
            services.AddTokenizers();

            // Assert
            var provider = services.BuildServiceProvider();

            var factory1 = provider.GetService<IDataProtectorTokenFactory<OrgDeleteTokenable>>();
            var factory2 = provider.GetService<IDataProtectorTokenFactory<EmergencyAccessInviteTokenable>>();
            var factory3 = provider.GetService<IDataProtectorTokenFactory<SsoTokenable>>();
            var factory4 = provider.GetService<IDataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>();

            Assert.NotNull(factory1);
            Assert.NotNull(factory2);
            Assert.NotNull(factory3);
            Assert.NotNull(factory4);
        }
    }
}
