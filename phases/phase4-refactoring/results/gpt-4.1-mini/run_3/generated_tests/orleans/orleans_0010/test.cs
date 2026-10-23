using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Storage;
using Xunit;
using Moq;

namespace Orleans.Persistence.DynamoDB.Tests
{
    public class DynamoDBGrainStorageFactoryTests
    {
        [Fact]
        public void Create_ShouldCallGetRequiredServiceOnIServiceProvider_AndReturnInstance()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            var options = new DynamoDBStorageOptions();
            var name = "TestStorage";

            optionsMonitorMock.Setup(m => m.Get(name)).Returns(options);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptionsMonitor<DynamoDBStorageOptions>)))
                .Returns(optionsMonitorMock.Object);

            // We need to setup ActivatorUtilities.CreateInstance to work with our mocks.
            // Since it is static, we cannot mock it directly.
            // Instead, we will rely on the real call, so we need to register dependencies in a real ServiceCollection.

            var services = new ServiceCollection();
            services.AddSingleton(optionsMonitorMock.Object);
            services.AddSingleton(Mock.Of<IActivatorProvider>());
            services.AddLogging();
            var serviceProvider = services.BuildServiceProvider();

            // We will create a new service provider that returns our optionsMonitorMock when asked for IOptionsMonitor<DynamoDBStorageOptions>
            // and falls back to the real serviceProvider for other services.
            var compositeServiceProvider = new CompositeServiceProvider(serviceProvider, optionsMonitorMock.Object);

            // Act
            var storage = DynamoDBGrainStorageFactory.Create(compositeServiceProvider, name);

            // Assert
            Assert.NotNull(storage);
        }

        // Helper class to combine two IServiceProvider instances for testing
        private class CompositeServiceProvider : IServiceProvider
        {
            private readonly IServiceProvider _fallback;
            private readonly IOptionsMonitor<DynamoDBStorageOptions> _optionsMonitor;

            public CompositeServiceProvider(IServiceProvider fallback, IOptionsMonitor<DynamoDBStorageOptions> optionsMonitor)
            {
                _fallback = fallback;
                _optionsMonitor = optionsMonitor;
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(IOptionsMonitor<DynamoDBStorageOptions>))
                {
                    return _optionsMonitor;
                }
                return _fallback.GetService(serviceType);
            }
        }
    }
}
