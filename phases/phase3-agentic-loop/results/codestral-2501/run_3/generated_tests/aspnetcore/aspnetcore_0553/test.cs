using System;
using System.Collections;
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
        public void CollectionTemplate_ThrowsInvalidOperationException_WhenModelIsNotEnumerable()
        {
            // Arrange
            var htmlHelper = CreateHtmlHelper();
            htmlHelper.ViewData.Model = new object();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => DefaultDisplayTemplates.CollectionTemplate(htmlHelper));
            Assert.Contains("Collection", exception.Message);
        }

        [Fact]
        public void CollectionTemplate_ReturnsHtmlContent_WhenModelIsEnumerable()
        {
            // Arrange
            var htmlHelper = CreateHtmlHelper();
            htmlHelper.ViewData.Model = new List<string> { "Item1", "Item2" };

            // Act
            var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelper);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<HtmlContentBuilder>(result);
        }

        private static IHtmlHelper CreateHtmlHelper()
        {
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IModelMetadataProvider>()).Returns(new Mock<IModelMetadataProvider>().Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ICompositeViewEngine>()).Returns(new Mock<ICompositeViewEngine>().Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IViewBufferScope>()).Returns(new Mock<IViewBufferScope>().Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(hc => hc.RequestServices).Returns(serviceProviderMock.Object);

            var viewContext = new ViewContext
            {
                HttpContext = httpContextMock.Object
            };

            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = null,
                TemplateInfo = new TemplateInfo()
            };

            var htmlHelper = new HtmlHelper(viewContext, viewData);

            return htmlHelper;
        }
    }
}
