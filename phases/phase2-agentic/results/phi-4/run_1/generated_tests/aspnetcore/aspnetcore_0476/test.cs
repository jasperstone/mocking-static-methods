using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

public class DictionaryModelBinderProviderTests
{
    [Fact]
    public void GetBinder_WhenModelTypeIsDictionary_CallsGetRequiredServiceCorrectly()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        var mvcOptionsMock = new Mock<IOptions<MvcOptions>>();
        var mvcOptionsValueMock = new Mock<MvcOptions>();

        serviceProviderMock
            .Setup(s => s.GetRequiredService<ILoggerFactory>())
            .Returns(loggerFactoryMock.Object);

        serviceProviderMock
            .Setup(s => s.GetRequiredService<IOptions<MvcOptions>>())
            .Returns(mvcOptionsMock.Object);

        mvcOptionsMock
            .Setup(m => m.Value)
            .Returns(mvcOptionsValueMock.Object);

        var contextMock = new Mock<ModelBinderProviderContext>();
        contextMock
            .Setup(c => c.Services)
            .Returns(serviceProviderMock.Object);

        contextMock
            .Setup(c => c.Metadata.ModelType)
            .Returns(typeof(IDictionary<string, int>));

        var metadataProviderMock = new Mock<IModelMetadataProvider>();
        contextMock
            .Setup(c => c.MetadataProvider)
            .Returns(metadataProviderMock.Object);

        var keyBinderMock = new Mock<IModelBinder>();
        var valueBinderMock = new Mock<IModelBinder>();

        metadataProviderMock
            .Setup(m => m.GetMetadataForType(typeof(string)))
            .Returns(new EmptyModelMetadataProvider().GetMetadataForType(typeof(string)));

        metadataProviderMock
            .Setup(m => m.GetMetadataForType(typeof(int)))
            .Returns(new EmptyModelMetadataProvider().GetMetadataForType(typeof(int)));

        contextMock
            .Setup(c => c.CreateBinder(It.IsAny<ModelMetadata>()))
            .Returns<IModelMetadata>(metadata => metadata.ModelType == typeof(string) ? keyBinderMock.Object : valueBinderMock.Object);

        var provider = new DictionaryModelBinderProvider();

        // Act
        var binder = provider.GetBinder(contextMock.Object);

        // Assert
        Assert.NotNull(binder);
        Assert.IsType<DictionaryModelBinder<string, int>>(binder);

        serviceProviderMock.Verify(s => s.GetRequiredService<ILoggerFactory>(), Times.Once);
        serviceProviderMock.Verify(s => s.GetRequiredService<IOptions<MvcOptions>>(), Times.Once);
    }
}
