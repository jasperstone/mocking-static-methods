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
        public void AddTokenizers_RegistersDataProtectorTokenFactories_AndResolvesLogger()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add a mock IDataProtectionProvider to the service collection
            var mockDataProtectionProvider = new Mock<IDataProtectionProvider>();
            services.AddSingleton(mockDataProtectionProvider.Object);

            // Add a mock ILogger for the generic DataProtectorTokenFactory<T>
            // We will add a generic logger factory that returns a mock logger for any T
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

            // Act
            ServiceCollectionExtensions.AddTokenizers(services);

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            // We want to verify that the service provider can resolve the IDataProtectorTokenFactory for one of the tokenable types
            // For example, EmergencyAccessInviteTokenable
            var tokenFactory = serviceProvider.GetService<IDataProtectorTokenFactory<EmergencyAccessInviteTokenable>>();
            Assert.NotNull(tokenFactory);

            // Also verify that the logger was resolved (indirectly tested by no exception thrown)
            var logger = serviceProvider.GetService<ILogger<DataProtectorTokenFactory<EmergencyAccessInviteTokenable>>>();
            Assert.NotNull(logger);
        }
    }

    // Minimal stub types to allow compilation of the test
    // These are only to satisfy the generic constraints and references in the tested code

    public interface IDataProtectorTokenFactory<T> { }

    public class DataProtectorTokenFactory<T> : IDataProtectorTokenFactory<T>
    {
        public DataProtectorTokenFactory(string clearTextPrefix, string dataProtectorPurpose, IDataProtectionProvider dataProtectionProvider, ILogger<DataProtectorTokenFactory<T>> logger)
        {
            // No implementation needed for test
        }
    }

    public class EmergencyAccessInviteTokenable
    {
        public static string ClearTextPrefix => "prefix";
        public static string DataProtectorPurpose => "purpose";
    }
}
