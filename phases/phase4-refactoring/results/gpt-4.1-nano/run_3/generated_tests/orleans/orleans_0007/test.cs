using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace Orleans.Tests
{
    // Dummy class to simulate AdoNetGrainStorageOptions for testing purposes
    public class AdoNetGrainStorageOptions
    {
        public string ConnectionString { get; set; }
    }

    // Custom IServiceProvider to track calls to GetRequiredService<T>
    public class TrackingServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider _inner;
        public List<Type> RequestedServices { get; } = new List<Type>();

        public TrackingServiceProvider(IServiceProvider inner)
        {
            _inner = inner;
        }

        public object GetService(Type serviceType)
        {
            RequestedServices.Add(serviceType);
            return _inner.GetService(serviceType);
        }

        public T GetRequiredService<T>()
        {
            RequestedServices.Add(typeof(T));
            return (T)_inner.GetRequiredService(typeof(T));
        }
    }

    public class AdoNetGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainStorage_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add a dummy IOptionsMonitor<AdoNetGrainStorageOptions>
            var optionsMonitorMock = new Moq.Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(new AdoNetGrainStorageOptions());

            // Build a real service provider with the options monitor
            var innerProvider = services.BuildServiceProvider();

            // Wrap it with our tracking provider
            var trackingProvider = new TrackingServiceProvider(innerProvider);

            // Register the tracking provider as singleton
            services.AddSingleton<IServiceProvider>(trackingProvider);

            // Act
            services.AddAdoNetGrainStorage("TestStorage", options => { options.ConnectionString = "Data Source=Test"; });

            // Build the final provider
            var provider = services.BuildServiceProvider();

            // Retrieve the tracking provider
            var trackedProvider = provider.GetRequiredService<IServiceProvider>() as TrackingServiceProvider;

            // Assert
            Assert.Contains(typeof(IOptionsMonitor<AdoNetGrainStorageOptions>), trackedProvider.RequestedServices);
        }
    }
}
