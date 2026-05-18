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
        public void AddTokenizers_RegistersDataProtectorTokenFactories_AndUsesGetRequiredServiceLogger()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add a mock ILogger for DataProtectorTokenFactory<EmergencyAccessInviteTokenable>
            var loggerMock = new Mock<ILogger<DataProtectorTokenFactory<EmergencyAccessInviteTokenable>>>();
            services.AddSingleton(loggerMock.Object);

            // Add a mock IDataProtectionProvider
            var dataProtectionProviderMock = new Mock<IDataProtectionProvider>();
            services.AddSingleton(dataProtectionProviderMock.Object);

            // Add extension method to IServiceProvider to get IDataProtectionProvider
            services.AddSingleton<IServiceProvider>(sp => sp);

            // Act
            services.AddTokenizers();

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var factory = serviceProvider.GetService<IDataProtectorTokenFactory<EmergencyAccessInviteTokenable>>();
            Assert.NotNull(factory);

            // The factory should be of type DataProtectorTokenFactory<EmergencyAccessInviteTokenable>
            Assert.IsType<DataProtectorTokenFactory<EmergencyAccessInviteTokenable>>(factory);
        }
    }
}
