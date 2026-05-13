using System;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

public class DictionaryModelBinderProviderTests
{
    [Fact]
    public void GetBinder_ShouldRetrieveMvcOptionsFromServices()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockMvcOptions = new Mock<IOptions<MvcOptions>>();
        var mockMvcOptionsValue = new MvcOptions();
        mockMvcOptions.Setup(m => m.Value).Returns(mockMvcOptionsValue);

        mockServiceProvider
            .Setup(s => s.GetRequiredService<IOptions<MvcOptions>>())
            .Returns(mockMvcOptions.Object);

        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockMetadataProvider = new Mock<IModelMetadataProvider>();
        var mockMetadata = new Mock<ModelMetadata>();
        mockMetadata.Setup(m => m.ModelType).Returns(typeof(IDictionary<string, int>));

        var context = new Mock<ModelBinderProviderContext>();
        context.Setup(c => c.Services).Returns(mockServiceProvider.Object);
        context.Setup(c => c.Metadata).Returns(mockMetadata.Object);
        context.Setup(c => c.MetadataProvider).Returns(mockMetadataProvider.Object);

        var provider = new DictionaryModelBinderProvider();

        // Act
        var binder = provider.GetBinder(context.Object);

        // Assert
        Assert.NotNull(binder);
        Assert.IsType<DictionaryModelBinder<string, int>>(binder);
        mockServiceProvider.Verify(s => s.GetRequiredService<IOptions<MvcOptions>>(), Times.Once);
    }
}
