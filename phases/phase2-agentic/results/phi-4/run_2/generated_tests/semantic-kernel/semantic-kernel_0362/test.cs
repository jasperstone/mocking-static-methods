using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;

public class TextSearchServiceCollectionExtensionsTests
{
    [Fact]
    public void AddVectorStoreTextSearch_ShouldRegisterVectorStoreTextSearch_WhenIVectorSearchableIsProvided()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var vectorSearchableMock = new Mock<IVectorSearchable<object>>();

        serviceProviderMock
            .Setup(sp => sp.GetService<IVectorSearchable<object>>())
            .Returns(vectorSearchableMock.Object);

        // Act
        services.AddVectorStoreTextSearch<object>(serviceProviderMock.Object);

        // Assert
        var provider = services.BuildServiceProvider();
        var service = provider.GetService<VectorStoreTextSearch<object>>();

        Assert.NotNull(service);
        Assert.Same(vectorSearchableMock.Object, ((VectorStoreTextSearch<object>)service)._vectorSearch);
    }
}
