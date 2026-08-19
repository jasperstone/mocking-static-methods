using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests;

public class QdrantServiceCollectionExtensionsTests
{
    [Fact]
    public void AddKeyedQdrantCollection_GetService_IEmbeddingGenerator()
    {
        // Arrange
        var services = new ServiceCollection();
        var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
        services.AddSingleton(embeddingGeneratorMock.Object);

        // Act
        services.AddKeyedQdrantCollection<object, object>(
            serviceKey: null,
            name: "test",
            _ => new QdrantClient("localhost", 6334, true, null),
            sp => new QdrantCollectionOptions(),
            ServiceLifetime.Singleton);

        var serviceProvider = services.BuildServiceProvider(validateScopes: true);

        // Assert
        var qdrantCollection = serviceProvider.GetService<QdrantCollection<object, object>>();
        Assert.NotNull(qdrantCollection);
    }
}
