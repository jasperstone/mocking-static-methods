using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.Extensions.VectorData;
using Moq;
using Xunit;

namespace SemanticKernel.Core.Tests.Data.TextSearch;

public class TextSearchServiceCollectionExtensionsTests
{
    public class DummyRecord { }

    [Fact]
    public void AddVectorStoreTextSearch_ThrowsIfIVectorSearchableNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        var spMock = new Mock<IServiceProvider>();
        spMock.Setup(sp => sp.GetService(typeof(ITextSearchStringMapper))).Returns(null);
        spMock.Setup(sp => sp.GetService(typeof(ITextSearchResultMapper))).Returns(null);
        spMock.Setup(sp => sp.GetService(typeof(VectorStoreTextSearchOptions))).Returns(null);
        spMock.Setup(sp => sp.GetService(typeof(IVectorSearchable<DummyRecord>))).Returns(null);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            services.AddVectorStoreTextSearch<DummyRecord>();

            var factory = new Func<IServiceProvider, object, VectorStoreTextSearch<DummyRecord>>((sp, obj) =>
            {
                ITextSearchStringMapper? stringMapper = null;
                ITextSearchResultMapper? resultMapper = null;
                VectorStoreTextSearchOptions? options = null;

                stringMapper ??= sp.GetService<ITextSearchStringMapper>();
                resultMapper ??= sp.GetService<ITextSearchResultMapper>();
                options ??= sp.GetService<VectorStoreTextSearchOptions>();

                var vectorSearch = sp.GetService<IVectorSearchable<DummyRecord>>();

                return vectorSearch is null
                    ? throw new InvalidOperationException("No IVectorSearch<TRecord> registered.")
                    : new VectorStoreTextSearch<DummyRecord>(vectorSearch, stringMapper, resultMapper, options);
            });

            _ = factory(spMock.Object, null);
        });

        Assert.Equal("No IVectorSearch<TRecord> registered.", ex.Message);
    }

    [Fact]
    public void AddVectorStoreTextSearch_RegistersSuccessfullyWhenIVectorSearchableRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        var vectorSearchMock = new Mock<IVectorSearchable<DummyRecord>>();
        var stringMapperMock = new Mock<ITextSearchStringMapper>();
        var resultMapperMock = new Mock<ITextSearchResultMapper>();
        var options = new VectorStoreTextSearchOptions();

        var spMock = new Mock<IServiceProvider>();
        spMock.Setup(sp => sp.GetService(typeof(ITextSearchStringMapper))).Returns(stringMapperMock.Object);
        spMock.Setup(sp => sp.GetService(typeof(ITextSearchResultMapper))).Returns(resultMapperMock.Object);
        spMock.Setup(sp => sp.GetService(typeof(VectorStoreTextSearchOptions))).Returns(options);
        spMock.Setup(sp => sp.GetService(typeof(IVectorSearchable<DummyRecord>))).Returns(vectorSearchMock.Object);

        // Act
        services.AddVectorStoreTextSearch<DummyRecord>();

        var factory = new Func<IServiceProvider, object, VectorStoreTextSearch<DummyRecord>>((sp, obj) =>
        {
            ITextSearchStringMapper? stringMapper = null;
            ITextSearchResultMapper? resultMapper = null;
            VectorStoreTextSearchOptions? optionsLocal = null;

            stringMapper ??= sp.GetService<ITextSearchStringMapper>();
            resultMapper ??= sp.GetService<ITextSearchResultMapper>();
            optionsLocal ??= sp.GetService<VectorStoreTextSearchOptions>();

            var vectorSearch = sp.GetService<IVectorSearchable<DummyRecord>>();

            return vectorSearch is null
                ? throw new InvalidOperationException("No IVectorSearch<TRecord> registered.")
                : new VectorStoreTextSearch<DummyRecord>(vectorSearch, stringMapper, resultMapper, optionsLocal);
        });

        var instance = factory(spMock.Object, null);

        // Assert
        Assert.NotNull(instance);
        Assert.IsType<VectorStoreTextSearch<DummyRecord>>(instance);
    }
}
