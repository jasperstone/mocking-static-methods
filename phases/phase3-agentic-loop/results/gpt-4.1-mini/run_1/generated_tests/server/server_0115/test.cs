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
        public void AddTokenizers_Registers_DataProtectorTokenFactory_And_Resolves_It()
        {
            // Arrange
            var services = new ServiceCollection();

            // Mock ILogger for DataProtectorTokenFactory<DuoUserStateTokenable>
            var loggerMock = new Mock<ILogger<DataProtectorTokenFactory<DuoUserStateTokenable>>>();
            services.AddSingleton(loggerMock.Object);

            // Mock IDataProtectionProvider
            var dataProtectionProviderMock = new Mock<IDataProtectionProvider>();
            services.AddSingleton(dataProtectionProviderMock.Object);

            // Act
            services.AddTokenizers();

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var factory = serviceProvider.GetService<IDataProtectorTokenFactory<DuoUserStateTokenable>>();
            Assert.NotNull(factory);
            Assert.IsType<DataProtectorTokenFactory<DuoUserStateTokenable>>(factory);
        }
    }
}
