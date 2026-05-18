using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Storage;
using Xunit;

namespace Orleans.Persistence.DynamoDB.Tests
{
    public class DynamoDBGrainStorageFactoryTests
    {
        [Fact]
        public void Create_ShouldCallGetRequiredServiceAndReturnInstance()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<DynamoDBStorageOptions>>();
            var activatorProviderMock = new Mock<IActivatorProvider>();
            var name = "TestStorage";
            var expectedOptions = new DynamoDBStorageOptions();

            optionsMonitorMock.Setup(m => m.Get(name)).Returns(expectedOptions);

            // Setup GetService to return mocks for required services
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptionsMonitor<DynamoDBStorageOptions>)))
                .Returns(optionsMonitorMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IActivatorProvider)))
                .Returns(activatorProviderMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILogger<DynamoDBGrainStorage>)))
                .Returns(Mock.Of<ILogger<DynamoDBGrainStorage>>());

            // Act
            var storage = DynamoDBGrainStorageFactory.Create(serviceProviderMock.Object, name);

            // Assert
            Assert.NotNull(storage);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>(), Times.Once);
            optionsMonitorMock.Verify(m => m.Get(name), Times.Once);
        }
    }

    internal static class ServiceProviderExtensions
    {
        public static T GetRequiredService<T>(this IServiceProvider provider)
        {
            var service = provider.GetService(typeof(T));
            if (service == null)
            {
                throw new InvalidOperationException($"Service of type {typeof(T)} not found");
            }
            return (T)service;
        }
    }
}
