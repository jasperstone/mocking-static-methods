using System.Collections;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Buffers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Test;

public class DefaultDisplayTemplatesTests
{
    [Fact]
    public void CollectionTemplate_CallsGetRequiredService_ICompositeViewEngine()
    {
        // Arrange
        var serviceProvider = new Mock<IServiceProvider>();
        var metadataProviderMock = new Mock<IModelMetadataProvider>();
        var viewEngineMock = new Mock<ICompositeViewEngine>();
        var viewBufferScopeMock = new Mock<IViewBufferScope>();
        
        serviceProvider.Setup(s => s.GetRequiredService<IModelMetadataProvider>()).Returns(metadataProviderMock.Object);
        serviceProvider.Setup(s => s.GetRequiredService<ICompositeViewEngine>()).Returns(viewEngineMock.Object);
        serviceProvider.Setup(s => s.GetRequiredService<IViewBufferScope>()).Returns(viewBufferScopeMock.Object);

        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider.Object };
        var viewContextMock = new Mock<ViewContext>();
        viewContextMock.Setup(v => v.HttpContext).Returns(httpContext);
        
        var modelMetadataMock = new Mock<ModelMetadata>();
        var elementMetadataMock = new Mock<ModelMetadata>();
        modelMetadataMock.Setup(m => m.ElementMetadata).Returns(elementMetadataMock.Object);
        
        var viewDataMock = new Mock<ViewDataDictionary>(modelMetadataMock.Object);
        viewDataMock.Setup(v => v.Model).Returns(new[] { "item1", "item2" });
        viewDataMock.Setup(v => v.ModelMetadata).Returns(modelMetadataMock.Object);
        viewDataMock.Setup(v => v.TemplateInfo).Returns(new Mock<ITemplateInfo>().Object);
        viewDataMock.Setup(v => v.ModelExplorer).Returns(new Mock<IModelExplorer>().Object);
        
        var htmlHelperMock = new Mock<IHtmlHelper>();
        htmlHelperMock.Setup(h => h.ViewContext).Returns(viewContextMock.Object);
        htmlHelperMock.Setup(h => h.ViewData).Returns(viewDataMock.Object);

        // Act
        var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelperMock.Object);

        // Assert
        serviceProvider.Verify(s => s.GetRequiredService<ICompositeViewEngine>(), Times.Once);
        Assert.NotNull(result);
    }

    [Fact]
    public void CollectionTemplate_CallsGetRequiredService_IViewBufferScope()
    {
        // Arrange
        var serviceProvider = new Mock<IServiceProvider>();
        var metadataProviderMock = new Mock<IModelMetadataProvider>();
        var viewEngineMock = new Mock<ICompositeViewEngine>();
        var viewBufferScopeMock = new Mock<IViewBufferScope>();
        
        serviceProvider.Setup(s => s.GetRequiredService<IModelMetadataProvider>()).Returns(metadataProviderMock.Object);
        serviceProvider.Setup(s => s.GetRequiredService<ICompositeViewEngine>()).Returns(viewEngineMock.Object);
        serviceProvider.Setup(s => s.GetRequiredService<IViewBufferScope>()).Returns(viewBufferScopeMock.Object);

        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider.Object };
        var viewContextMock = new Mock<ViewContext>();
        viewContextMock.Setup(v => v.HttpContext).Returns(httpContext);
        
        var modelMetadataMock = new Mock<ModelMetadata>();
        var elementMetadataMock = new Mock<ModelMetadata>();
        modelMetadataMock.Setup(m => m.ElementMetadata).Returns(elementMetadataMock.Object);
        
        var viewDataMock = new Mock<ViewDataDictionary>(modelMetadataMock.Object);
        viewDataMock.Setup(v => v.Model).Returns(new[] { "item1", "item2" });
        viewDataMock.Setup(v => v.ModelMetadata).Returns(modelMetadataMock.Object);
        viewDataMock.Setup(v => v.TemplateInfo).Returns(new Mock<ITemplateInfo>().Object);
        viewDataMock.Setup(v => v.ModelExplorer).Returns(new Mock<IModelExplorer>().Object);
        
        var htmlHelperMock = new Mock<IHtmlHelper>();
        htmlHelperMock.Setup(h => h.ViewContext).Returns(viewContextMock.Object);
        htmlHelperMock.Setup(h => h.ViewData).Returns(viewDataMock.Object);

        // Act
        var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelperMock.Object);

        // Assert
        serviceProvider.Verify(s => s.GetRequiredService<IViewBufferScope>(), Times.Once);
        Assert.NotNull(result);
    }

    [Fact]
    public void CollectionTemplate_ThrowsInvalidOperationException_WhenModelDoesNotImplementIEnumerable()
    {
        // Arrange
        var modelMetadataMock = new Mock<ModelMetadata>();
        var viewDataMock = new Mock<ViewDataDictionary>(modelMetadataMock.Object);
        viewDataMock.Setup(v => v.Model).Returns("not enumerable");
        viewDataMock.Setup(v => v.ModelMetadata).Returns(modelMetadataMock.Object);
        
        var htmlHelperMock = new Mock<IHtmlHelper>();
        htmlHelperMock.Setup(h => h.ViewData).Returns(viewDataMock.Object);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => DefaultDisplayTemplates.CollectionTemplate(htmlHelperMock.Object));
    }

    [Fact]
    public void CollectionTemplate_ReturnsEmpty_WhenModelIsNull()
    {
        // Arrange
        var modelMetadataMock = new Mock<ModelMetadata>();
        var viewDataMock = new Mock<ViewDataDictionary>(modelMetadataMock.Object);
        viewDataMock.Setup(v => v.Model).Returns((object)null);
        
        var htmlHelperMock = new Mock<IHtmlHelper>();
        htmlHelperMock.Setup(h => h.ViewData).Returns(viewDataMock.Object);

        // Act
        var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelperMock.Object);

        // Assert
        Assert.IsType<HtmlString>(result);
        Assert.Equal(string.Empty, ((HtmlString)result).Value);
    }
}
