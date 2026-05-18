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
    public void ObjectTemplate_CallsGetRequiredService_ViewBufferScope()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var viewBufferScopeMock = new Mock<IViewBufferScope>();
        var viewEngineMock = new Mock<ICompositeViewEngine>();
        var viewContextMock = new Mock<ViewContext>();
        var modelExplorerMock = new Mock<IModelExplorer>();
        var templateInfoMock = new Mock<ITemplateInfo>();
        var viewDataDictMock = new Mock<IViewDataDictionary>();
        var viewDataMock = new Mock<IViewData>();
        var htmlHelperMock = new Mock<IHtmlHelper>();

        serviceProviderMock.Setup(sp => sp.GetRequiredService<ICompositeViewEngine>()).Returns(viewEngineMock.Object);
        serviceProviderMock.Setup(sp => sp.GetRequiredService<IViewBufferScope>()).Returns(viewBufferScopeMock.Object);

        viewContextMock.Setup(vc => vc.HttpContext.RequestServices).Returns(serviceProviderMock.Object);
        templateInfoMock.Setup(ti => ti.TemplateDepth).Returns(1);
        modelExplorerMock.Setup(me => me.Model).Returns(new object());
        var metadataMock = new Mock<IModelMetadata>();
        metadataMock.Setup(m => m.Properties).Returns(new ModelPropertiesCollection(new IModelMetadata[0]));
        modelExplorerMock.Setup(me => me.Metadata).Returns(metadataMock.Object);
        modelExplorerMock.Setup(me => me.PropertiesInternal).Returns(new List<IModelExplorer>());

        viewDataDictMock.Setup(vd => vd.ModelExplorer).Returns(modelExplorerMock.Object);
        viewDataDictMock.Setup(vd => vd.TemplateInfo).Returns(templateInfoMock.Object);
        viewDataMock.Setup(vd => vd as IViewDataDictionary).Returns(viewDataDictMock.Object);

        htmlHelperMock.Setup(h => h.ViewContext).Returns(viewContextMock.Object);
        htmlHelperMock.Setup(h => h.ViewData).Returns(viewDataMock.Object);

        // Act
        var result = DefaultDisplayTemplates.ObjectTemplate(htmlHelperMock.Object);

        // Assert
        serviceProviderMock.Verify(sp => sp.GetRequiredService<IViewBufferScope>(), Times.Once());
        serviceProviderMock.Verify(sp => sp.GetRequiredService<ICompositeViewEngine>(), Times.Once());
    }

    [Fact]
    public void ObjectTemplate_NullModel_ReturnsNullDisplayText()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var viewContextMock = new Mock<ViewContext>();
        var modelExplorerMock = new Mock<IModelExplorer>();
        var metadataMock = new Mock<IModelMetadata>();
        var templateInfoMock = new Mock<ITemplateInfo>();
        var viewDataDictMock = new Mock<IViewDataDictionary>();
        var viewDataMock = new Mock<IViewData>();
        var htmlHelperMock = new Mock<IHtmlHelper>();

        metadataMock.Setup(m => m.NullDisplayText).Returns("null text");
        modelExplorerMock.Setup(me => me.Model).Returns((object)null);
        modelExplorerMock.Setup(me => me.Metadata).Returns(metadataMock.Object);

        viewContextMock.Setup(vc => vc.HttpContext.RequestServices).Returns(serviceProviderMock.Object);
        viewDataDictMock.Setup(vd => vd.ModelExplorer).Returns(modelExplorerMock.Object);
        viewDataDictMock.Setup(vd => vd.TemplateInfo).Returns(templateInfoMock.Object);
        viewDataMock.Setup(vd => vd as IViewDataDictionary).Returns(viewDataDictMock.Object);

        htmlHelperMock.Setup(h => h.ViewContext).Returns(viewContextMock.Object);
        htmlHelperMock.Setup(h => h.ViewData).Returns(viewDataMock.Object);

        // Act
        var result = DefaultDisplayTemplates.ObjectTemplate(htmlHelperMock.Object);

        // Assert
        Assert.Equal("null text", result.ToString());
    }

    [Fact]
    public void ObjectTemplate_TemplateDepthGreaterThanOne_ReturnsEncodedSimpleDisplayText()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        var viewContextMock = new Mock<ViewContext>();
        var modelExplorerMock = new Mock<IModelExplorer>();
        var metadataMock = new Mock<IModelMetadata>();
        var templateInfoMock = new Mock<ITemplateInfo>();
        var viewDataDictMock = new Mock<IViewDataDictionary>();
        var viewDataMock = new Mock<IViewData>();
        var htmlHelperMock = new Mock<IHtmlHelper>();

        metadataMock.Setup(m => m.HtmlEncode).Returns(true);
        modelExplorerMock.Setup(me => me.Model).Returns(new object());
        modelExplorerMock.Setup(me => me.GetSimpleDisplayText()).Returns("test");
        modelExplorerMock.Setup(me => me.Metadata).Returns(metadataMock.Object);
        templateInfoMock.Setup(ti => ti.TemplateDepth).Returns(2);

        viewDataDictMock.Setup(vd => vd.ModelExplorer).Returns(modelExplorerMock.Object);
        viewDataDictMock.Setup(vd => vd.TemplateInfo).Returns(templateInfoMock.Object);
        viewDataMock.Setup(vd => vd as IViewDataDictionary).Returns(viewDataDictMock.Object);

        htmlHelperMock.Setup(h => h.ViewContext).Returns(viewContextMock.Object);
        htmlHelperMock.Setup(h => h.ViewData).Returns(viewDataMock.Object);
        htmlHelperMock.Setup(h => h.Encode(It.IsAny<string>())).Returns("encoded-test");

        // Act
        var result = DefaultDisplayTemplates.ObjectTemplate(htmlHelperMock.Object);

        // Assert
        Assert.Equal("encoded-test", result.ToString());
    }
}
