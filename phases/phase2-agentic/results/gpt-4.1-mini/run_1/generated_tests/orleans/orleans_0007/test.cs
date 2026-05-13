using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Storage;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class AdoNetGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainStorage_CallsGetRequiredServiceOnIServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            // Setup a mock IOptionsMonitor to be returned by GetRequiredService
            var optionsMonitorMock = new Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            var options = new AdoNetGrainStorageOptions();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

            // Add the mock IOptionsMonitor to the service collection
            services.AddSingleton(optionsMonitorMock.Object);

            // We need to add the AddGrainStorage extension method dependency
            // Since AddGrainStorage is an extension method, we simulate it by adding a dummy implementation
            services.AddTransient<IGrainStorage, DummyGrainStorage>();

            // Act
            var result = AdoNetGrainStorageServiceCollectionExtensions.AddAdoNetGrainStorage(services, "TestStorage", ob => { });

            // Build the service provider to test the factory delegate
            var serviceProvider = services.BuildServiceProvider();

            // Retrieve the IConfigurationValidator service to trigger the factory delegate
            var validator = serviceProvider.GetService<IConfigurationValidator>();

            // Assert
            Assert.NotNull(validator);
            optionsMonitorMock.Verify(m => m.Get("TestStorage"), Times.Once);
            Assert.Same(options, ((AdoNetGrainStorageOptionsValidator)validator).Options);
            Assert.Equal("TestStorage", ((AdoNetGrainStorageOptionsValidator)validator).Name);
        }

        private class DummyGrainStorage : IGrainStorage
        {
            public System.Threading.Tasks.Task ClearStateAsync<T>(string grainType, GrainId grainReference, IGrainState<T> grainState) => 
                System.Threading.Tasks.Task.CompletedTask;

            public System.Threading.Tasks.Task ReadStateAsync<T>(string grainType, GrainId grainReference, IGrainState<T> grainState) => 
                System.Threading.Tasks.Task.CompletedTask;

            public System.Threading.Tasks.Task WriteStateAsync<T>(string grainType, GrainId grainReference, IGrainState<T> grainState) => 
                System.Threading.Tasks.Task.CompletedTask;
        }
    }
}
