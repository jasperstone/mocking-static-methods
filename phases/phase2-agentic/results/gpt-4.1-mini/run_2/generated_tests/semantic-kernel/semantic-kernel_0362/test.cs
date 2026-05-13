using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.Extensions.VectorData;
using Moq;
using Xunit;

namespace SemanticKernel.Tests.Data.TextSearch;

public class TextSearchServiceCollectionExtensionsTests
{
    private class DummyRecord { }

    [Fact]
    public void AddVectorStoreTextSearch_ThrowsIfIVectorSearchableNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        // We need to mock AddKeyedTransient to invoke the factory to test the GetService call.
        // So we add a fake AddKeyedTransient extension method for testing.
        bool factoryInvoked = false;
        services.AddKeyedTransient = (serviceId, factory) =>
        {
            factoryInvoked = true;
            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetService(typeof(ITextSearchStringMapper))).Returns(null);
            spMock.Setup(sp => sp.GetService(typeof(ITextSearchResultMapper))).Returns(null);
            spMock.Setup(sp => sp.GetService(typeof(VectorStoreTextSearchOptions))).Returns(null);
            spMock.Setup(sp => sp.GetService(typeof(IVectorSearchable<DummyRecord>))).Returns(null);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => factory(spMock.Object, null));
            Assert.Equal("No IVectorSearch<TRecord> registered.", ex.Message);
            return services;
        };

        // Act & Assert
        var ex2 = Assert.Throws<InvalidOperationException>(() =>
            services.AddVectorStoreTextSearch<DummyRecord>());

        Assert.True(factoryInvoked);
    }

    [Fact]
    public void AddVectorStoreTextSearch_RegistersVectorStoreTextSearch_WhenIVectorSearchableRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        var vectorSearchableMock = new Mock<IVectorSearchable<DummyRecord>>();
        var stringMapperMock = new Mock<ITextSearchStringMapper>();
        var resultMapperMock = new Mock<ITextSearchResultMapper>();
        var options = new VectorStoreTextSearchOptions();

        var spMock = new Mock<IServiceProvider>();
        spMock.Setup(sp => sp.GetService(typeof(ITextSearchStringMapper))).Returns(stringMapperMock.Object);
        spMock.Setup(sp => sp.GetService(typeof(ITextSearchResultMapper))).Returns(resultMapperMock.Object);
        spMock.Setup(sp => sp.GetService(typeof(VectorStoreTextSearchOptions))).Returns(options);
        spMock.Setup(sp => sp.GetService(typeof(IVectorSearchable<DummyRecord>))).Returns(vectorSearchableMock.Object);

        bool factoryInvoked = false;
        services.AddKeyedTransient = (serviceId, factory) =>
        {
            factoryInvoked = true;
            var instance = factory(spMock.Object, null);
            Assert.NotNull(instance);
            Assert.IsType<VectorStoreTextSearch<DummyRecord>>(instance);
            return services;
        };

        // Act
        var result = services.AddVectorStoreTextSearch<DummyRecord>();

        // Assert
        Assert.Same(services, result);
        Assert.True(factoryInvoked);
    }
}

// Extension method to allow mocking AddKeyedTransient for testing
public static class ServiceCollectionExtensionsForTest
{
    public static Func<string?, Func<IServiceProvider, object?, object>, IServiceCollection>? AddKeyedTransient;

    public static IServiceCollection AddKeyedTransient<T>(
        this IServiceCollection services,
        string? serviceId,
        Func<IServiceProvider, object?, object> factory)
    {
        if (AddKeyedTransient != null)
        {
            return AddKeyedTransient(serviceId, factory);
        }
        throw new NotImplementedException("AddKeyedTransient is not implemented in this test context.");
    }
}
