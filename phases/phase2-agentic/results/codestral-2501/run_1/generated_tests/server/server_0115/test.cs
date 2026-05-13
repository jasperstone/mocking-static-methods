using System;
using Bit.Core.AdminConsole.Models.Business.Tokenables;
using Bit.Core.Auth.Models.Business.Tokenables;
using Bit.Core.Auth.Identity.TokenProviders;
using Bit.Core.Auth.Identity;
using Bit.Core.Auth.Repositories;
using Bit.Core.Auth.Services;
using Bit.Core.Auth.Services.Implementations;
using Bit.Core.Auth.UserFeatures;
using Bit.Core.Auth.UserFeatures.PasswordValidation;
using Bit.Core.Billing.Services;
using Bit.Core.Billing.Services.Implementations;
using Bit.Core.Billing.TrialInitiation;
using Bit.Core.Dirt.Reports.ReportFeatures;
using Bit.Core.Entities;
using Bit.Core.Enums;
using Bit.Core.HostedServices;
using Bit.Core.KeyManagement;
using Bit.Core.NotificationCenter;
using Bit.Core.OrganizationFeatures;
using Bit.Core.Platform;
using Bit.Core.Platform.Mail.Delivery;
using Bit.Core.Platform.Mail.Enqueuing;
using Bit.Core.Platform.Mail.Mailer;
using Bit.Core.Platform.Push;
using Bit.Core.Platform.PushRegistration.Internal;
using Bit.Core.Repositories;
using Bit.Core.Resources;
using Bit.Core.SecretsManager.Repositories;
using Bit.Core.SecretsManager.Repositories.Noop;
using Bit.Core.Services;
using Bit.Core.Services.Implementations;
using Bit.Core.Services.Mail;
using Bit.Core.Settings;
using Bit.Core.Tokens;
using Bit.Core.Tools.ImportFeatures;
using Bit.Core.Tools.SendFeatures;
using Bit.Core.Tools.Services;
using Bit.Core.Utilities;
using Bit.Core.Vault;
using Bit.Core.Vault.Services;
using Bit.Infrastructure.Dapper;
using Bit.Infrastructure.EntityFramework;
using Bit.SharedWeb.Utilities;
using DnsClient;
using Duende.IdentityModel;
using LaunchDarkly.Sdk.Server;
using LaunchDarkly.Sdk.Server.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Caching.Cosmos;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StackExchange.Redis;
using Xunit;
using NoopRepos = Bit.Core.Repositories.Noop;
using Role = Bit.Core.Entities.Role;
using TableStorageRepos = Bit.Core.Repositories.TableStorage;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTokenizers_ShouldRegisterDataProtectorTokenFactories()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var dataProtectionProviderMock = new Mock<IDataProtectionProvider>();
            var loggerMock = new Mock<ILogger<DataProtectorTokenFactory<DuoUserStateTokenable>>>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IDataProtectionProvider)))
                .Returns(dataProtectionProviderMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService(typeof(ILogger<DataProtectorTokenFactory<DuoUserStateTokenable>>)))
                .Returns(loggerMock.Object);

            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Act
            serviceCollection.AddTokenizers();

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var tokenFactory = serviceProvider.GetRequiredService<IDataProtectorTokenFactory<DuoUserStateTokenable>>();

            Assert.NotNull(tokenFactory);
            Assert.IsType<DataProtectorTokenFactory<DuoUserStateTokenable>>(tokenFactory);
        }

        [Fact]
        public void AddTokenizers_ShouldCallGetRequiredServiceForLogger()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var dataProtectionProviderMock = new Mock<IDataProtectionProvider>();
            var loggerMock = new Mock<ILogger<DataProtectorTokenFactory<DuoUserStateTokenable>>>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IDataProtectionProvider)))
                .Returns(dataProtectionProviderMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService(typeof(ILogger<DataProtectorTokenFactory<DuoUserStateTokenable>>)))
                .Returns(loggerMock.Object);

            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Act
            serviceCollection.AddTokenizers();

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(ILogger<DataProtectorTokenFactory<DuoUserStateTokenable>>)), Times.Once);
        }
    }
}
