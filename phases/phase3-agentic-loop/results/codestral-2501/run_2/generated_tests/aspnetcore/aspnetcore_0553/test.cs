using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
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
        public void CollectionTemplate_ShouldRenderCollection()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockMetadataProvider = new Mock<IModelMetadataProvider>();
            var mockViewEngine = new Mock<ICompositeViewEngine>();
            var mockViewBufferScope = new Mock<IViewBufferScope>();

            mockServiceProvider.Setup(sp => sp.GetRequiredService<IModelMetadataProvider>()).Returns(mockMetadataProvider.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<ICompositeViewEngine>()).Returns(mockViewEngine.Object);
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IViewBufferScope>()).Returns(mockViewBufferScope.Object);

            var htmlHelper = new Mock<IHtmlHelper>();
            htmlHelper.Setup(hh => hh.ViewContext.HttpContext.RequestServices).Returns(mockServiceProvider.Object);
            htmlHelper.Setup(hh => hh.ViewData.ModelMetadata.ElementMetadata).Returns(new ModelMetadata(new EmptyModelMetadataProvider(), new ModelMetadataIdentity(), typeof(string), typeof(string), DisplayName: null));
            htmlHelper.Setup(hh => hh.ViewData.Model).Returns(new List<string> { "Item1", "Item2" });
            htmlHelper.Setup(hh => hh.ViewData.TemplateInfo).Returns(new TemplateInfo());
            htmlHelper.Setup(hh => hh.ViewContext).Returns(new ViewContext { HttpContext = new DefaultHttpContext() });

            // Act
            var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelper.Object);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<HtmlContentBuilder>(result);
        }

        [Fact]
        public void CollectionTemplate_ShouldThrowInvalidOperationException_WhenModelIsNotEnumerable()
        {
            // Arrange
            var htmlHelper = new Mock<IHtmlHelper>();
            htmlHelper.Setup(hh => hh.ViewData.Model).Returns(new object());
            htmlHelper.Setup(hh => hh.ViewContext).Returns(new ViewContext { HttpContext = new DefaultHttpContext() });

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => DefaultDisplayTemplates.CollectionTemplate(htmlHelper.Object));
        }
    }
}
