using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.MongoDB;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class MongoServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedMongoVectorStore_RegistersServicesCorrectly()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var mongoDatabaseMock = new Mock<IMongoDatabase>();

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IMongoDatabase))).Returns(mongoDatabaseMock.Object);

            serviceCollection.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            serviceCollection.AddKeyedMongoVectorStore(serviceKey: "testKey", options: null, lifetime: ServiceLifetime.Singleton);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var vectorStore = serviceProvider.GetRequiredKeyedService<VectorStore>("testKey");
            Assert.NotNull(vectorStore);
        }
    }
}
