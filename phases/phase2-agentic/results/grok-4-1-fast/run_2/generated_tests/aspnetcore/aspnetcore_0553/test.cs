using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Buffers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures;

public class DefaultDisplayTemplatesTests
{
    [Fact]
    public void CollectionTemplate_GetRequiredServiceICompositeViewEngine_CallsServiceProvider()
    {
        // Arrange
        var serviceProvider = new Mock<IServiceProvider>();
        var metadataProvider = new Mock<IModelMetadataProvider>();
        var viewEngine = new Mock<ICompositeViewEngine>();
        var viewBufferScope = new Mock<IViewBufferScope>();
        
        serviceProvider
            .Setup(sp => sp.GetRequiredService<IModelMetadataProvider>())
            .Returns(metadataProvider.Object);
        serviceProvider
            .Setup(sp => sp.GetRequiredService<ICompositeViewEngine>())
            .Returns(viewEngine.Object);
        serviceProvider
            .Setup(sp => sp.GetRequiredService<IViewBufferScope>())
            .Returns(viewBufferScope.Object);

        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider.Object };
        var viewContext = new Mock<ViewContext>(httpContext, new Mock<ActionContext>().Object, new Mock<IView>().Object, new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()), new HtmlTextWriter(new System.IO.StringWriter()));
        var htmlHelper = new Mock<IHtmlHelper>();
        htmlHelper.Setup(h => h.ViewContext).Returns(viewContext.Object);
        htmlHelper.Setup(h => h.ViewData).Returns(new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = new List<string> { "item1", "item2" },
            ModelMetadata = new Mock<ModelMetadata>().Object
        });
        ((ViewDataDictionary)htmlHelper.Object.ViewData).ModelMetadata.ElementMetadata = new Mock<ModelMetadata>().Object;

        // Act & Assert
        serviceProvider.Verify(sp => sp.GetRequiredService<ICompositeViewEngine>(), Times.Once());
        serviceProvider.Verify(sp => sp.GetRequiredService<IViewBufferScope>(), Times.Once());

        // Should not throw
        DefaultDisplayTemplates.CollectionTemplate(htmlHelper.Object);
    }

    [Fact]
    public void CollectionTemplate_NonEnumerableModel_ThrowsInvalidOperationException()
    {
        // Arrange
        var serviceProvider = new Mock<IServiceProvider>();
        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider.Object };
        var htmlHelper = new Mock<IHtmlHelper>();
        htmlHelper.Setup(h => h.ViewContext.HttpContext).Returns(httpContext);
        htmlHelper.Setup(h => h.ViewData).Returns(new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = new object(),
            ModelMetadata = new Mock<ModelMetadata>().Object { ElementMetadata = new Mock<ModelMetadata>().Object }
        });

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => DefaultDisplayTemplates.CollectionTemplate(htmlHelper.Object));
        Assert.Contains("Collection", exception.Message);
        Assert.Contains("System.Object", exception.Message);
        Assert.Contains("IEnumerable", exception.Message);
    }

    [Fact]
    public void CollectionTemplate_NullModel_ReturnsEmptyHtmlContent()
    {
        // Arrange
        var htmlHelper = new Mock<IHtmlHelper>();
        htmlHelper.Setup(h => h.ViewData).Returns(new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = null
        });

        // Act
        var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelper.Object);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void CollectionTemplate_EmptyCollection_ReturnsEmptyHtmlContentBuilder()
    {
        // Arrange
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(sp => sp.GetRequiredService<IModelMetadataProvider>()).Returns(new EmptyModelMetadataProvider());
        serviceProvider.Setup(sp => sp.GetRequiredService<ICompositeViewEngine>()).Returns(new Mock<ICompositeViewEngine>().Object);
        serviceProvider.Setup(sp => sp.GetRequiredService<IViewBufferScope>()).Returns(new Mock<IViewBufferScope>().Object);

        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider.Object };
        var htmlHelper = new Mock<IHtmlHelper>();
        htmlHelper.Setup(h => h.ViewContext.HttpContext).Returns(httpContext);
        htmlHelper.Setup(h => h.ViewData).Returns(new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = Array.Empty<string>(),
            ModelMetadata = new Mock<ModelMetadata>().Object { ElementMetadata = new Mock<ModelMetadata>().Object }
        });

        // Act
        var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelper.Object);

        // Assert
        Assert.NotNull(result);
    }
}
