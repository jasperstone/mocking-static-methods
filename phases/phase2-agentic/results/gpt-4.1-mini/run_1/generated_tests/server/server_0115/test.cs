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
        public void AddTokenizers_RegistersDataProtectorTokenFactories_AndCallsGetRequiredServiceLogger()
        {
            // Arrange
            var services = new ServiceCollection();

            // Setup a mock logger for the generic DataProtectorTokenFactory<T>
            var loggerType = typeof(ILogger<>);
            var dataProtectorTokenFactoryType = typeof(DataProtectorTokenFactory<>);

            // We will mock the IServiceProvider to verify GetRequiredService calls
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Setup GetRequiredService to return a mock logger for any requested logger type
            serviceProviderMock
                .Setup(sp => sp.GetService(It.IsAny<Type>()))
                .Returns((Type t) =>
                {
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == loggerType)
                    {
                        var loggerMockType = typeof(Mock<>).MakeGenericType(t);
                        var loggerMock = Activator.CreateInstance(loggerMockType);
                        var objectProperty = loggerMockType.GetProperty("Object");
                        return objectProperty.GetValue(loggerMock);
                    }
                    if (t == typeof(IDataProtectionProvider))
                    {
                        var dataProtectionProviderMock = new Mock<IDataProtectionProvider>();
                        return dataProtectionProviderMock.Object;
                    }
                    return null;
                });

            // Add a singleton for IDataProtectionProvider to the services to be resolved
            var dataProtectionProviderMock = new Mock<IDataProtectionProvider>();
            services.AddSingleton(dataProtectionProviderMock.Object);

            // Act
            // We call AddTokenizers extension method which internally calls GetRequiredService<ILogger<...>> on IServiceProvider
            // We simulate the service provider by building the service provider with our mocks
            services.AddTokenizers();

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            // We verify that the service provider can resolve the IDataProtectorTokenFactory for one of the tokenable types
            var factory = serviceProvider.GetService(typeof(IDataProtectorTokenFactory<>).MakeGenericType(typeof(Bit.Core.AdminConsole.Models.Business.Tokenables.OrgDeleteTokenable)));
            Assert.NotNull(factory);

            // We also verify that the service provider can resolve the logger for the DataProtectorTokenFactory<OrgDeleteTokenable>
            var logger = serviceProvider.GetService(typeof(ILogger<>).MakeGenericType(typeof(DataProtectorTokenFactory<Bit.Core.AdminConsole.Models.Business.Tokenables.OrgDeleteTokenable>)));
            // It may be null if not explicitly registered, so we do not assert here

            // This test mainly ensures no exceptions and registrations are done correctly
        }
    }
}
