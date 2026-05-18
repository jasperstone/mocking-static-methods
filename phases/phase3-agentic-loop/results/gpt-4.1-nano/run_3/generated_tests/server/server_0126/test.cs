using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using Bit.SharedWeb.Utilities;
using Bit.Core;
using Bit.Core.Entities;
using Bit.Core.Repositories;
using Bit.Core.Services;
using Bit.Core.Settings;

namespace Bit.Tests.Utilities
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDatabaseRepositories_ShouldReturnSupportedDatabaseProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var globalSettings = new GlobalSettings
            {
                SelfHosted = false,
                DatabaseProvider = "sqlserver",
                SqlServer = new SqlServerSettings { ConnectionString = "conn" }
            };

            // Act
            var provider = services.AddDatabaseRepositories(globalSettings);

            // Assert
            Assert.IsType<SupportedDatabaseProviders>(provider);
            Assert.Contains(services, s => s.ServiceType == typeof(IEventRepository));
        }

        [Fact]
        public void AddBaseServices_ShouldRegisterServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockSettings = new Mock<IGlobalSettings>();
            mockSettings.Setup(s => s).Returns(It.IsAny<IGlobalSettings>());

            // Act
            services.AddBaseServices(mockSettings.Object);

            // Assert
            Assert.Contains(services, s => s.ServiceType == typeof(ICipherService));
            Assert.Contains(services, s => s.ServiceType == typeof(IGroupService));
        }

        [Fact]
        public void AddTokenizers_ShouldRegisterTokenFactories()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddTokenizers();

            // Assert
            Assert.Contains(services, s => s.ServiceType == typeof(IDataProtectorTokenFactory<OrgDeleteTokenable>));
            Assert.Contains(services, s => s.ServiceType == typeof(IDataProtectorTokenFactory<SsoTokenable>));
        }

        [Fact]
        public void GetRequiredService_ShouldBeCalledOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockProvider = new Mock<IServiceProvider>();
            var mockLogger = new Mock<ILogger<DataProtectorTokenFactory<OrgDeleteTokenable>>>();
            var tokenFactory = new DataProtectorTokenFactory<OrgDeleteTokenable>(
                "prefix",
                "purpose",
                new Mock<IDataProtectionProvider>().Object,
                mockLogger.Object);

            mockProvider.Setup(p => p.GetRequiredService<ILogger<DataProtectorTokenFactory<OrgDeleteTokenable>>>()).Returns(mockLogger.Object);

            // Act
            var factory = new DataProtectorTokenFactory<OrgDeleteTokenable>(
                "prefix",
                "purpose",
                new Mock<IDataProtectionProvider>().Object,
                mockLogger.Object);

            // Assert
            Assert.NotNull(factory);
        }
    }
}
