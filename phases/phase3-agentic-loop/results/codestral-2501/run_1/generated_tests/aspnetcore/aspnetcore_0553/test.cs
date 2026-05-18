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
        public void CollectionTemplate_ShouldThrowInvalidOperationException_WhenModelIsNotEnumerable()
        {
            // Arrange
            var htmlHelper = CreateMockHtmlHelper(model: new object());

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => DefaultDisplayTemplates.CollectionTemplate(htmlHelper.Object));
            Assert.Contains("must implement IEnumerable", exception.Message);
        }

        [Fact]
        public void CollectionTemplate_ShouldReturnEmptyHtmlContent_WhenModelIsNull()
        {
            // Arrange
            var htmlHelper = CreateMockHtmlHelper(model: null);

            // Act
            var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelper.Object);

            // Assert
            Assert.IsType<HtmlString>(result);
            Assert.Equal(string.Empty, result.ToString());
        }

        [Fact]
        public void CollectionTemplate_ShouldCallGetRequiredService_ForICompositeViewEngineAndIViewBufferScope()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var viewEngineMock = new Mock<ICompositeViewEngine>();
            var viewBufferScopeMock = new Mock<IViewBufferScope>();
            var metadataProviderMock = new Mock<IModelMetadataProvider>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ICompositeViewEngine>())
                .Returns(viewEngineMock.Object);
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IViewBufferScope>())
                .Returns(viewBufferScopeMock.Object);
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IModelMetadataProvider>())
                .Returns(metadataProviderMock.Object);

            var htmlHelper = CreateMockHtmlHelper(model: new List<int> { 1, 2, 3 }, serviceProvider: serviceProviderMock.Object);

            // Act
            DefaultDisplayTemplates.CollectionTemplate(htmlHelper.Object);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<ICompositeViewEngine>(), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IViewBufferScope>(), Times.Once);
        }

        private Mock<IHtmlHelper> CreateMockHtmlHelper(object model, IServiceProvider serviceProvider = null)
        {
            var htmlHelperMock = new Mock<IHtmlHelper>();
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
            viewData.Model = model;
            viewData.TemplateInfo = new TemplateInfo { HtmlFieldPrefix = "prefix" };

            var viewContext = new ViewContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = serviceProvider ?? new Mock<IServiceProvider>().Object
                }
            };

            htmlHelperMock.Setup(h => h.ViewData).Returns(viewData);
            htmlHelperMock.Setup(h => h.ViewContext).Returns(viewContext);

            return htmlHelperMock;
        }
    }
}
