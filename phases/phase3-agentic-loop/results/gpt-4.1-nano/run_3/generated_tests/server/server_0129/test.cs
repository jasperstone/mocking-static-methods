using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Bit.Core.Utilities;
using Bit.SharedWeb.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bit.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        private IServiceCollection CreateServiceCollection()
        {
            var services = new ServiceCollection();
            services.AddLogging(); // Add logging for dependencies
            services.AddOptions();

            // Add a dummy DataProtectionProvider
            services.AddDataProtection();

            // Add a dummy ILogger
            services.AddSingleton<ILogger<DataProtectorTokenFactory<OrgDeleteTokenable>>, NullLogger<DataProtectorTokenFactory<OrgDeleteTokenable>>>();
            services.AddSingleton<ILogger<DataProtectorTokenFactory<EmergencyAccessInviteTokenable>>, NullLogger<DataProtectorTokenFactory<EmergencyAccessInviteTokenable>>>();
            services.AddSingleton<ILogger<DataProtectorTokenFactory<SsoTokenable>>, NullLogger<DataProtectorTokenFactory<SsoTokenable>>>();
            services.AddSingleton<ILogger<DataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>, NullLogger<DataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>>();

            // Add a dummy DataProtectionProvider
            services.AddDataProtection();

            return services;
        }

        [Fact]
        public void AddTokenizers_Should_Register_All_TokenFactories()
        {
            var services = CreateServiceCollection();

            // Act
            services.AddTokenizers();

            // Assert
            var provider = services.BuildServiceProvider();

            var orgTokenFactory = provider.GetService<IDataProtectorTokenFactory<OrgDeleteTokenable>>();
            var emergencyTokenFactory = provider.GetService<IDataProtectorTokenFactory<EmergencyAccessInviteTokenable>>();
            var ssoTokenFactory = provider.GetService<IDataProtectorTokenFactory<SsoTokenable>>();
            var webAuthnTokenFactory = provider.GetService<IDataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>();

            Assert.NotNull(orgTokenFactory);
            Assert.NotNull(emergencyTokenFactory);
            Assert.NotNull(ssoTokenFactory);
            Assert.NotNull(webAuthnTokenFactory);
        }

        [Fact]
        public void AddDatabaseRepositories_Should_Return_Correct_Provider_For_Supported_Database()
        {
            var services = CreateServiceCollection();

            var globalSettings = new GlobalSettings
            {
                SelfHosted = false,
                // Set provider to SQLServer for testing
                DatabaseProvider = "sqlserver",
                SqlServer = new SqlServerSettings { ConnectionString = "dummy" }
            };

            var provider = services.AddDatabaseRepositories(globalSettings);

            Assert.Equal(SupportedDatabaseProviders.SqlServer, provider);
        }

        [Fact]
        public void AddDatabaseRepositories_Should_Return_Correct_Provider_For_Non_SqlServer()
        {
            var services = CreateServiceCollection();

            var globalSettings = new GlobalSettings
            {
                SelfHosted = false,
                // Set provider to something else
                DatabaseProvider = "postgres",
                SqlServer = new SqlServerSettings { ConnectionString = "dummy" }
            };

            var provider = services.AddDatabaseRepositories(globalSettings);

            Assert.NotEqual(SupportedDatabaseProviders.SqlServer, provider);
        }

        [Fact]
        public void AddBaseServices_Should_Register_Services()
        {
            var services = CreateServiceCollection();

            var globalSettings = new MockGlobalSettings();

            services.AddBaseServices(globalSettings);

            var provider = services.BuildServiceProvider();

            Assert.NotNull(provider.GetService<ICipherService>());
            Assert.NotNull(provider.GetService<IGroupService>());
            Assert.NotNull(provider.GetService<IEventService>());
            Assert.NotNull(provider.GetService<IEmergencyAccessService>());
            Assert.NotNull(provider.GetService<IDeviceService>());
            Assert.NotNull(provider.GetService<ISsoConfigService>());
            Assert.NotNull(provider.GetService<IAuthRequestService>());
            Assert.NotNull(provider.GetService<IDuoUniversalTokenService>());
            Assert.NotNull(provider.GetService<ISendAuthorizationService>());
            Assert.NotNull(provider.GetService<IOrganizationDomainService>());
        }

        private class MockGlobalSettings : IGlobalSettings
        {
            public bool SelfHosted => false;
            public string DatabaseProvider => "sqlserver";
            public SqlServerSettings SqlServer => new SqlServerSettings { ConnectionString = "dummy" };
        }
    }
}
