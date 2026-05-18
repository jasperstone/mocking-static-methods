using System.Collections;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Buffers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Tests;

public class DefaultDisplayTemplatesTests
{
    [Fact]
    public void CollectionTemplate_CallsGetRequiredService_ICompositeViewEngine()
    {
        // Arrange
        var model = new[] { "item1", "item2" };
        var htmlHelper = CreateMockHtmlHelper(model);
        
        // Act
        DefaultDisplayTemplates.CollectionTemplate(htmlHelper.Object);
        
        // Assert
        Mock.Get(htmlHelper.Object.ViewContext.HttpContext.RequestServices)
            .Verify(sp => sp.GetRequiredService<ICompositeViewEngine>(), Times.Once);
    }
    
    [Fact]
    public void CollectionTemplate_CallsGetRequiredService_IViewBufferScope()
    {
        // Arrange
        var model = new[] { "item1", "item2" };
        var htmlHelper = CreateMockHtmlHelper(model);
        
        // Act
        DefaultDisplayTemplates.CollectionTemplate(htmlHelper.Object);
        
        // Assert
        Mock.Get(htmlHelper.Object.ViewContext.HttpContext.RequestServices)
            .Verify(sp => sp.GetRequiredService<IViewBufferScope>(), Times.Once);
    }
    
    [Fact]
    public void CollectionTemplate_CallsGetRequiredService_IModelMetadataProvider()
    {
        // Arrange
        var model = new[] { "item1", "item2" };
        var htmlHelper = CreateMockHtmlHelper(model);
        
        // Act
        DefaultDisplayTemplates.CollectionTemplate(htmlHelper.Object);
        
        // Assert
        Mock.Get(htmlHelper.Object.ViewContext.HttpContext.RequestServices)
            .Verify(sp => sp.GetRequiredService<IModelMetadataProvider>(), Times.AtLeastOnce);
    }
    
    [Fact]
    public void CollectionTemplate_NonEnumerableModel_ThrowsInvalidOperationException()
    {
        // Arrange
        var model = new object();
        var htmlHelper = CreateMockHtmlHelper(model);
        
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => DefaultDisplayTemplates.CollectionTemplate(htmlHelper.Object));
        Assert.Contains("IEnumerable", exception.Message);
    }
    
    [Fact]
    public void CollectionTemplate_NullModel_ReturnsEmptyHtmlContent()
    {
        // Arrange
        var htmlHelper = CreateMockHtmlHelper(null);
        
        // Act
        var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelper.Object);
        
        // Assert
        Assert.NotNull(result);
    }
    
    private static Mock<IHtmlHelper> CreateMockHtmlHelper(object model)
    {
        var mockViewData = new Mock<ViewDataDictionary<EmptyModelMetadataProvider, object>>();
        mockViewData.SetupGet(vd => vd.Model).Returns(model);
        
        var mockModelMetadata = new Mock<ModelMetadata>();
        mockModelMetadata.SetupGet(mm => mm.ElementMetadata).Returns(new Mock<ModelMetadata>().Object);
        mockViewData.SetupGet(vd => vd.ModelMetadata).Returns(mockModelMetadata.Object);
        
        var mockViewContext = new Mock<ViewContext>();
        var mockHttpContext = new Mock<HttpContext>();
        var mockServices = new Mock<IServiceProvider>();
        mockHttpContext.Setup(hc => hc.RequestServices).Returns(mockServices.Object);
        mockViewContext.SetupGet(vc => vc.HttpContext).Returns(mockHttpContext.Object);
        
        var mockHtmlHelper = new Mock<IHtmlHelper>();
        mockHtmlHelper.SetupGet(h => h.ViewData).Returns(mockViewData.Object);
        mockHtmlHelper.SetupGet(h => h.ViewContext).Returns(mockViewContext.Object);
        
        return mockHtmlHelper;
    }
}
