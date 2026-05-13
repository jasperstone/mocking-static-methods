using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public void CollectionTemplate_ThrowsInvalidOperationException_WhenModelIsNotEnumerable()
        {
            // Arrange
            var htmlHelper = CreateHtmlHelperWithModel(new object());
            var serviceProvider = new Mock<IServiceProvider>();
            var httpContext = new DefaultHttpContext { RequestServices = serviceProvider.Object };
            htmlHelper.ViewContext.HttpContext = httpContext;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => DefaultDisplayTemplates.CollectionTemplate(htmlHelper));
            Assert.Equal("The model item passed into the dictionary is of type 'System.Object', but this dictionary requires a model item of type 'System.Collections.IEnumerable'.", exception.Message);
        }

        [Fact]
        public void CollectionTemplate_CallsGetRequiredService_ForICompositeViewEngine()
        {
            // Arrange
            var htmlHelper = CreateHtmlHelperWithModel(new List<string> { "item1", "item2" });
            var serviceProvider = new Mock<IServiceProvider>();
            var httpContext = new DefaultHttpContext { RequestServices = serviceProvider.Object };
            htmlHelper.ViewContext.HttpContext = httpContext;

            var viewEngineMock = new Mock<ICompositeViewEngine>();
            serviceProvider.Setup(sp => sp.GetRequiredService<ICompositeViewEngine>()).Returns(viewEngineMock.Object);

            // Act
            DefaultDisplayTemplates.CollectionTemplate(htmlHelper);

            // Assert
            serviceProvider.Verify(sp => sp.GetRequiredService<ICompositeViewEngine>(), Times.Once);
        }

        [Fact]
        public void CollectionTemplate_CallsGetRequiredService_ForIViewBufferScope()
        {
            // Arrange
            var htmlHelper = CreateHtmlHelperWithModel(new List<string> { "item1", "item2" });
            var serviceProvider = new Mock<IServiceProvider>();
            var httpContext = new DefaultHttpContext { RequestServices = serviceProvider.Object };
            htmlHelper.ViewContext.HttpContext = httpContext;

            var viewBufferScopeMock = new Mock<IViewBufferScope>();
            serviceProvider.Setup(sp => sp.GetRequiredService<IViewBufferScope>()).Returns(viewBufferScopeMock.Object);

            // Act
            DefaultDisplayTemplates.CollectionTemplate(htmlHelper);

            // Assert
            serviceProvider.Verify(sp => sp.GetRequiredService<IViewBufferScope>(), Times.Once);
        }

        private static IHtmlHelper CreateHtmlHelperWithModel(object model)
        {
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model,
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(model?.GetType() ?? typeof(object))
            };

            var viewContext = new ViewContext
            {
                ViewData = viewData
            };

            var htmlHelper = new HtmlHelper(viewContext, new Mock<IHtmlGenerator>().Object);
            return htmlHelper;
        }
    }
}
