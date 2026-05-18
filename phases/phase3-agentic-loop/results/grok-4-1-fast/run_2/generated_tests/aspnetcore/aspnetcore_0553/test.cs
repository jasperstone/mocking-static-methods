using System.Collections;
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
    public void CollectionTemplate_GetRequiredServiceICompositeViewEngine_Coverage()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
        var viewContext = new ViewContext { HttpContext = httpContext };
        
        var metadataProvider = new DefaultModelMetadataProvider();
        var viewData = new ViewDataDictionary(metadataProvider, new ModelStateDictionary())
        {
            Model = new List<string> { "item1", "item2" }
        };
        viewData.ModelMetadata = metadataProvider.GetMetadataForType(typeof(List<string>));
        viewData.TemplateInfo = new TemplateInfo();

        var htmlHelperMock = new Mock<IHtmlHelper>();
        htmlHelperMock.Setup(h => h.ViewContext).Returns(viewContext);
        htmlHelperMock.Setup(h => h.ViewData).Returns(viewData);
        var htmlHelper = htmlHelperMock.Object;

        // Act
        var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelper);

        // Assert - Successfully reached and executed GetRequiredService<ICompositeViewEngine>()
        Assert.NotNull(result);
    }

    [Fact]
    public void CollectionTemplate_ThrowsForNonEnumerable()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider();
        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
        var viewContext = new ViewContext { HttpContext = httpContext };
        
        var metadataProvider = new DefaultModelMetadataProvider();
        var viewData = new ViewDataDictionary(metadataProvider, new ModelStateDictionary())
        {
            Model = "not enumerable"
        };
        viewData.ModelMetadata = metadataProvider.GetMetadataForType(typeof(string));
        viewData.TemplateInfo = new TemplateInfo();

        var htmlHelperMock = new Mock<IHtmlHelper>();
        htmlHelperMock.Setup(h => h.ViewContext).Returns(viewContext);
        htmlHelperMock.Setup(h => h.ViewData).Returns(viewData);
        var htmlHelper = htmlHelperMock.Object;

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => DefaultDisplayTemplates.CollectionTemplate(htmlHelper));
        Assert.Contains("IEnumerable", exception.Message);
    }

    private static IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IModelMetadataProvider, DefaultModelMetadataProvider>();
        services.AddSingleton<ICompositeViewEngine>(provider => new MockCompositeViewEngine());
        services.AddSingleton<IViewBufferScope>(provider => new MockViewBufferScope());
        return services.BuildServiceProvider();
    }
}

internal class MockCompositeViewEngine : ICompositeViewEngine
{
    public ViewEngineResult FindView(ActionContext context, string viewName, bool isMainPage) => ViewEngineResult.Null;
    public ViewEngineResult GetView(string executingFilePath, string viewName, bool isMainPage) => ViewEngineResult.Null;
}

internal class MockViewBufferScope : IViewBufferScope
{
    public IViewBufferScopeRetainer Retainer => new MockRetainer();
}

internal class MockRetainer : IViewBufferScopeRetainer
{
    public void OnCompleted(RenderAsyncScope scope) { }
    public void OnStarting(RenderAsyncScope scope) { }
    public void OnCancelled(RenderAsyncScope scope) { }
}
