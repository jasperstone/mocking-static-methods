using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Persistence.AzureStorage;
using Orleans.Storage;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Orleans.Tests
{
    public class AzureTableGrainStorageFactoryTests
    {
        [Fact]
        public async Task Create_AzureTableGrainStorageFactory_CreatesInstance()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<AzureTableStorageOptions>();
            services.AddSingleton<IGrainStorageSerializer, MockGrainStorageSerializer>();
            services.AddSingleton<IActivatorProvider, MockActivatorProvider>();
            var serviceProvider = services.BuildServiceProvider();

            var optionsMonitor = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();
            optionsMonitor.Setup(m => m.Get(It.IsAny<string>())).Returns(new AzureTableStorageOptions());
            serviceProvider.GetService<IOptionsMonitor<AzureTableStorageOptions>>().Get("test");

            // Act
            var instance = AzureTableGrainStorageFactory.Create(serviceProvider, "test");

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public async Task Create_AzureTableGrainStorageFactory_ThrowsException_WhenOptionsMonitorIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<AzureTableStorageOptions>();
            services.AddSingleton<IGrainStorageSerializer, MockGrainStorageSerializer>();
            services.AddSingleton<IActivatorProvider, MockActivatorProvider>();
            var serviceProvider = services.BuildServiceProvider();

            var optionsMonitor = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();
            optionsMonitor.Setup(m => m.Get(It.IsAny<string>())).Returns((AzureTableStorageOptions)null);
            serviceProvider.GetService<IOptionsMonitor<AzureTableStorageOptions>>().Get("test");

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => AzureTableGrainStorageFactory.Create(serviceProvider, "test"));
        }

        private class MockGrainStorageSerializer : IGrainStorageSerializer
        {
            public Task<object> DeserializeAsync(byte[] data, Type grainStateType)
            {
                throw new NotImplementedException();
            }

            public Task<byte[]> SerializeAsync(object grainState)
            {
                throw new NotImplementedException();
            }
        }

        private class MockActivatorProvider : IActivatorProvider
        {
            public object CreateInstance(Type type, params object[] args)
            {
                throw new NotImplementedException();
            }
        }
    }
}
