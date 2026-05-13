using System;
using System.Collections.Generic;
using System.Linq;
using Bit.Core.Auth.Models.Business.Tokenables;
using Bit.Core.Tokens;
using Bit.SharedWeb.Utilities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTokenizers_SsoEmail2faSessionTokenFactoryThrowsWhenLoggerMissing()
        {
            var services = new ServiceCollection();
            services.AddTokenizers();

            var descriptor = services.Single(sd => sd.ServiceType == typeof(IDataProtectorTokenFactory<SsoEmail2faSessionTokenable>));
            Assert.NotNull(descriptor.ImplementationFactory);

            var trackingProvider = new TrackingServiceProvider(new Dictionary<Type, object>
            {
                { typeof(IDataProtectionProvider), new StubDataProtectionProvider() }
            });

            var exception = Assert.Throws<InvalidOperationException>(() => descriptor.ImplementationFactory!(trackingProvider));

            Assert.Contains("ILogger", exception.Message);
            Assert.Contains(nameof(SsoEmail2faSessionTokenable), exception.Message);
            Assert.Contains(typeof(ILogger<DataProtectorTokenFactory<SsoEmail2faSessionTokenable>>), trackingProvider.RequestedTypes);
        }

        [Fact]
        public void AddTokenizers_ResolvesSsoEmail2faSessionTokenFactory()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IDataProtectionProvider>(new StubDataProtectionProvider());
            services.AddSingleton<ILogger<DataProtectorTokenFactory<SsoEmail2faSessionTokenable>>>(NullLogger<DataProtectorTokenFactory<SsoEmail2faSessionTokenable>>.Instance);
            services.AddTokenizers();

            using var provider = services.BuildServiceProvider();

            var factory = provider.GetRequiredService<IDataProtectorTokenFactory<SsoEmail2faSessionTokenable>>();

            Assert.NotNull(factory);
            Assert.IsType<DataProtectorTokenFactory<SsoEmail2faSessionTokenable>>(factory);
        }

        private sealed class TrackingServiceProvider : IServiceProvider
        {
            private readonly IDictionary<Type, object> _services;
            private readonly List<Type> _requestedTypes = new();

            public TrackingServiceProvider(IDictionary<Type, object> services)
            {
                _services = services;
            }

            public IReadOnlyList<Type> RequestedTypes => _requestedTypes;

            public object GetService(Type serviceType)
            {
                _requestedTypes.Add(serviceType);
                _services.TryGetValue(serviceType, out var service);
                return service;
            }
        }

        private sealed class StubDataProtectionProvider : IDataProtectionProvider
        {
            public IDataProtector CreateProtector(string purpose) => new StubDataProtector();

            private sealed class StubDataProtector : IDataProtector
            {
                public IDataProtector CreateProtector(string purpose) => this;

                public byte[] Protect(byte[] plaintext) => plaintext;

                public byte[] Unprotect(byte[] protectedData) => protectedData;

                public string Protect(string plaintext) => plaintext;

                public string Unprotect(string protectedData) => protectedData;
            }
        }
    }
}
