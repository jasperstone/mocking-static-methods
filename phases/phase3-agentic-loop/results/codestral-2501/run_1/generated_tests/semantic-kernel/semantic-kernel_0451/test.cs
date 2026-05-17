using System;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class CosmosNoSqlServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedCosmosNoSqlVectorStore_ShouldRegisterServices()
        {
            // Arrange
            var serviceCollectionMock = new Mock<IServiceCollection>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var databaseMock = new Mock<Database>();
            var options = new CosmosNoSqlVectorStoreOptions();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<Database>())
                .Returns(databaseMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IServiceProvider)))
                .Returns(serviceProviderMock.Object);

            var serviceDescriptor = new ServiceDescriptor(typeof(CosmosNoSqlVectorStore), (Func<IServiceProvider, object>)((sp) =>
            {
                var database = sp.GetRequiredService<Database>();

                return new CosmosNoSqlVectorStore(database, options);
            }), ServiceLifetime.Singleton);

            serviceCollectionMock
                .Setup(sc => sc.Add(It.IsAny<ServiceDescriptor>()))
                .Callback<ServiceDescriptor>(sd => serviceDescriptor = sd);

            // Act
            CosmosNoSqlServiceCollectionExtensions.AddKeyedCosmosNoSqlVectorStore(
                serviceCollectionMock.Object,
                serviceKey: null,
                options: options,
                lifetime: ServiceLifetime.Singleton);

            // Assert
            serviceCollectionMock.Verify(sc => sc.Add(It.IsAny<ServiceDescriptor>()), Times.Exactly(2));
            Assert.NotNull(serviceDescriptor);
            Assert.Equal(ServiceLifetime.Singleton, serviceDescriptor.Lifetime);
            Assert.Equal(typeof(CosmosNoSqlVectorStore), serviceDescriptor.ServiceType);
        }
    }
}
