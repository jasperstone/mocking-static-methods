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

namespace Microsoft.AspNetCore.Mvc.ViewFeatures;

public class DefaultDisplayTemplatesTests
{
    [Fact]
    public void ObjectTemplate_CallsGetRequiredService_ViewBufferScope_WhenTemplateDepthIsOne()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var viewBufferScopeMock = new Mock<IViewBufferScope>();
        var viewEngineMock = new Mock<ICompositeViewEngine>();
        
        var modelMetadataMock = new Mock<IModelMetadata>();
        modelMetadataMock.Setup(m => m.Properties).Returns(new ModelPropertiesCollection(Array.Empty<IModelMetadata>()));
        modelMetadataMock.Setup(m => m.Properties.Count).Returns(0);

        var modelExplorerMock = new Mock<IModelExplorer>();
        modelExplorerMock.Setup(m => m.Model).Returns(new object());
        modelExplorerMock.Setup(m => m.Metadata).Returns(modelMetadataMock.Object);

        var templateInfoMock = new Mock<ITemplateInfo>();
        templateInfoMock.Setup(t => t.TemplateDepth).Returns(1);

        var viewDataMock = new Mock<IViewData>();
        viewDataMock.SetupGet(v => v.ModelExplorer).Returns(modelExplorerMock.Object);
        viewDataMock.SetupGet(v => v.TemplateInfo).Returns(templateInfoMock.Object);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(h => h.RequestServices).Returns(serviceProviderMock.Object);

        var viewContextMock = new Mock<ViewContext>();
        viewContextMock.SetupGet(v => v.HttpContext).Returns(httpContextMock.Object);

        var htmlHelperMock = new Mock<IHtmlHelper>();
        htmlHelperMock.SetupGet(h => h.ViewData).Returns(viewDataMock.Object);
        htmlHelperMock.SetupGet(h => h.ViewContext).Returns(viewContextMock.Object);

        serviceProviderMock.Setup(s => s.GetRequiredService<ICompositeViewEngine>()).Returns(viewEngineMock.Object);
        serviceProviderMock.Setup(s => s.GetRequiredService<IViewBufferScope>()).Returns(viewBufferScopeMock.Object)
                          .Verifiable();

        // Act
        var result = DefaultDisplayTemplates.ObjectTemplate(htmlHelperMock.Object);

        // Assert
        serviceProviderMock.Verify(s => s.GetRequiredService<IViewBufferScope>(), Times.Once());
        Assert.NotNull(result);
    }

    [Fact]
    public void ObjectTemplate_DoesNotCallGetRequiredService_WhenModelIsNull()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var modelMetadataMock = new Mock<IModelMetadata>();
        modelMetadataMock.Setup(m => m.NullDisplayText).Returns("null text");

        var modelExplorerMock = new Mock<IModelExplorer>();
        modelExplorerMock.Setup(m => m.Model).Returns((object)null);
        modelExplorerMock.Setup(m => m.Metadata).Returns(modelMetadataMock.Object);

        var templateInfoMock = new Mock<ITemplateInfo>();
        var viewDataMock = new Mock<IViewData>();
        viewDataMock.SetupGet(v => v.ModelExplorer).Returns(modelExplorerMock.Object);
        viewDataMock.SetupGet(v => v.TemplateInfo).Returns(templateInfoMock.Object);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(h => h.RequestServices).Returns(serviceProviderMock.Object);

        var viewContextMock = new Mock<ViewContext>();
        viewContextMock.SetupGet(v => v.HttpContext).Returns(httpContextMock.Object);

        var htmlHelperMock = new Mock<IHtmlHelper>();
        htmlHelperMock.SetupGet(h => h.ViewData).Returns(viewDataMock.Object);
        htmlHelperMock.SetupGet(h => h.ViewContext).Returns(viewContextMock.Object);

        // Act
        var result = DefaultDisplayTemplates.ObjectTemplate(htmlHelperMock.Object);

        // Assert
        serviceProviderMock.Verify(s => s.GetRequiredService<IViewBufferScope>(), Times.Never());
        Assert.Equal("null text", result.ToString());
    }

    [Fact]
    public void ObjectTemplate_DoesNotCallGetRequiredService_WhenTemplateDepthGreaterThanOne()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var modelMetadataMock = new Mock<IModelMetadata>();
        modelMetadataMock.Setup(m => m.HtmlEncode).Returns(false);

        var modelExplorerMock = new Mock<IModelExplorer>();
        modelExplorerMock.Setup(m => m.Model).Returns(new object());
        modelExplorerMock.Setup(m => m.GetSimpleDisplayText()).Returns("simple text");
        modelExplorerMock.Setup(m => m.Metadata).Returns(modelMetadataMock.Object);

        var templateInfoMock = new Mock<ITemplateInfo>();
        templateInfoMock.Setup(t => t.TemplateDepth).Returns(2);

        var viewDataMock = new Mock<IViewData>();
        viewDataMock.SetupGet(v => v.ModelExplorer).Returns(modelExplorerMock.Object);
        viewDataMock.SetupGet(v => v.TemplateInfo).Returns(templateInfoMock.Object);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(h => h.RequestServices).Returns(serviceProviderMock.Object);

        var viewContextMock = new Mock<ViewContext>();
        viewContextMock.SetupGet(v => v.HttpContext).Returns(httpContextMock.Object);

        var htmlHelperMock = new Mock<IHtmlHelper>();
        htmlHelperMock.SetupGet(h => h.ViewData).Returns(viewDataMock.Object);
        htmlHelperMock.SetupGet(h => h.ViewContext).Returns(viewContextMock.Object);

        // Act
        var result = DefaultDisplayTemplates.ObjectTemplate(htmlHelperMock.Object);

        // Assert
        serviceProviderMock.Verify(s => s.GetRequiredService<IViewBufferScope>(), Times.Never());
        Assert.Equal("simple text", result.ToString());
    }
}
