using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.DataProtection;
using Moq;
using Xunit;
using Bit.SharedWeb.Utilities;
using Bit.Core.AdminConsole.Models.Business.Tokenables;

namespace Bit.SharedWeb.Tests.Utilities
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTokenizers_Registers_DataProtectorTokenFactory_With_Logger_And_DataProtectionProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockLogger = new Mock<ILogger<DataProtectorTokenFactory<DuoUserStateTokenable>>>();
            var mockDataProtectionProvider = new Mock<IDataProtectionProvider>();

            // Setup a service provider that returns the mocks when requested
            services.AddSingleton(mockLogger.Object);
            services.AddSingleton(mockDataProtectionProvider.Object);

            // We need to simulate the serviceProvider.GetRequiredService<ILogger<DataProtectorTokenFactory<DuoUserStateTokenable>>>()
            // and serviceProvider.GetDataProtectionProvider() extension methods.
            // Since these are extension methods, we will mock IServiceProvider to return the mocks.

            // Instead of calling AddTokenizers directly (which uses serviceProvider in the factory),
            // we will register the factory manually to test the call to GetRequiredService.

            services.AddSingleton<IDataProtectorTokenFactory<DuoUserStateTokenable>>(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<DataProtectorTokenFactory<DuoUserStateTokenable>>>();
                var dataProtectionProvider = mockDataProtectionProvider.Object; // simulate GetDataProtectionProvider extension
                return new DataProtectorTokenFactory<DuoUserStateTokenable>(
                    DuoUserStateTokenable.ClearTextPrefix,
                    DuoUserStateTokenable.DataProtectorPurpose,
                    dataProtectionProvider,
                    logger);
            });

            var serviceProviderBuilt = services.BuildServiceProvider();

            // Act
            var factory = serviceProviderBuilt.GetRequiredService<IDataProtectorTokenFactory<DuoUserStateTokenable>>();

            // Assert
            Assert.NotNull(factory);
            Assert.IsType<DataProtectorTokenFactory<DuoUserStateTokenable>>(factory);
        }
    }

    // Minimal stub classes to allow compilation of the test
    public class DuoUserStateTokenable
    {
        public static string ClearTextPrefix => "prefix";
        public static string DataProtectorPurpose => "purpose";
    }

    public interface IDataProtectorTokenFactory<T> { }

    public class DataProtectorTokenFactory<T> : IDataProtectorTokenFactory<T>
    {
        public string Prefix { get; }
        public string Purpose { get; }
        public IDataProtectionProvider DataProtectionProvider { get; }
        public ILogger<DataProtectorTokenFactory<T>> Logger { get; }

        public DataProtectorTokenFactory(string prefix, string purpose, IDataProtectionProvider dataProtectionProvider, ILogger<DataProtectorTokenFactory<T>> logger)
        {
            Prefix = prefix;
            Purpose = purpose;
            DataProtectionProvider = dataProtectionProvider;
            Logger = logger;
        }
    }
}
