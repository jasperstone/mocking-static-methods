using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Buffers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Tests
{
    public class DefaultDisplayTemplatesTests
    {
        [Fact]
        public void ObjectTemplate_Should_ReturnHtmlContent_WhenModelIsNotNull()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockViewEngine = new Mock<ICompositeViewEngine>();
            var mockViewBufferScope = new Mock<IViewBufferScope>();

            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(ICompositeViewEngine)))
                .Returns(mockViewEngine.Object);

            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IViewBufferScope)))
                .Returns(mockViewBufferScope.Object);

            var htmlHelper = new Mock<IHtmlHelper>();
            htmlHelper.Setup(h => h.ViewContext.HttpContext.RequestServices).Returns(mockServiceProvider.Object);

            var modelExplorer = new Mock<ModelExplorer>();
            modelExplorer.Setup(m => m.Model).Returns(new object());
            modelExplorer.Setup(m => m.Metadata.Properties).Returns(new List<ModelExplorer>());

            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                ModelExplorer = modelExplorer.Object,
                TemplateInfo = new TemplateInfo()
            };

            htmlHelper.Setup(h => h.ViewData).Returns(viewData);

            // Act
            var result = DefaultDisplayTemplates.ObjectTemplate(htmlHelper.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<HtmlString>(result);
        }

        [Fact]
        public void ObjectTemplate_Should_ReturnNullDisplayText_WhenModelIsNull()
        {
            // Arrange
            var htmlHelper = new Mock<IHtmlHelper>();
            var modelExplorer = new Mock<ModelExplorer>();
            modelExplorer.Setup(m => m.Model).Returns((object)null);
            modelExplorer.Setup(m => m.Metadata.NullDisplayText).Returns("NullDisplayText");

            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                ModelExplorer = modelExplorer.Object,
                TemplateInfo = new TemplateInfo()
            };

            htmlHelper.Setup(h => h.ViewData).Returns(viewData);

            // Act
            var result = DefaultDisplayTemplates.ObjectTemplate(htmlHelper.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<HtmlString>(result);
            Assert.Equal("NullDisplayText", result.ToString());
        }

        [Fact]
        public void ObjectTemplate_Should_ReturnSimpleDisplayText_WhenTemplateDepthGreaterThanOne()
        {
            // Arrange
            var htmlHelper = new Mock<IHtmlHelper>();
            var modelExplorer = new Mock<ModelExplorer>();
            modelExplorer.Setup(m => m.Model).Returns(new object());
            modelExplorer.Setup(m => m.GetSimpleDisplayText()).Returns("SimpleDisplayText");
            modelExplorer.Setup(m => m.Metadata.HtmlEncode).Returns(false);

            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                ModelExplorer = modelExplorer.Object,
                TemplateInfo = new TemplateInfo { TemplateDepth = 2 }
            };

            htmlHelper.Setup(h => h.ViewData).Returns(viewData);

            // Act
            var result = DefaultDisplayTemplates.ObjectTemplate(htmlHelper.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<HtmlString>(result);
            Assert.Equal("SimpleDisplayText", result.ToString());
        }
    }
}
