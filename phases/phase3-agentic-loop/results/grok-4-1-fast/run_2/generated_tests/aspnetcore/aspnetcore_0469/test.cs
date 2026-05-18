using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

public class ArrayModelBinderProviderTests
{
    [Fact]
    public void GetBinder_NullContext_ThrowsArgumentNullException()
    {
        // Arrange
        var provider = new ArrayModelBinderProvider();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => provider.GetBinder(null!));
        Assert.Equal("context", exception.ParamName);
    }

    [Fact]
    public void GetBinder_NonArrayModelType_ReturnsNull()
    {
        // Arrange
        var metadataMock = new Mock<ModelMetadata>();
        metadataMock.SetupGet(m => m.ModelType).Returns(typeof(string));

        var contextMock = new Mock<ModelBinderProviderContext>();
        contextMock.SetupGet(c => c.Metadata).Returns(metadataMock.Object);

        var provider = new ArrayModelBinderProvider();

        // Act
        var result = provider.GetBinder(contextMock.Object);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetBinder_ArrayModelType_CreatesArrayModelBinderAndCallsGetRequiredServiceMvcOptions()
    {
        // Arrange
        var elementType = typeof(string);
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        var mvcOptions = new MvcOptions();
        var optionsMock = new Mock<IOptions<MvcOptions>>();
        optionsMock.Setup(o => o.Value).Returns(mvcOptions);

        var servicesMock = new Mock<IServiceProvider>();
        servicesMock.Setup(s => s.GetRequiredService<ILoggerFactory>()).Returns(loggerFactoryMock.Object);
        servicesMock.Setup(s => s.GetRequiredService<IOptions<MvcOptions>>()).Returns(optionsMock.Object);

        var elementMetadataMock = new Mock<ModelMetadata>();
        elementMetadataMock.SetupGet(m => m.ModelType).Returns(elementType);

        var metadataMock = new Mock<ModelMetadata>();
        metadataMock.SetupGet(m => m.ModelType).Returns(elementType.MakeArrayType());
        metadataMock.SetupGet(m => m.ElementMetadata).Returns(elementMetadataMock.Object);

        var elementBinderMock = new Mock<IModelBinder>();

        var contextMock = new Mock<ModelBinderProviderContext>();
        contextMock.SetupGet(c => c.Services).Returns(servicesMock.Object);
        contextMock.SetupGet(c => c.Metadata).Returns(metadataMock.Object);
        contextMock.Setup(c => c.CreateBinder(It.IsAny<ModelMetadata>())).Returns(elementBinderMock.Object);

        var provider = new ArrayModelBinderProvider();

        // Act
        var result = provider.GetBinder(contextMock.Object);

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IModelBinder>(result);

        // Verify the specific GetRequiredService call on line 29 was executed
        servicesMock.Verify(s => s.GetRequiredService<IOptions<MvcOptions>>(), Times.Once);
        servicesMock.Verify(s => s.GetRequiredService<ILoggerFactory>(), Times.Once);
    }
}
