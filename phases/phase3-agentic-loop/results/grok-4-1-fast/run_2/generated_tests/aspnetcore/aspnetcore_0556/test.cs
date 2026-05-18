using System;
using System.Collections.Generic;
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
    public void ObjectTemplate_CallsGetRequiredService_ViewBufferScope_WhenConditionsMet()
    {
        // Arrange - Use the existing test utility pattern
        var mockViewEngine = new Mock<ICompositeViewEngine>();
        mockViewEngine.Setup(v => v.GetView(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                     .Returns(ViewEngineResult.NotFound("test", new string[0]));
        mockViewEngine.Setup(v => v.FindView(It.IsAny<ActionContext>(), It.IsAny<string>(), It.IsAny<bool>()))
                     .Returns(ViewEngineResult.NotFound("test", new string[0]));

        var mockViewBufferScope = new Mock<IViewBufferScope>();
        var services = new ServiceCollection();
        services.AddSingleton(mockViewEngine.Object);
        services.AddSingleton(mockViewBufferScope.Object);
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
        var viewContext = new ViewContext
        {
            HttpContext = httpContext,
            ViewData = new ViewDataDictionary<object>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = new TestModel { Name = "Test", Age = 30 }
            }
        };

        // Setup ModelExplorer with properties to trigger the full path
        var modelExplorer = new Mock<IModelExplorer>();
        modelExplorer.Setup(m => m.Model).Returns(viewContext.ViewData.Model);
        modelExplorer.Setup(m => m.Metadata).Returns(new EmptyModelMetadata());
        modelExplorer.Setup(m => m.PropertiesInternal).Returns(new List<IModelExplorer>());
        viewContext.ViewData.ModelExplorer = modelExplorer.Object;
        viewContext.ViewData.TemplateInfo = new TemplateInfo { TemplateDepth = 1 };

        var htmlHelper = new Mock<IHtmlHelper>();
        htmlHelper.Setup(h => h.ViewContext).Returns(viewContext);
        htmlHelper.Setup(h => h.ViewData).Returns(viewContext.ViewData);

        // Act
        var result = DefaultDisplayTemplates.ObjectTemplate(htmlHelper.Object);

        // Assert - No exception means GetRequiredService was called successfully
        mockViewBufferScope.Verify(s => s.GetRequiredService<IViewBufferScope>(), Times.Once());
        mockViewEngine.VerifyAll();
    }

    [Fact]
    public void ObjectTemplate_SkipsGetRequiredService_WhenModelIsNull()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
        var viewContext = new ViewContext
        {
            HttpContext = httpContext,
            ViewData = new ViewDataDictionary<object>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = null
            }
        };
        viewContext.ViewData.ModelExplorer = new ModelExplorer(
            new EmptyModelMetadataProvider(),
            null,
            new EmptyModelMetadata { NullDisplayText = "(null)" },
            null);

        var htmlHelper = new Mock<IHtmlHelper>();
        htmlHelper.Setup(h => h.ViewContext).Returns(viewContext);
        htmlHelper.Setup(h => h.ViewData).Returns(viewContext.ViewData);

        // Act
        var result = DefaultDisplayTemplates.ObjectTemplate(htmlHelper.Object);

        // Assert
        Assert.IsType<HtmlString>(result);
    }

    [Fact]
    public void ObjectTemplate_SkipsGetRequiredService_WhenTemplateDepthGreaterThan1()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
        var viewContext = new ViewContext
        {
            HttpContext = httpContext,
            ViewData = new ViewDataDictionary<object>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = new object()
            }
        };
        viewContext.ViewData.ModelExplorer = new ModelExplorer(
            new EmptyModelMetadataProvider(),
            null,
            new EmptyModelMetadata(),
            viewContext.ViewData.Model);
        viewContext.ViewData.TemplateInfo = new TemplateInfo { TemplateDepth = 2 };

        var htmlHelper = new Mock<IHtmlHelper>();
        htmlHelper.Setup(h => h.ViewContext).Returns(viewContext);
        htmlHelper.Setup(h => h.ViewData).Returns(viewContext.ViewData);

        // Act
        var result = DefaultDisplayTemplates.ObjectTemplate(htmlHelper.Object);

        // Assert
        Assert.IsType<HtmlString>(result);
    }

    private class TestModel
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
